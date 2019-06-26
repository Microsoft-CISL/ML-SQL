import os
import sys

from sklearn.ensemble import GradientBoostingClassifier
from python2sql.ml.ml_pipeline import MLPipeline


if __name__ == '__main__':

    # FILE PATHS
    SKLEARN_DIR = os.path.dirname(os.path.abspath(__file__))
    DATA_DIR = os.path.abspath('..')
    full_dataset_file_path = os.path.join(DATA_DIR, "dataset", "HeartAllWithId.csv")
    OUTPUT_DIR = os.path.join(SKLEARN_DIR, "assets", "output")

    # INITIALIZE MACHINE LEARNING PIPELINE
    ml_pipeline = MLPipeline(OUTPUT_DIR)

    # LOAD DATASET
    class_attribute = "Label"
    header = ["Age", "Sex", "Cp", "TrestBps", "Chol", "Fbs", "RestEcg", "Thalac", "Exang", "OldPeak", "Slope", "Ca",
              "Thal", "Label", "Id"]
    data, data_header, data_class_labels = ml_pipeline.load_data(full_dataset_file_path, class_attribute, columns=header, sep=';')

    # SPLIT DATASET IN TRAIN AND TEST
    X_train, X_test, y_train, y_test = ml_pipeline.split_data_train_test(data, class_attribute)

    # FEATURE REMOVAL
    drop_features = []
    selected_dataset = ml_pipeline.drop_features_from_data(X_train, drop_features)

    # TRAIN CLASSIFIER
    features = list(selected_dataset.columns.values)
    classifier = GradientBoostingClassifier(max_leaf_nodes=20, n_estimators=100, min_samples_leaf=10, learning_rate=0.2)
    ml_pipeline.train_classifier(selected_dataset, y_train, features, classifier)

    # MAKE PREDICTIONS AND EVALUATE RESULTS
    ml_pipeline.execute_prediction_pipeline(X_test, y_test)

    # TRANSLATE MACHINE LEARNING PIPELINE IN SQL
    #ml_sql_pipepline = MLSQLPipeline(ml_pipeline)
    #print(ml_sql_pipepline.generate_sql_queries())

