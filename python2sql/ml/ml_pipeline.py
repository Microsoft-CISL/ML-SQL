import os
import pickle
import time

import numpy as np
import pandas as pd
from sklearn.model_selection import train_test_split
from sqlalchemy import create_engine
from .utils import evaluate_binary_classification_results, evaluate_regression_results, evaluate_multi_classification_results


class MLPipeline(object):

    def __init__(self, output_dir):
        self.output_dir = output_dir
        self.drop_features = []
        self.original_data = None
        self.original_header = None
        self.classifier = None
        self.y_pred = None
        self.transformations = []
        self.prediction_pipeline_times = {}
        self.prediction_step_names = []
        #engine = create_engine('mysql+pymysql://root:ndulsp+92+pgnll@localhost/ml_sql_test')
        #self.connection = engine.connect()

    def load_data(self, data_file, class_attribute, columns=None, sep=','):

        print("[BEGIN] STARTING DATA LOADING...")

        if columns:
            data = pd.read_csv(data_file, header=None, names=columns, sep=sep)
        else:
            data = pd.read_csv(data_file, sep=sep)

        data_header = data.columns.values
        data_class_labels = np.unique(data[class_attribute].values)

        print("Dataset:")
        print("\tNum. rows: {}".format(data.shape[0]))
        print("\tNum. features: {}".format(data.shape[1]))

        print("[END] DATA LOADING COMPLETED.\n")

        return data, data_header, data_class_labels

    def split_data_train_test(self, data, class_attribute, test_size=0.2):

        print("[BEGIN] STARTING SPLITTING DATASET IN TRAIN AND TEST...")

        y = data[class_attribute]
        X = data.drop([class_attribute], axis=1)
        X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=test_size, random_state=42)

        train_dataset = pd.concat([X_train, y_train], axis=1)
        test_dataset = pd.concat([X_test, y_test], axis=1)
        print("Train:")
        print("\tNum. rows: {}".format(train_dataset.shape[0]))
        print("\tNum. features: {}".format(train_dataset.shape[1]))
        print("Test:")
        print("\tNum. rows: {}".format(test_dataset.shape[0]))
        print("\tNum. features: {}".format(test_dataset.shape[1]))

        # save train and test sets into the disk
        # train_dataset.to_csv("{}{}".format(self.output_dir, "train.csv"), index=False)
        # test_dataset.to_csv("{}{}".format(self.output_dir, "test.csv"), index=False)


        print("[END] SPLITTING DATASET IN TRAIN AND TEST COMPLETED.\n")

        return X_train, X_test, y_train, y_test

    def drop_features_from_data(self, data, drop_features, train=True):

        print("[BEGIN] STARTING REMOVING FEATURES...")

        start_time_feature_removal = time.time()

        selected_dataset = data.drop(drop_features, axis=1)
        self.drop_features = drop_features
        print("Data (after feature removal):")
        print("\tNum. rows: {}".format(selected_dataset.shape[0]))
        print("\tNum. features: {}".format(selected_dataset.shape[1]))

        feature_removal_time = time.time() - start_time_feature_removal
        print("Feature removal time: {}".format(feature_removal_time))

        if not train:
            self.prediction_pipeline_times["feature removal"] = feature_removal_time*1000
            self.prediction_step_names.append("feature removal")

        print("[END] FEATURE REMOVAL COMPLETED.\n")

        return selected_dataset

    def apply_transformation(self, data, transformer, train=True, target_attribute=None):

        print("[BEGIN] STARTING DATA SET TRANSFORMATION...")

        transformation_name = type(transformer).__name__

        start_time_transformation = time.time()

        if train:
            if target_attribute != None:
                data[target_attribute] = data[target_attribute].astype('str')
                transformer.fit(data[target_attribute])
            else:
                transformer.fit(data)
            transformer.feature_names = list(data.columns.values)

        if target_attribute != None:
            data[target_attribute] = data[target_attribute].astype('str')
            transformed_dataset = transformer.transform(data[target_attribute])
        else:
            transformed_dataset = transformer.transform(data)

        print("Data (after {} transformation):".format(transformation_name))
        print("\tNum. rows: {}".format(transformed_dataset.shape[0]))
        print("\tNum. features: {}".format(transformed_dataset.shape[1]))

        transformation_time = time.time() - start_time_transformation
        print("Transformation time: {}".format(transformation_time))

        if train:
            self.transformations.append(transformer)
        else:
            self.prediction_pipeline_times["{} transformation".format(transformation_name)] = transformation_time*1000
            self.prediction_step_names.append("{} transformation".format(transformation_name))

        # print("Starting saving models...")
        # pickle.dump(classifier, open("{}{}".format(self.output_dir, "model.pkl"), 'wb'))
        # print("Model saved successfully.")

        print("[END] TRAINING SET TRANSFORMATION COMPLETED.\n")

        return transformed_dataset

    def train_regressor(self, x_data, y_data, features, regressor, fit_method=None, save_restore_model=False):
        print("[BEGIN] STARTING TRAINING REGRESSOR...")

        regressor_name = type(regressor).__name__
        model_file_path = os.path.join(self.output_dir, '{}_model.pkl'.format(regressor_name))
        exists = os.path.isfile(model_file_path)

        if exists and save_restore_model:  # restore previously generated model

            print("\nRestoring previously generated model...")
            # load the model from disk
            self.regressor = pickle.load(open(model_file_path, 'rb'))
            print("Model restored successfully.\n")

        else:  # train the classifier

            start_time_training = time.time()

            if not fit_method:
                regressor.fit(x_data, y_data)
            else:
                # FIXME: ho harcodato un modello di regressione in questa linea perché il formato di invocazione del metodo fit non è standard
                regressor = regressor.fit(method=fit_method, maxiter=1000)
            regressor.feature_names = features
            self.regressor = regressor

            training_time = time.time() - start_time_training
            print("Training time: {}".format(training_time))

            if save_restore_model:
                print("Starting saving models...")
                pickle.dump(regressor, open(model_file_path, 'wb'))
                print("Model saved successfully.")

        print("[END] REGRESSOR TRAINING COMPLETED.\n")

        return self.regressor

    def train_classifier(self, x_data, y_data, features, classifier, save_restore_model=False):

        print("[BEGIN] STARTING TRAINING CLASSIFIER...")

        classifier_name = type(classifier).__name__
        model_file_path = os.path.join(self.output_dir, '{}_model.pkl'.format(classifier_name))
        exists = os.path.isfile(model_file_path)

        if exists and save_restore_model:  # restore previously generated model

            print("\nRestoring previously generated model...")
            # load the model from disk
            self.classifier = pickle.load(open(model_file_path, 'rb'))
            print("Model restored successfully.\n")

        else:       # train the classifier

            start_time_training = time.time()

            classifier.fit(x_data, y_data)
            classifier.feature_names = features
            if isinstance(y_data, np.ndarray):
                classifier.class_labels = np.unique(y_data)
            else:
                classifier.class_labels = np.unique(y_data.values)
            self.classifier = classifier

            training_time = time.time() - start_time_training
            print("Training time: {}".format(training_time))

            if save_restore_model:
                print("Starting saving models...")
                pickle.dump(classifier, open(model_file_path, 'wb'))
                print("Model saved successfully.")

        print("[END] CLASSIFIER TRAINING COMPLETED.\n")

        return self.classifier

    def perform_classifier_predictions(self, data, output, y_true, classification_type='binary', db_connection=None):

        print("[BEGIN] STARTING CLASSIFIER PREDICTION...")

        start_time_prediction = time.time()

        y_pred = self.classifier.predict(data)
        try:
            if classification_type == 'binary':
                probs_for_all_classes = self.classifier.predict_proba(data)
                probs_for_predicted_class = []
                for probs in probs_for_all_classes:
                    prob = max(probs)
                    probs_for_predicted_class.append(prob)
            elif classification_type == 'multi':
                probs_for_predicted_class = self.classifier.predict_proba(data)
            else:
                raise ValueError("Only binary and multi classification supported. Use 'binary' or 'multi' parameters only.")
        except AttributeError:
            probs_for_predicted_class = []
            print("The classifier doesn't have the method predict_proba. This variable will be set to an empty list.")

        scores = self.classifier.decision_function(data)
        self.probs = probs_for_predicted_class
        self.scores = scores
        self.y_pred = y_pred

        if output:
            if output == "console":
                print("console")
                print(self.y_pred)
            elif output == "db":
                if db_connection:
                    print("db")
                    chunk_predictions = pd.DataFrame(data={'prediction': y_pred})
                    # chunk_predictions.to_sql('chunk_predictions', con=db_connection, if_exists="replace", index=False, method='multi')
                    #chunk_predictions.to_sql('chunk_predictions', con=db_connection, if_exists="append", index=False,
                    #                         method='multi')
                    chunk_predictions.to_sql('chunk_predictions', con=db_connection, if_exists="append", index=False)
            elif output == "file":
                print("file")
                chunk_predictions = pd.DataFrame(data={'prediction': y_pred, 'true': y_true})
                chunk_predictions.to_csv(os.path.join(self.output_dir, "chunk_predictions_new.csv"), index=False)
            else:
                print("ERROR OUTPUT METHOD")
                exit(1)

        prediction_time = time.time() - start_time_prediction
        print("Prediction time: {}".format(prediction_time))

        self.prediction_pipeline_times["prediction"] = prediction_time*1000
        self.prediction_step_names.append("prediction")

        print("[END] CLASSIFIFER PREDICTION COMPLETED.\n")

        return y_pred, probs_for_predicted_class, scores

    def perform_regression_predictions(self, data, output, y_true, db_connection=None):

        print("[BEGIN] STARTING REGRESSION PREDICTION...")

        start_time_prediction = time.time()

        y_pred = self.regressor.predict(data)
        # no probability in regression task
        probs = None
        self.y_pred = y_pred
        # set the scores equals to regression prediction
        self.scores = y_pred
        self.probs = probs

        if output:
            if output == "console":
                print("console")
                print(self.y_pred)
            elif output == "db":
                if db_connection:
                    print("db")
                    chunk_predictions = pd.DataFrame(data={'prediction': y_pred})
                    # chunk_predictions.to_sql('chunk_predictions', con=db_connection, if_exists="replace", index=False, method='multi')
                    #chunk_predictions.to_sql('chunk_predictions', con=db_connection, if_exists="append", index=False,
                    #                         method='multi')
                    chunk_predictions.to_sql('chunk_predictions', con=db_connection, if_exists="append", index=False)
            elif output == "file":
                print("file")
                chunk_predictions = pd.DataFrame(data={'prediction': y_pred, 'true': y_true})
                chunk_predictions.to_csv(os.path.join(self.output_dir, "chunk_predictions_new.csv"), index=False)
            else:
                print("ERROR OUTPUT METHOD")
                exit(1)

        prediction_time = time.time() - start_time_prediction
        print("Prediction time: {}".format(prediction_time))

        self.prediction_pipeline_times["prediction"] = prediction_time*1000
        self.prediction_step_names.append("prediction")

        print("[END] REGRESSION PREDICTION COMPLETED.\n")

        return y_pred, self.probs, y_pred

    def execute_prediction_pipeline(self, x_data, y_data, output=None, transformation_target_attribute=None, ml_task_type="classification", evaluate_model=True, db_connection=None):

        print("[BEGIN] STARTING PREDICTION PIPELINE...")
        self.prediction_pipeline_times = {}
        start_time = time.time()

        # feature removal ----------------------------------------------------
        if len(self.drop_features) > 0:
            transformed_dataset = self.drop_features_from_data(x_data, self.drop_features, train=False)
        else:
            transformed_dataset = x_data
            # --------------------------------------------------------------------

        # transformation -----------------------------------------------------
        for transformer in self.transformations:
            transformed_dataset = self.apply_transformation(transformed_dataset, transformer, train=False, target_attribute=transformation_target_attribute)
        # --------------------------------------------------------------------
        self.last_transformed_dataset = transformed_dataset

        if ml_task_type == "classification":

            # prediction ---------------------------------------------------------
            y_pred, probs, scores = self.perform_classifier_predictions(transformed_dataset, output, y_data, db_connection=db_connection)
            # --------------------------------------------------------------------

            # classification evaluation -----------------------------------------
            classifier_name = type(self.classifier).__name__
            if evaluate_model:
                evaluate_binary_classification_results(classifier_name, y_data, y_pred, self.output_dir)
            # --------------------------------------------------------------------

        elif ml_task_type == "regression":

            # prediction ---------------------------------------------------------
            y_pred, probs, scores = self.perform_regression_predictions(transformed_dataset, output, y_data, db_connection=db_connection)
            # --------------------------------------------------------------------

            # regression evalutation ---------------------------------------------
            regressor_name = type(self.regressor).__name__
            if evaluate_model:
                evaluate_regression_results(regressor_name, y_data, y_pred, self.output_dir)
            # --------------------------------------------------------------------

        elif ml_task_type == "multi-classification":

            # prediction ---------------------------------------------------------
            y_pred, probs, scores = self.perform_classifier_predictions(transformed_dataset, output, y_data, classification_type='multi', db_connection=db_connection)
            # --------------------------------------------------------------------

            # classification evalutation -----------------------------------------
            classifier_name = type(self.classifier).__name__
            if evaluate_model:
                evaluate_multi_classification_results(classifier_name, y_data, y_pred, probs, self.output_dir)

            # --------------------------------------------------------------------
        else:
            raise ValueError("Wrong machine learning task type.")

        total_time = time.time() - start_time
        print("Total time: {}".format(total_time))
        #self.prediction_pipeline_times["total"] = total_time*1000

        print("[END] PREDICTION PIPELINE COMPLETED.\n")

        return y_pred, probs, scores

    def get_classifier(self):
        return self.classifier

    def get_transformations(self):
        return self.transformations

    def get_prediction_pipeline_times(self):
        return self.prediction_pipeline_times

    def get_prediction_step_names(self):
        return self.prediction_step_names

    def get_regressor(self):
        return self.regressor

    def set_regressor(self, regressor):
        self.regressor = regressor

    def get_last_transformed_dataset(self):
        return self.last_transformed_dataset
