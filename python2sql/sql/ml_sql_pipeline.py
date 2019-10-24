import pandas as pd
from sqlalchemy import create_engine
from python2sql.sql.sql_wrappers import *
from python2sql.ml.utils import evaluate_binary_classification_results
from scipy.special import expit


class MLSQLPipeline(object):

    def __init__(self, data, table_name, ml_pipeline, predictions, probabilities, scores):
        self.ml_pipeline = ml_pipeline
        self.table_name = table_name
        self.predictions = predictions
        self.probabilities = probabilities
        self.scores = scores
        self.queries = []
        self.engine = create_engine('mysql+pymysql://root:ndulsp+92+pgnll@localhost/mlsql')

        try:
            data["PredictedLabel"] = predictions
            data["Probability"] = probabilities
            data["Score"] = probabilities
            data = data.astype({"PredictedLabel": bool, "Probability": float, "Score": float})
            data.to_sql(table_name, con=self.engine)
        except ValueError:
            print("Table already existing.")

    def create_numbers_table(self):

        try:
            numbers = list(range(0,10000))
            numbers_df = pd.DataFrame(data=numbers, columns=["n"])
            numbers_df.to_sql("numbers", con=self.engine)
        except ValueError:
            print("Numbers table already existing.")


    def generate_sql_queries(self):

        queries = []

        # TODO: CHECK IF THE PROGRAM IS CORRECT WHEN MULTIPLE TRANSFORMATIONS ARE USED
        transformations = self.ml_pipeline.get_transformations()
        for transformation in transformations:

            transformation_name = type(transformation).__name__
            sql_wrapper_class = "{}SQL".format(transformation_name)
            sql_wrapper = eval(sql_wrapper_class)(transformation, self.table_name)

            query = sql_wrapper.generate_sql_query()
            queries.append(query)

        classifier = self.ml_pipeline.get_classifier()
        classifier_name = type(classifier).__name__
        sql_wrapper_class = "{}SQL".format(classifier_name)
        sql_wrapper = eval(sql_wrapper_class)(classifier, "")

        if len(queries) > 0:
            table_name = " ( " + queries[-1] + " ) as F"
        else:
            table_name = None
        query = sql_wrapper.generate_sql_query(table_name)
        queries.append(query)
        self.queries = queries

        return queries

    def perform_query(self):

        final_query = self.queries[-1]
        print(final_query)
        sql_predictions = []
        sklearn_predictions = []
        with self.engine.connect() as con:
            results = con.execute(final_query)

            tot = 0
            matched = 0
            sql_positive = 0
            sklearn_positive = 0
            for result in results:
                #sql_prediction = int(round(result[0]))
                sql_prediction = result[0]
                sql_predictions.append(sql_prediction)
                result_id = result[1]
                #if sql_prediction > 1:
                #    print("ERROR")
                #    sql_prediction = 1
                sklearn_prediction = result[2]
                sklearn_predictions.append(sklearn_prediction)

                if sql_prediction == sklearn_prediction:
                    matched += 1
                #else:
                #    print("{} vs {}".format(sql_prediction, sklearn_prediction))

                if sql_prediction == 1:
                    sql_positive += 1

                if sklearn_prediction == 1:
                    sklearn_positive += 1
                tot += 1

        print("ACCURACY: {}/{}".format(matched, tot))
        print("SQL POSITIVE: {}/{}".format(sql_positive, tot))
        print("SKLEARN POSITIVE: {}/{}".format(sklearn_positive, tot))

        #evaluate_binary_classification_results("SQL model", sklearn_predictions, sql_predictions)

        for i in range(len(sql_predictions)):
            print("{} vs {} = {}".format(float(sql_predictions[i]), float(sklearn_predictions[i]), abs(float(sql_predictions[i]) - float(sklearn_predictions[i]))))



