from python2sql.sql.sql_wrappers import *
from python2sql.utils.db_connection import DatabaseConnectionManager


class IrisMultiClassificationSQLPipeline(object):

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
            # data["Probability"] = probabilities
            class_labels = self.ml_pipeline.get_classifier().classes_
            dict_type = {"PredictedLabel": float}
            for i in range(len(class_labels)):
                class_value = class_labels[i]
                prob_column_name = "Probability_{}".format(class_value)
                score_column_name = "Score_{}".format(class_value)
                data[prob_column_name] = probabilities[:,i]
                data[score_column_name] = scores[:,i]
                dict_type[prob_column_name] = float
                dict_type[score_column_name] = float

            data = data.astype(dict_type)
            data.to_sql(table_name, con=self.engine, index=False, if_exists="replace")
        except Exception as e:
            print("Error in data table creation.")
            print(e)
            exit(1)

    def generate_sql_queries(self):

        queries = []

        classifier = self.ml_pipeline.get_classifier()
        classifier_name = type(classifier).__name__
        sql_wrapper_class = "{}SQL".format(classifier_name)
        sql_wrapper = eval(sql_wrapper_class)(classifier, self.table_name)

        query = sql_wrapper.generate_sql_query()
        queries.append(query)

        return queries

    def perform_query(self, query_dict, type="efficiency"):

        query = query_dict[type]

        with self.engine.connect() as con:
            results = con.execute(query)#.fetchall()
            i = 0

            prediction_dict = {}
            num_classes = self.scores.shape[1]

            #predicted_scores = []
            #real_scores = []

            for r in results:
                if i < 10:
                    print(r)
                i += 1

                for c in range(num_classes):
                    if c not in prediction_dict:
                        prediction_dict[c] = {"predicted_scores": [r[c + 1]]}
                        prediction_dict[c]["real_scores"] = [r[c + 1 + num_classes]]
                    else:
                        prediction_dict[c]["predicted_scores"].append(r[c + 1])
                        prediction_dict[c]["real_scores"].append(r[c + 1 + num_classes])

        predictions = []
        for key in sorted(prediction_dict.keys()):
            pred = prediction_dict[key]
            predictions.append(pred)

        return predictions