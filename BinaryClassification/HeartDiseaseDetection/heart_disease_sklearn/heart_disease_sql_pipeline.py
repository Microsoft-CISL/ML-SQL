from python2sql.sql.sql_wrappers import *
from python2sql.utils.db_connection import DatabaseConnectionManager


class HeartDiseaseSQLPipeline(object):

    def __init__(self, data, table_name, ml_pipeline, predictions, probabilities, scores):
        self.ml_pipeline = ml_pipeline
        self.table_name = table_name
        self.predictions = predictions
        self.probabilities = probabilities
        self.scores = scores
        self.queries = []
        db_manager = DatabaseConnectionManager()
        self.engine = db_manager.create_db_connection("mysql", "new_ml_sql_test")

        self._create_data_table(table_name, data, predictions, probabilities, scores)

    def _create_data_table(self, table_name, data, predictions, probabilities, scores):
        try:
            data["PredictedLabel"] = predictions
            data["Probability"] = probabilities
            data["Score"] = scores
            data = data.astype({"PredictedLabel": bool, "Probability": float, "Score": float})
            data.to_sql(table_name, con=self.engine, index=False, if_exists='replace')
        except Exception:
            print("Error in data table creation.")
            exit(1)

    def generate_sql_queries(self):

        queries = []

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
            table_name = self.table_name
        query = sql_wrapper.generate_sql_query(table_name)
        queries.append(query)


        return queries

    def perform_query_old(self, query):

        print(query)
        sql_predictions = []
        sklearn_predictions = []
        with self.engine.connect() as con:
            results = con.execute(query)

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

    def perform_query(self, query):

        results = self.engine.execute(query)
        i = 0
        predicted_scores = []
        real_scores = []
        for r in results:
            if i < 10:
                print(r)
            i += 1
            predicted_scores.append(r[0])
            real_scores.append(r[2])

        predictions = [{"predicted_scores": predicted_scores, "real_scores": real_scores}]

        return predictions



