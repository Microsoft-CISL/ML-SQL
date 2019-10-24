import os
import pandas as pd
import numpy as np
from sklearn.preprocessing import LabelEncoder
from lightning.classification import SDCAClassifier # pip install sklearn-contrib-lightning
from sklearn.linear_model import LogisticRegression
import argparse
from python2sql.ml.ml_pipeline import MLPipeline
from MulticlassClassification.MulticlassClassification_Iris.multiclass_classification_iris_scikit_learn.iris_multiclass_sql_pipeline import IrisMultiClassificationSQLPipeline
from python2sql.ml.utils import evaluate_regression_results
from python2sql.test.test_executor import TestExecutor


class IrisMultiClassificationPipeline(object):
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
        self.class_attribute = "#Label"

        # COMPLETE DATASET
        self.data = pd.read_csv(self.dataset_file_path, sep='\t')
        # self.data["Id"] = range(len(self.data))

        # convert class attribute, which is categorical, to numerical
        labelencoder = LabelEncoder()
        labelencoder.fit(self.data[self.class_attribute])
        class_converted = labelencoder.transform(self.data[self.class_attribute])
        self.data[self.class_attribute] = class_converted

        self.y = self.data[self.class_attribute]
        self.X = self.data.drop([self.class_attribute], axis=1)
        self.data_header = self.data.columns.values
        self.data_class_labels = np.unique(self.y.values)

        # TRAIN
        self.train = pd.read_csv(self.train_dataset_file_path, sep='\t')
        # self.train["Id"] = range(len(self.train))

        # convert class attribute, which is categorical, to numerical
        class_converted = labelencoder.transform(self.train[self.class_attribute])
        self.train[self.class_attribute] = class_converted

        self.y_train = self.train[self.class_attribute]
        self.X_train = self.train.drop([self.class_attribute], axis=1)

        # TEST
        self.test = pd.read_csv(self.test_dataset_file_path, sep='\t')
        # self.test["Id"] = range(len(self.test))

        # convert class attribute, which is categorical, to numerical
        class_converted = labelencoder.transform(self.test[self.class_attribute])
        self.test[self.class_attribute] = class_converted

        self.y_test = self.test[self.class_attribute]
        self.X_test = self.test.drop([self.class_attribute], axis=1)
        # ---------------------------------------------------------------------------------------------

        # FEATURE REMOVAL
        self.drop_features = ["Id"]
        selected_dataset = self.ml_pipeline.drop_features_from_data(self.X_train, self.drop_features)

        # TRAIN CLASSIFICATION MODEL
        # classifier = SDCAClassifier() # non restituisce le probabilità
        # FIXME: increasing the regularization parameter and optionally the max iter parameter it is possible to achieve
        # better log loss
        classifier = LogisticRegression(penalty='l2', C=100, multi_class='multinomial', solver='saga', random_state=24, max_iter=1000)
        self.ml_pipeline.train_classifier(selected_dataset, self.y_train, selected_dataset.columns.values, classifier, save_restore_model=restore_save_trained_model)

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

        self.predictions, self.probabilities, self.scores = self.ml_pipeline.execute_prediction_pipeline(X, y,
                                                                                     ml_task_type="multi-classification")

        return self.predictions, self.probabilities, self.scores

    def predict_sql(self):

        # SAVE PREDICTION RESULTS INTO FILE
        # self.ml_pipeline.execute_prediction_pipeline(self.X_test, self.y_test, output="file")

        # TRANSLATE MACHINE LEARNING PIPELINE IN SQL
        table_name = "iris"
        db_data = self.data.drop(self.drop_features[:-1], axis=1)  # i mantain in the data the id column
        ml_sql_pipepline = IrisMultiClassificationSQLPipeline(db_data, table_name, self.ml_pipeline, self.predictions,
                                                          self.probabilities, self.scores)
        sql_queries = ml_sql_pipepline.generate_sql_queries()
        sql_predictions = ml_sql_pipepline.perform_query(sql_queries[-1], type="effectiveness")

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

    iris_pipeline = IrisMultiClassificationPipeline("IRIS", "iris-full_with_id.txt", "iris-train_with_id.txt", "iris-test_with_id.txt")
    test_executor = TestExecutor(iris_pipeline, ml_task="multi-classification", sep='\t')

    if test_method == 'sklearn_effectiveness':
        test_executor.evaluate_sklearn_pipeline_effectiveness()
    elif test_method == 'sklearn_efficiency':
        test_executor.evaluate_sklearn_pipeline_chunk_performance()
    elif test_method == 'sklearn_sql_comparison':
        test_executor.compare_sql_sklearn_predictions()
    else:
        raise ValueError(
            "Test method not available ({}). The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")