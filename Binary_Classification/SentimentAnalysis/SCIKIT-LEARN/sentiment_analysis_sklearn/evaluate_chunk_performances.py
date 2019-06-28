import os
import sys

from nltk.util import ngrams
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import SGDClassifier

from python2sql.ml.ml_pipeline import MLPipeline
from python2sql.test.performance_evaluation import EvaluatePredictionTimes


def words_and_3_grams(text):

    # first get individual words
    tokenized = text.split()
    for token in tokenized:
        yield token.lower()

    # and get a list of all the character 3-grams
    tri_grams = ngrams(text, 3)
    for tri_gram in tri_grams:
        yield ''.join(tri_gram).lower()


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
    data_size = data.shape[0]

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
    classifier = SGDClassifier(loss="log")
    ml_pipeline.train_classifier(transformed_dataset, y_train, features, classifier)
    # END TRAIN MODEL --------------------------------------------------------------------------------------------------

    # BEGIN SAVE PREDICTION TIMES --------------------------------------------------------------------------------------
    #chunk_sizes = [1, 10, 100, 1000, 10000, 100000, data_size]
    chunk_sizes = [10, 100, 1000, 10000, 100000, data_size]
    output_methods = [None, "console", "db", "file"]
    times_evaluator = EvaluatePredictionTimes(full_dataset_file_path, data_size, data_header, class_attribute,
                                              chunk_sizes, output_methods, ml_pipeline, sep='\t', transformation_target_attribute="comment")
    times_evaluator.evaluate_pipeline_times()
    times_evaluator.save_performance_to_file(OUTPUT_DIR)
    # END SAVE PREDICTION TIMES ----------------------------------------------------------------------------------------
