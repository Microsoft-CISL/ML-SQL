import os
import sys

from nltk.util import ngrams
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import SGDClassifier

from python2sql.ml.ml_pipeline import MLPipeline


def words_and_3_grams(text):

    # first get individual words
    tokenized = text.split()
    for token in tokenized:
        yield token.lower()

    # and get a list of all the character 3-grams
    tri_grams = ngrams(text, 3)
    for tri_gram in tri_grams:
        yield ''.join(tri_gram).lower()


def train_model(train_dataset, class_attribute, drop_features, model_file_path, tfidf_transformer_file_path,
                featurized_train_dataset_file_path, restore=False):

    tfidf_transformer = TfidfVectorizer(analyzer=words_and_3_grams, use_idf=False)
    logistic = SGDClassifier(loss="log")

    # EXTRACT MODEL PARAMETERS ---------------------------
    # featurize text
    vocabulary = tfidf_transformer.vocabulary_

    # stochastic gradient descend with logistic regression
    weights = logistic.coef_.ravel()
    bias = logistic.intercept_[0]
    print("weights: {}".format(weights))
    print("bias: {}".format(bias))
    print("[END] MODEL TRAINING {}\n".format('-' * 20))


if __name__ == '__main__':

    # FILE PATHS
    SKLEARN_DIR = os.path.dirname(os.path.abspath(__file__))
    DATA_DIR = os.path.abspath('..')
    full_dataset_file_path = os.path.join(DATA_DIR, "dataset", "wikiDetoxAnnotated40kRows.tsv")
    OUTPUT_DIR = os.path.join(SKLEARN_DIR, "assets", "output")

    # INITIALIZE MACHINE LEARNING PIPELINE
    ml_pipeline = MLPipeline(OUTPUT_DIR)

    # LOAD DATASET
    class_attribute = "Label"
    data, data_header, data_class_labels = ml_pipeline.load_data(full_dataset_file_path, class_attribute, sep='\t')

    # SPLIT DATASET IN TRAIN AND TEST
    X_train, X_test, y_train, y_test = ml_pipeline.split_data_train_test(data, class_attribute)

    # FEATURE REMOVAL
    drop_features = ["rev_id", "year", "logged_in", "ns", "sample", "split"]
    selected_dataset = ml_pipeline.drop_features_from_data(X_train, drop_features)

    # TRAIN SET TRANSFORMATION
    transformer = TfidfVectorizer(analyzer=words_and_3_grams, use_idf=False)
    transformed_dataset = ml_pipeline.apply_transformation(selected_dataset, transformer, target_attribute="comment")

    # TRAIN CLASSIFIER
    # FIXME: TO CHECK WITH RESPECT WITH THE SQL WRAPPER THAT HAS ALREADY TO BE IMPLEMENTED
    features = list(selected_dataset.columns.values)
    # FIXME: CHECK EVALUATION RESULTS: PRECISION AND RECALL ARE ILL-DIFINED
    classifier = SGDClassifier(loss="log")
    ml_pipeline.train_classifier(transformed_dataset, y_train, features, classifier)

    # MAKE PREDICTIONS AND EVALUATE RESULTS
    ml_pipeline.execute_prediction_pipeline(X_test, y_test, transformation_target_attribute="comment")

    # TRANSLATE MACHINE LEARNING PIPELINE IN SQL
    # TODO: ALREADY TO BE IMPLEMENTED
    #ml_sql_pipepline = MLSQLPipeline(ml_pipeline)
    #print(ml_sql_pipepline.generate_sql_queries())
