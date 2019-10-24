import pandas as pd
# from sklearn.preprocessing import LabelEncoder
from sklearn.preprocessing import OrdinalEncoder
from sklearn.pipeline import Pipeline
from sklearn.compose import ColumnTransformer
from sklearn.impute import SimpleImputer
import seaborn as sns
from sklearn.preprocessing import StandardScaler
import matplotlib.pyplot as plt
from sklearn.model_selection import train_test_split
from sklearn.ensemble import GradientBoostingRegressor
import time
from sklearn.metrics import *
from sklearn.metrics.classification import unique_labels
from math import sqrt
import pandas as pd
from sklearn.preprocessing import MinMaxScaler
from sklearn.preprocessing import StandardScaler
import numpy as np
import argparse
from python2sql.test.test_executor import TestExecutor
import os
import category_encoders as ce # pip install category_encoders
from python2sql.ml.ml_pipeline import MLPipeline
from Regression.Regression_FlightDelay.regression_flight_delay.flight_delay_sql_pipeline import FlightDelaySQLPipeline


def my_train_regressor(x_data, y_data, features, regressor):
    print("[BEGIN] STARTING TRAINING REGRESSOR...")

    start_time_training = time.time()

    regressor.fit(x_data, y_data)

    regressor.feature_names = features

    training_time = time.time() - start_time_training
    print("Training time: {}".format(training_time))

    print("[END] REGRESSOR TRAINING COMPLETED.\n")

    return regressor


def perform_regression_predictions(regressor, data, output, y_true, db_connection=None):
    print("[BEGIN] STARTING REGRESSION PREDICTION...")

    start_time_prediction = time.time()

    y_pred = regressor.predict(data)
    # no probability in regression task
    probs = None
    # set the scores equals to regression prediction
    scores = y_pred

    prediction_time = time.time() - start_time_prediction
    print("Prediction time: {}".format(prediction_time))

    print("[END] REGRESSION PREDICTION COMPLETED.\n")

    return y_pred, probs, y_pred


def _get_regression_metric_scores(y_test, y_pred):
    metric_scores = {}
    # R squared
    r2score = r2_score(y_test, y_pred)
    # mean absolute error
    absolute_loss = mean_absolute_error(y_test, y_pred)
    # mean squared error
    squared_loss = mean_squared_error(y_test, y_pred)
    # root mean squared error
    rms_loss = sqrt(mean_squared_error(y_test, y_pred))

    metric_scores["r2_score"] = r2score
    metric_scores["absolute_loss"] = absolute_loss
    metric_scores["squared_loss"] = squared_loss
    metric_scores["rms_loss"] = rms_loss

    return metric_scores


def _print_regression_metrics_on_console(metric_scores, regression_method):
    r2score = metric_scores["r2_score"]
    absolute_loss = metric_scores["absolute_loss"]
    squared_loss = metric_scores["squared_loss"]
    rms_loss = metric_scores["rms_loss"]

    print("{}".format("*" * 60))
    print("*       Metrics for {} regression model      ".format(regression_method))
    print("*{}".format("-" * 59))
    print("*       R2 Score:      {}".format(r2score))
    print("*       Absolute loss:  {}".format(absolute_loss))
    print("*       Squared loss:  {}".format(squared_loss))
    print("*       RMS loss:  {}".format(rms_loss))
    print("*" * 60)


def evaluate_regression_results(regression_method, y_test, y_pred, output_dir):
    print("[BEGIN] STARTING REGRESSION EVALUTATION...")

    metric_scores = _get_regression_metric_scores(y_test, y_pred)
    _print_regression_metrics_on_console(metric_scores, regression_method)

    print("[END] REGRESSION EVALUTATION COMPLETED.\n")


def execute_prediction_pipeline(x_data, y_data, transformer, regressor, output=None,
                                transformation_target_attribute=None, ml_task_type="classification",
                                evaluate_model=True, db_connection=None):
    print("[BEGIN] STARTING PREDICTION PIPELINE...")
    start_time = time.time()

    # feature removal ----------------------------------------------------
    transformed_dataset = x_data.drop(["Id"], axis=1)
    # --------------------------------------------------------------------

    # transformation -----------------------------------------------------		
    transformed_dataset = transformer.transform(transformed_dataset)
    #transformed_dataset = scaler.transform(transformed_dataset)
    # --------------------------------------------------------------------

    # prediction ---------------------------------------------------------
    y_pred, probs, scores = perform_regression_predictions(regressor, transformed_dataset, output, y_data,
                                                           db_connection=db_connection)
    # --------------------------------------------------------------------

    # regression evalutation ---------------------------------------------
    regressor_name = type(regressor).__name__
    if evaluate_model:
        evaluate_regression_results(regressor_name, y_data, y_pred, "")
    # --------------------------------------------------------------------

    total_time = time.time() - start_time
    print("Total time: {}".format(total_time))

    print("[END] PREDICTION PIPELINE COMPLETED.\n")

    return y_pred, probs, scores


def preprocessing():
    df1 = pd.read_csv("2004.csv")
    df2 = pd.read_csv("2005.csv")
    df3 = pd.read_csv("2006.csv")
    df4 = pd.read_csv("2007.csv")
    df5 = pd.read_csv("2008.csv")
    df = pd.concat([df1, df2, df3, df4, df5])
    print(df.shape)

    # [BEGIN] DESCRIPTIVE STATISTICS -----------------------------------------

    # print(df.info())
    # print(df.head())
    # print(df.describe())
    # sns_plot = sns.heatmap(df.corr(), annot=True)
    # fig = sns_plot.get_figure()
    # fig.savefig("output.png")

    # [END] DESCRIPTIVE STATISTICS -------------------------------------------

    # [BEGIN] MISSING VALUES -------------------------------------------------

    # rows with at least one missing value
    rows_with_na = df[df.isna().any(axis=1)]
    print("Num. rows with missing values: {}".format(len(rows_with_na)))

    # missing value distribution in columns
    print(df.isna().sum())
    mask_col_with_null = df.isna().sum() > 0
    cols_with_null = df.columns[mask_col_with_null].tolist()
    print("Columns with null values: {}".format(cols_with_null))

    # columns with null values analysis in order to understand which replace strategy to be adopted
    numerical_col_with_null = []
    categorical_col_with_null = []
    for col in cols_with_null:
        print(col)
        print(df[col].dtypes)
        print(df[col].unique()[:2])
        print()
        if not (df[col].dtypes == object):
            numerical_col_with_null.append(col)
        else:
            categorical_col_with_null.append(col)

    # removing 'CancellationCode' column which has a large number of missing values
    print("Removing 'CancellationCode' column which has a large number of missing values")
    df = df.drop(['CancellationCode'], axis=1)
    categorical_col_with_null = list(set(categorical_col_with_null) - set(['CancellationCode']))

    # replace null values with mean for numerical feature
    print("Replacing null values with means for numerical feature {}".format(numerical_col_with_null))
    df[numerical_col_with_null] = df[numerical_col_with_null].fillna(df[numerical_col_with_null].mean())

    # replace null values with the most frequent value for categorical feature
    print("Replacing null values with the most frequent values for categorical features {}".format(
        categorical_col_with_null))
    # imp = SimpleImputer(strategy="most_frequent")
    # df[categorical_col_with_null] = imp.fit_transform(df[categorical_col_with_null])
    df[categorical_col_with_null] = df[categorical_col_with_null].fillna(df[categorical_col_with_null].mode().iloc[0])

    # rows with at least one missing value
    rows_with_na = df[df.isna().any(axis=1)]
    print("Num. rows with missing values: {}".format(len(rows_with_na)))

    df["Id"] = range(df.shape[0])

    # df.to_csv("flight_complete_data.csv", index=False)

    # [END] MISSING VALUES ---------------------------------------------------

    attribute_to_predict = "LateAircraftDelay"
    y = df[attribute_to_predict]
    X = df.drop([attribute_to_predict], axis=1)
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.3, random_state=24)
    print(X_train.shape)
    print(y_train.shape)
    print(X_test.shape)
    print(y_test.shape)
    # print(X_train.dtypes)
    # print(df.dtypes)

    print("Starting saving training data...")
    train = X_train.copy()
    train[attribute_to_predict] = y_train
    train.to_csv("flight_delay_train_data.csv", index=False)

    print("Starting saving testing data...")
    test = X_test.copy()
    test[attribute_to_predict] = y_test
    test.to_csv("flight_delay_test_data.csv", index=False)



class FlightDelayRegressionPipeline(object):
    def __init__(self, name, dataset_file, train_file, test_file, out_dir=None):
        self.name = name
        SKLEARN_DIR = os.path.dirname(os.path.abspath(__file__))
        DATA_DIR = os.path.abspath('..')
        # self.dataset_file_path = os.path.join(DATA_DIR, "dataset", dataset_file)
        # self.train_dataset_file_path = os.path.join(DATA_DIR, "dataset", train_file)
        # self.test_dataset_file_path = os.path.join(DATA_DIR, "dataset", test_file)
        self.OUTPUT_DIR = os.path.join(SKLEARN_DIR, "assets", "output")
        #self.dataset_file_path = "/home/matteo/Scrivania/TESTML.NET/Regression/Regression_FlightDelay/FlightDelay/Data/flight_complete_data.csv"
        #self.train_dataset_file_path = "/home/matteo/Scrivania/TESTML.NET/Regression/Regression_FlightDelay/FlightDelay/Data/flight_delay_train_data.csv"
        #self.test_dataset_file_path = "/home/matteo/Scrivania/TESTML.NET/Regression/Regression_FlightDelay/FlightDelay/Data/flight_delay_test_data.csv"
        self.dataset_file_path = "/home/matteo/Scrivania/TESTML.NET/Regression/Regression_FlightDelay/FlightDelay/Data/flight_complete_data_very_small.csv"
        self.train_dataset_file_path = "/home/matteo/Scrivania/TESTML.NET/Regression/Regression_FlightDelay/FlightDelay/Data/flight_delay_train_data_very_small.csv"
        self.test_dataset_file_path = "/home/matteo/Scrivania/TESTML.NET/Regression/Regression_FlightDelay/FlightDelay/Data/flight_delay_test_data_very_small.csv"
        #self.dataset_file_path = "/mnt/flight_delay_data/flight_complete_data.csv"
        #self.train_dataset_file_path = "/mnt/flight_delay_data/flight_delay_train_data.csv"
        #self.test_dataset_file_path = "/mnt/flight_delay_data/flight_delay_test_data.csv"

        # INITIALIZE MACHINE LEARNING PIPELINE
        self.ml_pipeline = MLPipeline(self.OUTPUT_DIR)

    def transform_and_fit(self, restore_save_trained_model=False):
        # LOAD DATASET --------------------------------------------------------------------------------
        self.class_attribute = "LateAircraftDelay"

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
        #self.drop_features = ["UniqueCarrier", "TailNum", "Origin", "Dest", "Id"]
        self.drop_features = ["TailNum", "Id"]
        selected_dataset = self.ml_pipeline.drop_features_from_data(self.X_train, self.drop_features)

        # TRAIN SET TRANSFORMATION
        # Encode the categorical features with one-hot encoder
        # categorical_features = ["UniqueCarrier", "TailNum", "Origin", "Dest"]
        categorical_features = ["UniqueCarrier", "Origin", "Dest"]
        numerical_features = []
        for col in selected_dataset.columns.values:
            if not (selected_dataset[col].dtypes == object):
                numerical_features.append(col)
        #ohe = ce.OneHotEncoder(handle_unknown='value', cols=categorical_features, use_cat_names=True)
        from sklearn.preprocessing import OneHotEncoder
        ohe = OneHotEncoder(handle_unknown='ignore')
        cat_encoder = Pipeline(steps=[
            ('one_hot_encoder', ohe)
        ])

        transformer = ColumnTransformer(
            remainder='passthrough',  # passthough features not listed
            transformers=[
                ('categorical', cat_encoder, categorical_features)
            ])
        transformed_dataset = self.ml_pipeline.apply_transformation(selected_dataset, transformer)
        print(transformed_dataset.shape)

        # TRAIN CLASSIFIER
        features = list(self.ml_pipeline.get_transformations()[0].transformers_[0][1].steps[0][1].get_feature_names())
        features += numerical_features
        regressor = GradientBoostingRegressor(max_leaf_nodes=20, n_estimators=100, min_samples_leaf=10,
                                                learning_rate=0.2, random_state=24)
        self.ml_pipeline.train_regressor(transformed_dataset, self.y_train, features,
                                         regressor, save_restore_model=restore_save_trained_model)

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

        return self.predictions, self.probabilities, self.scores

    def predict_sql(self):

        # The predictions of the SKLEARN's model are equal to the values predicted by SQL.
        # The only difference is that SKLEARN adds to the final score (i.e., the weighted sum of the tree scores) an init
        # score. This part has not been implemented in SQL, but this score has been added to the final query as an offset.
        # retrieving the SKLEARN init score
        from sklearn.utils.validation import check_array
        from sklearn.tree._tree import DTYPE
        X_init = self.ml_pipeline.get_last_transformed_dataset()
        X_init = check_array(X_init, dtype=DTYPE, order="C", accept_sparse='csr')
        init_score = self.ml_pipeline.get_regressor()._raw_predict_init(X_init).ravel()[0]
        self.ml_pipeline.get_regressor().init_score = init_score

        # SAVE PREDICTION RESULTS INTO FILE
        # self.ml_pipeline.execute_prediction_pipeline(X_test, y_test, output="file")

        # TRANSLATE MACHINE LEARNING PIPELINE IN SQL
        table_name = "flight_delay_FastTree"
        db_data = self.data.drop(self.drop_features[:-1], axis=1)  # i mantain in the data the id column
        ml_sql_pipepline = FlightDelaySQLPipeline(db_data, table_name, self.ml_pipeline, self.predictions,
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


if __name__ == "__main__":

    parser = argparse.ArgumentParser(description='SKLEARN vs SQL credit card pipeline.')
    parser.add_argument('-test', '--test_method', dest='test_method', type=str, action='store',
                        help="The name of test to be performed. The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")
    args = parser.parse_args()

    test_method = args.test_method

    flight_delay_pipeline = FlightDelayRegressionPipeline("FLIGHT_DELAY", "flight_delay.csv", "flight_delay_train_data.csv",
                                                           "flight_delay_test_data.csv")
    test_executor = TestExecutor(flight_delay_pipeline, ml_task="regression")

    if test_method == 'sklearn_effectiveness':
        test_executor.evaluate_sklearn_pipeline_effectiveness()
    elif test_method == 'sklearn_efficiency':
        test_executor.evaluate_sklearn_pipeline_chunk_performance()
    elif test_method == 'sklearn_sql_comparison':
        test_executor.compare_sql_sklearn_predictions()
    else:
        raise ValueError(
            "Test method not available ({}). The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")
