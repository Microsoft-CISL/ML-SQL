import os
from nltk.util import ngrams
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import SGDClassifier
import pandas as pd
import pickle
import numpy as np
import unicodedata
import argparse

from sklearn.pipeline import FeatureUnion
from python2sql.ml.ml_pipeline import MLPipeline
from BinaryClassification.SentimentAnalysis.sentiment_analysis_sklearn.sentiment_sql_pipeline import SentimentSQLPipeline
from python2sql.ml.utils import evaluate_regression_results
from python2sql.test.test_executor import TestExecutor


def strip_accents(unicode_string):
    """
    Strip accents (all combining unicode characters) from a unicode string.
    """

    ndf_string = unicodedata.normalize('NFD', unicode_string)
    is_not_accent = lambda char: unicodedata.category(char) != 'Mn'
    return ''.join(
        char for char in ndf_string if is_not_accent(char)
    )


def words_and_3_grams(text):

    # remove escape character
    text = text.replace("\\", "")
    # remove accents
    text = strip_accents(text)

    # tokenize
    tokenized = text.split()
    for token in tokenized:
        yield token.lower()

    # get trigrams
    tri_grams = list(ngrams(text, 3, pad_left=True, pad_right=True, left_pad_symbol='<␂>', right_pad_symbol='<␃>'))
    for tri_gram in tri_grams:

        # lower case
        gram = ''.join(tri_gram).lower()
        # replace spaces with special character
        gram = gram.replace(" ", "<␠>")
        # remove false tri-grams
        if gram.startswith("<␂><␂>") or gram.endswith("<␃><␃>"):
            continue

        yield gram

def tri_grams(text):
    # remove escape character
    text = text.replace("\\", "")
    # remove accents
    text = strip_accents(text)

    # get trigrams
    tri_grams = list(ngrams(text, 3, pad_left=True, pad_right=True, left_pad_symbol='<␂>', right_pad_symbol='<␃>'))
    for tri_gram in tri_grams:

        # lower case
        gram = ''.join(tri_gram).lower()
        # replace spaces with special character
        gram = gram.replace(" ", "<␠>")
        # remove false tri-grams
        if gram.startswith("<␂><␂>") or gram.endswith("<␃><␃>"):
            continue

        yield gram


def words(text):

    # remove escape character
    text = text.replace("\\", "")
    # remove accents
    text = strip_accents(text)

    # tokenize
    tokenized = text.split()
    for token in tokenized:
        yield token.lower()


# def train_model(train_dataset, class_attribute, drop_features, model_file_path, tfidf_transformer_file_path,
#                 featurized_train_dataset_file_path, restore=False):
#
#     tfidf_transformer = TfidfVectorizer(analyzer=words_and_3_grams, use_idf=False)
#     logistic = SGDClassifier(loss="log")
#
#     # EXTRACT MODEL PARAMETERS ---------------------------
#     # featurize text
#     vocabulary = tfidf_transformer.vocabulary_
#
#     # stochastic gradient descend with logistic regression
#     weights = logistic.coef_.ravel()
#     bias = logistic.intercept_[0]
#     print("weights: {}".format(weights))
#     print("bias: {}".format(bias))
#     print("[END] MODEL TRAINING {}\n".format('-' * 20))

def save_tfidf_model(transformer, data, out_file):
    # save model vocabulary
    with open("/home/matteo/Scrivania/vocabulary_sklearn.pickle", 'wb') as file:
        pickle.dump(transformer.vocabulary_, file)


    # save featurized training set
    import csv

    cx = data.tocoo()

    out_file_header = ['row_index', 'col_index', 'value']
    with open(out_file, 'w') as resultFile:
        wr = csv.writer(resultFile)

        # write header
        wr.writerow(out_file_header)

        # write data rows
        for i, j, v in zip(cx.row, cx.col, cx.data):
            wr.writerow([i, j, v])


def split_train_and_test_sets(train, test, class_attribute):
    y_train = train[class_attribute]
    X_train = train.drop([class_attribute], axis=1)
    y_test = test[class_attribute]
    X_test = test.drop([class_attribute], axis=1)

    return X_train, y_train, X_test, y_test

def get_vocabulary_keys(transformer):
    final_vocabulary = []
    w = 0
    for item in transformer.get_params()["transformer_list"]:
        feature_model_name = item[0]
        feature_model = item[1]

        for token in feature_model.vocabulary_:

            if feature_model_name == "words":
                w += 1
                token_key = "w.{}".format(token)
                final_vocabulary.append((feature_model.vocabulary_[token], token_key))
            else:
                token_key = "t.{}".format(token)
                final_vocabulary.append((feature_model.vocabulary_[token]+w, token_key))

    return [item[1] for item in sorted(final_vocabulary, key=lambda x: x[0])]


def save_multiple_vocabularies(transformer, transformed_dataset):
    final_vocabulary = {}
    reverse_word_vocabulary = {}
    reverse_trigram_vocabulary = {}

    for item in transformer.get_params()["transformer_list"]:
        feature_model_name = item[0]
        feature_model = item[1]

        for token in feature_model.vocabulary_:

            if feature_model_name == "words":
                token_key = "w.{}".format(token)
                reverse_word_vocabulary[feature_model.vocabulary_[token]] = token_key
            else:
                token_key = "t.{}".format(token)
                reverse_trigram_vocabulary[feature_model.vocabulary_[token]] = token_key

            final_vocabulary[token_key] = feature_model.vocabulary_[token]
    num_words = len(reverse_word_vocabulary)

    cx = transformed_dataset.tocoo()
    with open("/home/matteo/Scrivania/NEW_token_with_frequency_sklearn.txt", "w") as file:
        for i, j, v in zip(cx.row, cx.col, cx.data):
            if j in reverse_word_vocabulary:
                #print(i, j, reverse_word_vocabulary[j], v)
                file.write("{}\t{}\t{}\n".format(i, reverse_word_vocabulary[j], v))
            else:
                #print(i, j, reverse_trigram_vocabulary[j - num_words], v)
                file.write("{}\t{}\t{}\n".format(i, reverse_trigram_vocabulary[j - num_words], v))


class SentimentAnalysisPipeline(object):
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
        self.class_attribute = "Label"
        header = ["Label", "Comment", "Id"]

        # COMPLETE DATASET
        self.data = pd.read_csv(self.dataset_file_path, sep='\t', header=None, names=header)
        self.y = self.data[self.class_attribute]
        self.X = self.data.drop([self.class_attribute], axis=1)
        self.data_header = self.data.columns.values
        self.data_class_labels = np.unique(self.y.values)

        # TRAIN
        self.train = pd.read_csv(self.train_dataset_file_path, sep='\t', header=None, names=header)
        self.y_train = self.train[self.class_attribute]
        self.X_train = self.train.drop([self.class_attribute], axis=1)

        # TEST
        self.test = pd.read_csv(self.test_dataset_file_path, sep='\t', header=None, names=header)
        self.y_test = self.test[self.class_attribute]
        self.X_test = self.test.drop([self.class_attribute], axis=1)
        # ---------------------------------------------------------------------------------------------

        # FEATURE REMOVAL
        self.drop_features = ["Id"]
        selected_dataset = self.ml_pipeline.drop_features_from_data(self.X_train, self.drop_features)

        # TRAIN SET TRANSFORMATION
        transformer = FeatureUnion([("words", TfidfVectorizer(analyzer=words, use_idf=False)),
                                    ("trigrmas", TfidfVectorizer(analyzer=tri_grams, use_idf=False))])

        transformed_dataset = self.ml_pipeline.apply_transformation(selected_dataset, transformer,
                                                               target_attribute="Comment")

        #save_multiple_vocabularies(transformer, transformed_dataset)
        #exit(1)

        # TRAIN CLASSIFIER
        features = get_vocabulary_keys(transformer)
        classifier = SGDClassifier(loss="log", random_state=24)
        self.ml_pipeline.train_classifier(transformed_dataset, self.y_train, features, classifier, save_restore_model=restore_save_trained_model)

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
                                                                            transformation_target_attribute="Comment")

        return self.predictions, self.probabilities, self.scores

    def predict_sql(self):

        # TRANSLATE MACHINE LEARNING PIPELINE IN SQL
        table_name = "sentiment_analysis_detection_with_score"
        db_data = self.data.drop(self.drop_features[:-1], axis=1)  # i mantain in the data the id column
        ml_sql_pipepline = SentimentSQLPipeline(db_data, table_name, self.ml_pipeline, self.predictions, self.probabilities,
                                                      self.scores)
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

    # TODO: CREATE DATABASE new_ml_sql_test DEFAULT CHARACTER SET utf8mb4 DEFAULT COLLATE utf8mb4_unicode_ci;
    parser = argparse.ArgumentParser(description='SKLEARN vs SQL credit card pipeline.')
    parser.add_argument('-test', '--test_method', dest='test_method', type=str, action='store',
                        help="The name of test to be performed. The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")
    args = parser.parse_args()

    test_method = args.test_method

    sentimen_analysis_pipeline = SentimentAnalysisPipeline("SENTIMENT_ANALYSIS", "sentiment_data.csv", "sentiment_training_data.csv", "sentiment_test_data.csv")
    test_executor = TestExecutor(sentimen_analysis_pipeline, sep='\t', header=True, transformation_target_attribute="Comment")

    if test_method == 'sklearn_effectiveness':
        test_executor.evaluate_sklearn_pipeline_effectiveness()
    elif test_method == 'sklearn_efficiency':
        test_executor.evaluate_sklearn_pipeline_chunk_performance()
    elif test_method == 'sklearn_sql_comparison':
        test_executor.compare_sql_sklearn_predictions()
    else:
        raise ValueError(
            "Test method not available ({}). The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")