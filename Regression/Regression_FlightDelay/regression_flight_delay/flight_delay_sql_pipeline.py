from python2sql.sql.sql_wrappers import *
from python2sql.utils.db_connection import DatabaseConnectionManager


class FlightDelaySQLPipeline(object):

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

    def _get_ohe_feature_value_mapping(self):

        # one_hot_encoder_feature_mapping = {}
        # one_hot_encoder_mapping = self.ml_pipeline.get_transformations()[0].transformers_[0][1].steps[0][
        #     1].category_mapping
        # for col_mapping in one_hot_encoder_mapping:
        #     col = col_mapping['col']
        #     mapping = col_mapping['mapping']
        #     ohe_mapping = {"{}_{}".format(col, key): {"value": key, "col": col} for key in mapping.keys()}
        #     one_hot_encoder_feature_mapping.update(ohe_mapping)

        one_hot_encoder_feature_mapping = {}
        categorical_features = self.ml_pipeline.get_transformations()[0].transformers_[0][2]
        #one_hot_encoder_mapping = self.ml_pipeline.get_transformations()[0].transformers_[0][1].steps[0][
        #    1].get_feature_names(categorical_features)
        one_hot_encoder_mapping = self.ml_pipeline.get_transformations()[0].transformers_[0][1].steps[0][
            1].get_feature_names()

        for col_mapping in one_hot_encoder_mapping:
            feature_item = col_mapping.split("_")
            feature = categorical_features[int(feature_item[0].replace('x', ""))]
            value = feature_item[1]

            ohe_mapping = {col_mapping: {"value": value, "col": feature}}
            one_hot_encoder_feature_mapping.update(ohe_mapping)

        return one_hot_encoder_feature_mapping

    def generate_sql_queries(self):

        queries = []

        regressor = self.ml_pipeline.get_regressor()
        regressor_name = type(regressor).__name__
        sql_wrapper_class = "{}SQL".format(regressor_name)
        sql_wrapper = eval(sql_wrapper_class)(regressor, self.table_name)

        one_hot_encoder_feature_mapping = self._get_ohe_feature_value_mapping()
        sql_wrapper._manage_one_hot_encoding_features_in_trees_rules(one_hot_encoder_feature_mapping)

        query = sql_wrapper.generate_sql_query()
        queries.append(query)

        return queries

    def perform_query(self, query):

        print(query)
        results = self.engine.execute(query)
        i = 0
        predicted_scores = []
        real_scores = []
        for r in results:
            if i < 20:
                print(r)
            i += 1
            predicted_scores.append(r[0])
            real_scores.append(r[2])

        predictions = [{"predicted_scores": predicted_scores, "real_scores": real_scores}]

        return predictions



