import os
import sys
import argparse
import pandas as pd
import numpy as np
from sklearn.preprocessing import LabelEncoder
from nltk.util import ngrams
from sklearn.feature_extraction.text import TfidfVectorizer
import nltk
nltk.download("stopwords")
from nltk.corpus import stopwords
from sklearn.linear_model import Perceptron
from sklearn.multiclass import OneVsRestClassifier
from sklearn.pipeline import FeatureUnion
import unicodedata
from sklearn.model_selection import cross_validate

from python2sql.ml.ml_pipeline import MLPipeline
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


def bigrams_words_and_3_grams(text):
    stop_words = set(stopwords.words('english'))

    text = ' '.join([w for w in text.split() if not w in stop_words])

    # first get individual words
    tokenized = text.split()
    for token in tokenized:
        yield token.lower()

    # get a list of all the character 2-grams
    bi_grams = list(nltk.bigrams(tokenized))
    for bi_gram in bi_grams:
        yield ''.join(bi_gram).lower()

    # and get a list of all the character 3-grams
    tri_grams = ngrams(text, 3)
    for tri_gram in tri_grams:
        yield ''.join(tri_gram).lower()


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
    with open("/home/matteo/Scrivania/NEW_token_with_frequency_sklearn_spam.txt", "w") as file:
        for i, j, v in zip(cx.row, cx.col, cx.data):
            if j in reverse_word_vocabulary:
                #print(i, j, reverse_word_vocabulary[j], v)
                file.write("{}\t{}\t{}\n".format(i, reverse_word_vocabulary[j], v))
            else:
                #print(i, j, reverse_trigram_vocabulary[j - num_words], v)
                file.write("{}\t{}\t{}\n".format(i, reverse_trigram_vocabulary[j - num_words], v))


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


class SpamDetectionBinaryClassificationPipeline(object):
    def __init__(self, name, dataset_file, train_file, test_file, out_dir=None):
        # TODO
        self.name = name
        SKLEARN_DIR = os.path.dirname(os.path.abspath(__file__))
        DATA_DIR = os.path.abspath('..')
        self.dataset_file_path = os.path.join(DATA_DIR, "dataset", dataset_file)
        #self.train_dataset_file_path = os.path.join(DATA_DIR, "dataset", train_file)
        #self.test_dataset_file_path = os.path.join(DATA_DIR, "dataset", test_file)
        self.OUTPUT_DIR = os.path.join(SKLEARN_DIR, "assets", "output")

        # INITIALIZE MACHINE LEARNING PIPELINE
        self.ml_pipeline = MLPipeline(self.OUTPUT_DIR)

    def transform_and_fit(self, restore_save_trained_model=False):

        # TODO
        # LOAD DATASET --------------------------------------------------------------------------------
        self.class_attribute = "class"

        # COMPLETE DATASET
        self.data = pd.read_csv(self.dataset_file_path, sep="\t")
        self.y = self.data[self.class_attribute]
        self.X = self.data.drop([self.class_attribute], axis=1)
        self.data_header = self.data.columns.values
        self.data_class_labels = np.unique(self.y.values)

        # TRAIN
        #self.train = pd.read_csv(self.train_dataset_file_path)
        #self.y_train = self.train[self.class_attribute]
        #self.X_train = self.train.drop([self.class_attribute], axis=1)

        # TEST
        #self.test = pd.read_csv(self.test_dataset_file_path)
        #self.y_test = self.test[self.class_attribute]
        #self.X_test = self.test.drop([self.class_attribute], axis=1)
        # ---------------------------------------------------------------------------------------------

        # FEATURE REMOVAL
        self.drop_features = ["Id"]
        selected_dataset = self.ml_pipeline.drop_features_from_data(self.data, self.drop_features)

        # TRAIN SET TRANSFORMATION
        transformer = FeatureUnion([("words", TfidfVectorizer(analyzer=words, use_idf=False)),
                                    ("trigrmas", TfidfVectorizer(analyzer=tri_grams, use_idf=False))])

        transformed_dataset = self.ml_pipeline.apply_transformation(selected_dataset, transformer,
                                                                    target_attribute="text")

        # TRAIN CLASSIFIER
        features = get_vocabulary_keys(transformer)
        save_multiple_vocabularies(transformer, transformed_dataset)
        exit(1)

        classifier = OneVsRestClassifier(Perceptron(random_state=0, max_iter=10))

        scores = cross_validate(classifier, transformed_dataset, self.y, scoring='roc_auc', cv=5)
        print(sorted(scores.keys()))
        print(scores)
        exit(1)

        print(scores['test_recall_macro'])

        self.ml_pipeline.train_classifier(transformed_dataset, y_train, features, classifier)


        # TRAIN CLASSIFIER
        features = get_vocabulary_keys(transformer)
        # classifier = GradientBoostingClassifier(max_leaf_nodes=20, n_estimators=1, min_samples_leaf=10, learning_rate=0.2)
        classifier = GradientBoostingClassifier(max_leaf_nodes=20, n_estimators=100, min_samples_leaf=10,
                                                learning_rate=0.2, random_state=24) # TODO
        # TODO
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

        self.predictions, self.probabilities, self.scores = self.ml_pipeline.execute_prediction_pipeline(X, y)

        return self.predictions, self.probabilities, self.scores

    def predict_sql(self):

        # The predictions of the SKLEARN's model are equal to the values predicted by SQL.
        # The only difference is that SKLEARN adds to the final score (i.e., the weighted sum of the tree scores) an init
        # score. This part has not been implemented in SQL, but this score has been added to the final query as an offset.
        # retrieving the SKLEARN init score
        from sklearn.utils.validation import check_array
        from sklearn.tree._tree import DTYPE
        X_init = self.X.drop(self.drop_features, axis=1)
        X_init = check_array(X_init, dtype=DTYPE, order="C", accept_sparse='csr')
        init_score = self.ml_pipeline.get_classifier()._raw_predict_init(X_init).ravel()[0]
        self.ml_pipeline.get_classifier().init_score = init_score

        # SAVE PREDICTION RESULTS INTO FILE
        # self.ml_pipeline.execute_prediction_pipeline(self.X_test, self.y_test, output="file")

        # TRANSLATE MACHINE LEARNING PIPELINE IN SQL
        table_name = "creditcard_with_prediction"
        db_data = self.data.drop(self.drop_features[:-1], axis=1)  # i mantain in the data the id column
        ml_sql_pipepline = CreditCardSQLPipeline(db_data, table_name, self.ml_pipeline, self.predictions,
                                                          self.probabilities, self.scores)
        # TODO
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

    # TODO
    def get_name(self):
        return self.name


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description='SKLEARN vs SQL credit card pipeline.')
    parser.add_argument('-test', '--test_method', dest='test_method', type=str, action='store',
                        help="The name of test to be performed. The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")
    args = parser.parse_args()

    test_method = args.test_method

    spam_detection_pipeline = SpamDetectionBinaryClassificationPipeline("SPAM_DETECTION", "SMSSpamCollectionWithId.csv",
                                                                      None,
                                                                      None)
    test_executor = TestExecutor(spam_detection_pipeline, sep='\t')

    if test_method == 'sklearn_effectiveness':
        test_executor.evaluate_sklearn_pipeline_effectiveness()
    elif test_method == 'sklearn_efficiency':
        test_executor.evaluate_sklearn_pipeline_chunk_performance()
    elif test_method == 'sklearn_sql_comparison':
        test_executor.compare_sql_sklearn_predictions()
    else:
        raise ValueError(
            "Test method not available ({}). The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")
    exit(1)




    # FILE PATHS
    SKLEARN_DIR = os.path.dirname(os.path.abspath(__file__))
    DATA_DIR = os.path.abspath('..')
    full_dataset_file_path = os.path.join(DATA_DIR, "dataset", "SMSSpamCollection")
    OUTPUT_DIR = os.path.join(SKLEARN_DIR, "assets", "output")

    # INITIALIZE MACHINE LEARNING PIPELINE
    ml_pipeline = MLPipeline(OUTPUT_DIR)

    # LOAD DATASET
    class_attribute = "Class"
    header = ["Class", "Message"]
    data, data_header, data_class_labels = ml_pipeline.load_data(full_dataset_file_path, class_attribute,
                                                                 columns=header, sep='\t')

    # convert class attribute, which is categorical, to numerical (boolean)
    labelencoder_X = LabelEncoder()
    class_converted = labelencoder_X.fit_transform(data[class_attribute])
    data[class_attribute] = class_converted

    # SPLIT DATASET IN TRAIN AND TEST
    X_train, X_test, y_train, y_test = ml_pipeline.split_data_train_test(data, class_attribute)

    # FEATURE REMOVAL
    drop_features = []
    selected_dataset = ml_pipeline.drop_features_from_data(X_train, drop_features)

    # TRAIN SET TRANSFORMATION
    transformer = TfidfVectorizer(analyzer=bigrams_words_and_3_grams, use_idf=False)
    # FIXME: TO BE MODIFIED W.R.T. THE CHANGES APPLIED TO SENTIMENT ANALYSIS EXAMPLE
    transformed_dataset = ml_pipeline.apply_transformation(selected_dataset, transformer, target_attribute="Message")

    # TRAIN CLASSIFIER
    # FIXME: TO CHECK WITH RESPECT WITH THE SQL WRAPPER THAT HAS ALREADY TO BE IMPLEMENTED
    features = list(selected_dataset.columns.values)
    classifier = OneVsRestClassifier(Perceptron(random_state=0, max_iter=10))
    ml_pipeline.train_classifier(transformed_dataset, y_train, features, classifier)

    # MAKE PREDICTIONS AND EVALUATE RESULTS
    ml_pipeline.execute_prediction_pipeline(X_test, y_test, transformation_target_attribute="Message")

    # TRANSLATE MACHINE LEARNING PIPELINE IN SQL
    # TODO: ALREADY TO BE IMPLEMENTED
    #ml_sql_pipepline = MLSQLPipeline(ml_pipeline)
    #print(ml_sql_pipepline.generate_sql_queries())
