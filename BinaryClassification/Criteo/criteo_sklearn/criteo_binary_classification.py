from sklearn.pipeline import Pipeline
from sklearn.compose import ColumnTransformer
from sklearn.model_selection import train_test_split
from sklearn.ensemble import GradientBoostingClassifier
import pandas as pd
import numpy as np
import argparse
from python2sql.test.test_executor import TestExecutor
import os
import category_encoders as ce # pip install category_encoders
from sklearn.preprocessing import OneHotEncoder

from python2sql.ml.ml_pipeline import MLPipeline
from BinaryClassification.Criteo.criteo_sklearn.criteo_sql_pipeline import CriteoSQLPipeline


def preprocessing():
    columns = ["label", "feature_integer1", "feature_integer2", "feature_integer3", "feature_integer4",
               "feature_integer5", "feature_integer6", "feature_integer7", "feature_integer8", "feature_integer9",
               "feature_integer10", "feature_integer11", "feature_integer12", "feature_integer13",
               "categorical_feature1", "categorical_feature2", "categorical_feature3", "categorical_feature4",
               "categorical_feature5", "categorical_feature6", "categorical_feature7", "categorical_feature8",
               "categorical_feature9", "categorical_feature10", "categorical_feature11", "categorical_feature12",
               "categorical_feature13", "categorical_feature14", "categorical_feature15", "categorical_feature16",
               "categorical_feature17", "categorical_feature18", "categorical_feature19", "categorical_feature20",
               "categorical_feature21", "categorical_feature22", "categorical_feature23", "categorical_feature24",
               "categorical_feature25", "categorical_feature26"]
    df = pd.read_csv("criteo_complete_data_sample.csv", sep="\t", header=None, names=columns)
    print(df.columns)
    print(df.shape)

    # [BEGIN] DESCRIPTIVE STATISTICS -----------------------------------------

    print(df.info())
    print()
    print(df.head())
    print()
    print(df.describe())
    print()
    # sns_plot = sns.heatmap(df.corr(), annot=True)
    # fig = sns_plot.get_figure()
    # fig.savefig("output.png")

    print("Unique values per column:")
    for col in df.columns.values:
        print("{}: {}".format(col, len(df[col].unique())))
    print()
    # [END] DESCRIPTIVE STATISTICS -------------------------------------------

    # [BEGIN] MISSING VALUES -------------------------------------------------
    # rows with at least one missing value
    rows_with_na = df[df.isna().any(axis=1)]
    print("Num. rows with missing values: {}\n".format(len(rows_with_na)))

    # missing value distribution in columns
    print(df.isna().sum())
    mask_col_with_null = df.isna().sum() > 0
    cols_with_null = df.columns[mask_col_with_null].tolist()
    print("Columns with null values: {}\n".format(cols_with_null))

    # analysis of the columns with null values in order to understand which replace strategy to be adopted
    numerical_col_with_null = []
    categorical_col_with_null = []
    for col in cols_with_null:
        # print(col)
        # print(df[col].dtypes)
        # print(df[col].unique()[:2])
        # print()
        if not (df[col].dtypes == object):
            numerical_col_with_null.append(col)
        else:
            categorical_col_with_null.append(col)
    print("Numerical features with null: {}".format(numerical_col_with_null))
    print("Categorical features with null: {}\n".format(categorical_col_with_null))

    # replace null values with mean for numerical feature
    print("Replacing null values with means for numerical feature {}\n".format(numerical_col_with_null))
    df[numerical_col_with_null] = df[numerical_col_with_null].fillna(df[numerical_col_with_null].mean())

    # replace null values with the most frequent value for categorical feature
    print("Replacing null values with the most frequent values for categorical features {}\n".format(
        categorical_col_with_null))
    # imp = SimpleImputer(strategy="most_frequent")
    # df[categorical_col_with_null] = imp.fit_transform(df[categorical_col_with_null])
    df[categorical_col_with_null] = df[categorical_col_with_null].fillna(df[categorical_col_with_null].mode().iloc[0])

    # rows with at least one missing value
    rows_with_na = df[df.isna().any(axis=1)]
    print("Num. rows with missing values: {}\n".format(len(rows_with_na)))

    df["Id"] = range(df.shape[0])

    df.to_csv("criteo_complete_data_sample_clean.csv", index=False)

    # [END] MISSING VALUES ---------------------------------------------------

    attribute_to_predict = "label"
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
    cols = train.columns.tolist()
    new_cols = cols[-1:] + cols[:-1]
    train = train[new_cols]
    train.to_csv("criteo_train_data_sample_clean.csv", index=False)

    print("Starting saving testing data...")
    test = X_test.copy()
    test[attribute_to_predict] = y_test
    cols = test.columns.tolist()
    new_cols = cols[-1:] + cols[:-1]
    test = test[new_cols]
    test.to_csv("criteo_test_data_sample_clean.csv", index=False)


class CriteoBinaryClassificationPipeline(object):
    def __init__(self, name, dataset_file, train_file, test_file, out_dir=None):
        self.name = name
        SKLEARN_DIR = os.path.dirname(os.path.abspath(__file__))
        DATA_DIR = os.path.abspath('..')
        self.dataset_file_path = os.path.join(DATA_DIR, "dataset", dataset_file)
        self.train_dataset_file_path = os.path.join(DATA_DIR, "dataset", train_file)
        self.test_dataset_file_path = os.path.join(DATA_DIR, "dataset", test_file)
        self.OUTPUT_DIR = os.path.join(SKLEARN_DIR, "assets", "output")
        self.dataset_file_path = "/home/matteo/Scrivania/criteo/data/criteo_data.csv"
        self.train_dataset_file_path = "/home/matteo/Scrivania/criteo/data/criteo_train_data.csv"
        self.test_dataset_file_path = "/home/matteo/Scrivania/criteo/data/criteo_test_data.csv"
        #self.dataset_file_path = "/mnt/flight_delay_data/flight_complete_data.csv"
        #self.train_dataset_file_path = "/mnt/flight_delay_data/flight_delay_train_data.csv"
        #self.test_dataset_file_path = "/mnt/flight_delay_data/flight_delay_test_data.csv"

        # INITIALIZE MACHINE LEARNING PIPELINE
        self.ml_pipeline = MLPipeline(self.OUTPUT_DIR)

    def transform_and_fit(self, restore_save_trained_model=False):
        # LOAD DATASET --------------------------------------------------------------------------------
        self.class_attribute = "label"

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
        self.drop_features = ["Id"]
        selected_dataset = self.ml_pipeline.drop_features_from_data(self.X_train, self.drop_features)

        # TRAIN SET TRANSFORMATION
        # Encode the categorical features with one-hot encoder
        categorical_features = []
        numerical_features = []
        for col in selected_dataset.columns.values:
            if not (selected_dataset[col].dtypes == object):
                numerical_features.append(col)
            else:
                categorical_features.append(col)
        #ohe = ce.OneHotEncoder(handle_unknown='value', cols=categorical_features, use_cat_names=True)
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
        exit(1)

        # TRAIN CLASSIFIER
        features = list(self.ml_pipeline.get_transformations()[0].transformers_[0][1].steps[0][1].get_feature_names())
        features += numerical_features
        classifier = GradientBoostingClassifier(max_leaf_nodes=20, n_estimators=100, min_samples_leaf=10,
                                                learning_rate=0.2, random_state=24)
        self.ml_pipeline.train_classifier(transformed_dataset, self.y_train, features,
                                          classifier, save_restore_model=restore_save_trained_model)

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

        self.predictions, self.probabilities, self.scores = self.ml_pipeline.execute_prediction_pipeline(X, y)

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
        init_score = self.ml_pipeline.get_classifier()._raw_predict_init(X_init).ravel()[0]
        self.ml_pipeline.get_classifier().init_score = init_score

        # SAVE PREDICTION RESULTS INTO FILE
        # self.ml_pipeline.execute_prediction_pipeline(X_test, y_test, output="file")

        # TRANSLATE MACHINE LEARNING PIPELINE IN SQL
        table_name = "criteo_FastTree"
        db_data = self.data.drop(self.drop_features[:-1], axis=1)  # i mantain in the data the id column
        ml_sql_pipepline = CriteoSQLPipeline(db_data, table_name, self.ml_pipeline, self.predictions,
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

    criteo_pipeline = CriteoBinaryClassificationPipeline("CRITEO", "criteo_data_sample.csv", "criteo_sample_train_data.csv",
                                                          "criteo_sample_test_data.csv")
    #criteo_pipeline = CriteoBinaryClassificationPipeline("CRITEO", "criteo_complete_data_sample_clean.csv", "criteo_train_data_sample_clean.csv",
    #                                                   "criteo_test_data_sample_clean.csv")
    test_executor = TestExecutor(criteo_pipeline)

    if test_method == 'sklearn_effectiveness':
        test_executor.evaluate_sklearn_pipeline_effectiveness()
    elif test_method == 'sklearn_efficiency':
        test_executor.evaluate_sklearn_pipeline_chunk_performance()
    elif test_method == 'sklearn_sql_comparison':
        test_executor.compare_sql_sklearn_predictions()
    else:
        raise ValueError(
            "Test method not available ({}). The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")
