from python2sql.sql.sql_wrappers import *
from python2sql.utils.db_connection import DatabaseConnectionManager


class BikeSharingPoissonRegressionSQLPipeline(object):
    def __init__(self, data, table_name, ml_pipeline, predictions, probabilities, scores):
        self.ml_pipeline = ml_pipeline
        self.table_name = table_name
        self.predictions = predictions
        self.probabilities = probabilities
        self.scores = scores
        db_manager = DatabaseConnectionManager()
        self.engine = db_manager.create_db_connection("mysql", "new_ml_sql_test")
        self.columns = data.columns.values[:-2]

        # create table with original data and predictions, probabilities and scores
        self._create_data_table(table_name, data, predictions, probabilities, scores)

    def _create_data_table(self, table_name, data, predictions, probabilities, scores):

        try:
            data["PredictedLabel"] = predictions
            data["Probability"] = probabilities
            data["Score"] = scores
            data = data.astype({"PredictedLabel": float, "Probability": float, "Score": float})
            data.to_sql(table_name, con=self.engine, index=False, if_exists="replace")
        except Exception as e:
            print("Error in data table creation.")
            print(e)
            exit(1)

    def generate_sql_queries(self):

        queries = []

        regressor = self.ml_pipeline.get_regressor()

        # attempt with statsmodels poisson model
        # regressor_name = "PoissonRegression"
        regressor_name = type(regressor).__name__

        sql_wrapper_class = "{}SQL".format(regressor_name)
        sql_wrapper = eval(sql_wrapper_class)(regressor, self.table_name)

        query = sql_wrapper.generate_sql_query()
        queries.append(query)

        return queries

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