import os
import pandas as pd
import numpy as np
import argparse
import time
from python2sql.ml.utils import evaluate_regression_results
from sqlalchemy import create_engine
import pickle
import statsmodels.api as sm
from sklearn.linear_model import LogisticRegression
from sklearn.linear_model import SGDRegressor

from python2sql.ml.ml_pipeline import MLPipeline
from Regression.Regression_BikeSharingDemand.regression_bike_sharing_sklearn.bike_sharing_poisson_regression_sql_pipeline import BikeSharingPoissonRegressionSQLPipeline
from python2sql.test.test_executor import TestExecutor


def perform_regression_predictions(data, regressor, output, output_dir, y_true):

    print("[BEGIN] STARTING REGRESSION PREDICTION...")

    start_time_prediction = time.time()

    y_pred = regressor.predict(data)
    #scores = [regressor.score(data, y_true)]
    scores = [0]
    print("score: {}".format(scores))
    probs = None

    if output:
        if output == "console":
            print("console")
            print(y_pred)
        elif output == "db":
            print("db")
            chunk_predictions = pd.DataFrame(data={'prediction': y_pred})
            # FIXME: add STRING DB CONNECTION
            engine = create_engine('STRING DB CONNECTION')
            chunk_predictions.to_sql('chunk_predictions', con=engine, if_exists="replace", index=False)
        elif output == "file":
            print("file")
            chunk_predictions = pd.DataFrame(data={'prediction': y_pred, 'true': y_true})
            chunk_predictions.to_csv(os.path.join(output_dir, "chunk_predictions_new.csv"), index=False)
        else:
            print("ERROR OUTPUT METHOD")

    prediction_time = time.time() - start_time_prediction
    print("Prediction time: {}".format(prediction_time))

    print("[END] REGRESSION PREDICTION COMPLETED.\n")

    return y_pred, probs, scores, prediction_time*1000


def execute_prediction_pipeline(x_data, regressor, output_dir, output=None):
    print("[BEGIN] STARTING PREDICTION PIPELINE...")
    prediction_pipeline_times = {}
    prediction_step_names = []
    start_time = time.time()

    # prediction ---------------------------------------------------------
    y_data = x_data.get_label()
    y_pred, probs, scores, prediction_time = perform_regression_predictions(x_data, regressor, output, output_dir, y_data)
    prediction_pipeline_times["prediction"] = prediction_time
    prediction_step_names.append("prediction")
    # exit(1)
    # --------------------------------------------------------------------
    regressor_name = type(regressor).__name__
    evaluate_regression_results(regressor_name, y_data, y_pred)

    total_time = time.time() - start_time
    print("Total time: {}".format(total_time))
    # self.prediction_pipeline_times["total"] = total_time*1000

    print("[END] PREDICTION PIPELINE COMPLETED.\n")

    return y_pred, probs, scores


def train_regressor(xgtrain, regressor, params, num_rounds, watchlist, output_dir):

    print("[BEGIN] STARTING TRAINING CLASSIFIER...")

    model_file_path = os.path.join(output_dir, 'model.pkl')
    exists = os.path.isfile(model_file_path)

    if exists:  # restore previously generated model

        print("\nRestoring previously generated model...")
        # load the model from disk
        regressor = pickle.load(open(model_file_path, 'rb'))
        print("Model restored successfully.\n")

    else:       # train the classifier

        start_time_training = time.time()

        regressor = regressor.train(params, xgtrain, num_rounds, watchlist, early_stopping_rounds=1000)

        training_time = time.time() - start_time_training
        print("Training time: {}".format(training_time))

        print("Starting saving models...")
        pickle.dump(regressor, open(model_file_path, 'wb'))
        print("Model saved successfully.")

    print("[END] CLASSIFIER TRAINING COMPLETED.\n")

    return regressor


class BikeRegressionPoissonPipeline(object):
    def __init__(self, name, dataset_file, train_file, test_file, out_dir=None):
        self.name = name
        SKLEARN_DIR = os.path.dirname(os.path.abspath(__file__))
        DATA_DIR = os.path.abspath('..')
        self.dataset_file_path = os.path.join(DATA_DIR, "dataset", dataset_file)
        self.train_dataset_file_path = os.path.join(DATA_DIR, "dataset", train_file)
        self.test_dataset_file_path = os.path.join(DATA_DIR, "dataset", test_file)
        self.OUTPUT_DIR = os.path.join(SKLEARN_DIR, "assets", "output")

        # INITIALIZE MACHINE LEARNING PIPELINE
        self.ml_pipeline = MLPipeline(self.OUTPUT_DIR)

    def transform_and_fit(self, restore_save_trained_model=False):

        # LOAD DATASET --------------------------------------------------------------------------------
        self.class_attribute = "cnt"

        # COMPLETE DATASET
        self.data = pd.read_csv(self.dataset_file_path)
        self.y = self.data[self.class_attribute]
        self.X = self.data.drop([self.class_attribute], axis=1)
        self.data_header = self.data.columns.values
        self.data_class_labels = np.unique(self.y.values)

        # TRAIN
        self.train = pd.read_csv(self.train_dataset_file_path)
        self.y_train = self.train[self.class_attribute]
        self.X_train = self.train.drop([self.class_attribute], axis=1)

        # TEST
        self.test = pd.read_csv(self.test_dataset_file_path)
        self.y_test = self.test[self.class_attribute]
        self.X_test = self.test.drop([self.class_attribute], axis=1)
        # ---------------------------------------------------------------------------------------------

        # FEATURE REMOVAL
        self.drop_features = ["instant", "dteday", "casual", "registered", "Id"]
        selected_dataset = self.ml_pipeline.drop_features_from_data(self.X, self.drop_features)

        # TRAIN REGRESSION MODEL
        # attempt to use poisson regressor from statsmodels library
        # self.poisson_mod = sm.Poisson(self.y.values, selected_dataset.values)
        # self.ml_pipeline.train_regressor(selected_dataset, self.y, selected_dataset.columns.values, self.poisson_mod,
        #                            fit_method='lbfgs', save_restore_model=restore_save_trained_model)

        # FIXME: this model can be improved, but i searched for the model that produced the most similar results with
        # respect ML.NET
        regressor = SGDRegressor(random_state=3)
        self.ml_pipeline.train_regressor(selected_dataset, self.y, selected_dataset.columns.values, regressor,
                                         save_restore_model=restore_save_trained_model)

    def predict(self, validation_data='test'):
        # MAKE PREDICTIONS AND EVALUATE RESULTS

        if validation_data == 'test':
            X = self.X_test
            y = self.y_test
        elif validation_data == 'all':
            X = self.X
            y = self.y
        else:
            raise ValueError("Invalid value for validation_data parameter. Use 'test' or 'all'.")

        self.predictions, self.probabilities, self.scores = self.ml_pipeline.execute_prediction_pipeline(X, y, ml_task_type="regression")

        # extracting parameters from statsmodels poisson model
        # params = self.ml_pipeline.get_regressor().params
        # self.scores = self.poisson_mod.hessian_factor(params)

        return self.predictions, self.probabilities, self.scores

    def predict_sql(self):

        table_name = "bike_sharing_Lbfgs"
        db_data = self.data.drop(self.drop_features[:-1], axis=1)  # i mantain in the data the id column
        ml_sql_pipepline = BikeSharingPoissonRegressionSQLPipeline(db_data, table_name, self.ml_pipeline, self.predictions,
                                                                   self.probabilities, self.scores)
        sql_queries = ml_sql_pipepline.generate_sql_queries()
        sql_predictions = ml_sql_pipepline.perform_query(sql_queries[-1])

        return sql_predictions

    def get_data(self):
        return self.data

    def get_data_header(self):
        return self.data_header

    def get_class_labels(self):
        return self.data_class_labels

    def get_ml_pipeline(self):
        return self.ml_pipeline

    def get_dataset_file_path(self):
        return self.dataset_file_path

    def get_class_attribute(self):
        return self.class_attribute

    def get_output_dir(self):
        return self.OUTPUT_DIR

    def get_y(self):
        return self.y

    def get_name(self):
        return self.name


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description='SKLEARN vs SQL credit card pipeline.')
    parser.add_argument('-test', '--test_method', dest='test_method', type=str, action='store',
                        help="The name of test to be performed. The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")
    args = parser.parse_args()

    test_method = args.test_method

    bike_poisson_pipeline = BikeRegressionPoissonPipeline("BIKE_POISSON", "hour_all_with_id.csv", "hour_train_with_id.csv", "hour_test_with_id.csv")
    test_executor = TestExecutor(bike_poisson_pipeline, ml_task="regression")

    if test_method == 'sklearn_effectiveness':
        test_executor.evaluate_sklearn_pipeline_effectiveness()
    elif test_method == 'sklearn_efficiency':
        test_executor.evaluate_sklearn_pipeline_chunk_performance()
    elif test_method == 'sklearn_sql_comparison':
        test_executor.compare_sql_sklearn_predictions()
    else:
        raise ValueError(
            "Test method not available ({}). The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")