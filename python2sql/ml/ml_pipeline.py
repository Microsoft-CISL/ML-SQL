import os
import pickle
import time

import numpy as np
import pandas as pd
from sklearn.model_selection import train_test_split
from sqlalchemy import create_engine


class MLPipeline(object):
    """
    TODO
    """

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
        # TODO: RESTORING TRANSFORMED TRAIN SET FROM FILE?

        transformation_name = type(transformer).__name__

        start_time_transformation = time.time()

        if train:
            #transformer = transformation_type(**transformation_params)
            if target_attribute != None:
                transformer.fit(data[target_attribute])
            else:
                transformer.fit(data)
            transformer.feature_names = list(data.columns.values)

        if target_attribute != None:
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

    def train_classifier(self, x_data, y_data, features, classifier):

        print("[BEGIN] STARTING TRAINING CLASSIFIER...")
        # TODO: RESTORING TRAINED MODEL FROM FILE?

        model_file_path = os.path.join(self.output_dir, 'model.pkl')
        exists = os.path.isfile(model_file_path)

        if exists:  # restore previously generated model

            print("\nRestoring previously generated model...")
            # load the model from disk
            self.classifier = pickle.load(open(model_file_path, 'rb'))
            print("Model restored successfully.\n")

        else:       # train the classifier

            start_time_training = time.time()

            #classifier = classifier_type(**classifier_params)
            classifier.fit(x_data, y_data)
            classifier.feature_names = features
            classifier.class_labels = np.unique(y_data.values)
            self.classifier = classifier

            training_time = time.time() - start_time_training
            print("Training time: {}".format(training_time))

            print("Starting saving models...")
            pickle.dump(classifier, open(model_file_path, 'wb'))
            print("Model saved successfully.")

        print("[END] CLASSIFIER TRAINING COMPLETED.\n")

    def perform_classifier_predictions(self, data, output):

        print("[BEGIN] STARTING CLASSIFIER PREDICTION...")

        start_time_prediction = time.time()

        y_pred = self.classifier.predict(data)
        self.y_pred = y_pred

        if output:
            if output == "console":
                print("console")
                print(self.y_pred)
            elif output == "db":
                print("db")
                chunk_predictions = pd.DataFrame(data={'prediction': y_pred})
                # FIXME: add STRING DB CONNECTION
                engine = create_engine('STRING DB CONNECTION')
                chunk_predictions.to_sql('chunk_predictions', con=engine, if_exists="replace", index=False)
            elif output == "file":
                print("file")
                chunk_predictions = pd.DataFrame(data={'prediction': y_pred})
                chunk_predictions.to_csv(os.path.join(self.output_dir, "chunk_predictions.csv"), index=False)
            else:
                print("ERROR OUTPUT METHOD")

        prediction_time = time.time() - start_time_prediction
        print("Prediction time: {}".format(prediction_time))

        self.prediction_pipeline_times["prediction"] = prediction_time*1000
        self.prediction_step_names.append("prediction")

        print("[END] CLASSIFIFER PREDICTION COMPLETED.\n")

        return y_pred

    def execute_prediction_pipeline(self, x_data, y_data, output=None, transformation_target_attribute=None):

        print("[BEGIN] STARTING PREDICTION PIPELINE...")
        self.prediction_pipeline_times = {}
        start_time = time.time()

        # feature removal ----------------------------------------------------
        transformed_dataset = self.drop_features_from_data(x_data, self.drop_features, train=False)
        # --------------------------------------------------------------------

        # transformation -----------------------------------------------------
        # FIXME: A TARGET ATTRIBUTE FOR EACH TRANSFORMATION
        for transformer in self.transformations:
            transformed_dataset = self.apply_transformation(transformed_dataset, transformer, train=False, target_attribute=transformation_target_attribute)
        # --------------------------------------------------------------------

        # prediction ---------------------------------------------------------
        y_pred = self.perform_classifier_predictions(transformed_dataset, output)
        # --------------------------------------------------------------------

        # classification evalutation -----------------------------------------
        #classifier_name = type(self.classifier).__name__
        #evaluate_binary_classification_results(classifier_name, y_data, y_pred)
        # --------------------------------------------------------------------

        total_time = time.time() - start_time
        print("Total time: {}".format(total_time))
        #self.prediction_pipeline_times["total"] = total_time*1000

        print("[END] PREDICTION PIPELINE COMPLETED.\n")

    def get_classifier(self):
        return self.classifier

    def get_transformations(self):
        return self.transformations

    def get_prediction_pipeline_times(self):
        return self.prediction_pipeline_times

    def get_prediction_step_names(self):
        return self.prediction_step_names
