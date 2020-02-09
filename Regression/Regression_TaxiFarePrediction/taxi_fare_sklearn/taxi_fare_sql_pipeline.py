from python2sql.sql.sql_wrappers import *
from python2sql.utils.db_connection import DatabaseConnectionManager


class TaxiFareSQLPipeline(object):

    def __init__(self, data, table_name, ml_pipeline, predictions, probabilities, scores):
        self.ml_pipeline = ml_pipeline
        self.table_name = table_name
        self.predictions = predictions
        self.probabilities = probabilities
        self.scores = scores
        db_manager = DatabaseConnectionManager()
        self.engine = db_manager.create_db_connection("mysql", "new_ml_sql_test")
        self.columns = data.columns.values[:-2]

        # get pipeline transformations
        transformations = self._get_transformations()
        self.scaler = transformations[0]
        self.ohe = transformations[1]

        # CREATE TABLES ---------------------------------------------------------------------------

        # create table with original data and predictions, probabilities and scores
        self._create_data_table(table_name, data, predictions, probabilities, scores)

        # create table with regression weights
        self.weight_table = "weights_taxi"
        self._create_weights_table()
        # -----------------------------------------------------------------------------------------

    def _get_transformations(self):

        transformations = []
        transformers = self.ml_pipeline.get_transformations()[0].transformers_

        for transformer in transformers:
            transformer_category = transformer[0]
            transformer_pipeline = transformer[1]
            transformer_columns = transformer[2]

            transformer_pipeline_step = transformer_pipeline.steps[0]
            transformer_pipeline_model = transformer_pipeline_step[1]
            transformer_pipeline_model.feature_names = self.columns

            transformer_dict = {}
            transformer_dict["model"] = transformer_pipeline_model
            transformer_dict["columns"] = transformer_columns

            transformations.append(transformer_dict)

            #if transformer_category == "continous":
            #    self.scaler["model"] = transformer_pipeline_model
            #    self.scaler["columns"] = transformer_columns
            #elif transformer_category == "categorical":
            #    self.ohe["model"] = transformer_pipeline_model
            #    self.ohe["columns"] = transformer_columns
        return transformations

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

    def _create_weights_table(self):

        try:
            regressor = self.ml_pipeline.get_regressor()
            regressor_name = type(regressor).__name__
            features = regressor.feature_names

            sql_wrapper_class = "{}SQL".format(regressor_name)
            sql_wrapper = eval(sql_wrapper_class)(regressor, "")

            params = sql_wrapper.get_params()
            weights = params["weights"]
            bias = params["bias"]
            self.weights = weights
            self.bias = bias

            try:
                data = []
                for i in range(len(weights)):
                    data.append([features[i], weights[i]])
                data_weights = pd.DataFrame(data=data, columns=["label", "weight"])
                data_weights["label"] = range(len(data_weights))
                # print(data_weights.head())
                data_weights.to_sql(self.weight_table, con=self.engine, index=False, if_exists="replace")
            except ValueError:
                print("Weigths table already existing.")
                exit(1)
        except Exception as e:
            print(e)
            exit(1)

    def _generate_normalize_table(self):
        sql_scaler = StandardScalerSQL(self.scaler["model"], self.table_name)
        query = sql_scaler.generate_sql_query(self.scaler["columns"])
        return query

    def _get_ohe_feature_value_mapping(self, ohe_category_mapping, feature, offset):

        mapping_dict = {}
        selected_col = None
        for col_mapping in ohe_category_mapping:
            col = col_mapping['col']
            if feature.startswith(col):
                mapping = col_mapping['mapping'].to_dict()
                if str(col_mapping["data_type"]).startswith("int"): # if it is a numerical feature
                    mapping_dict = {int(key): mapping[int(key)] + offset - 1 for key in mapping if
                                    not (key is np.nan or key != key)}
                else:
                    mapping_dict = {key: mapping[key] + offset - 1 for key in mapping if
                                                not (key is np.nan or key != key)}
                selected_col = col
        return mapping_dict, selected_col

    def _create_value_mapping_one_hot_enconding(self):
        columns_weight_position = {}
        ohe_category_mapping = self.ohe["model"].category_mapping
        #offset = 0

        #    offset += len(columns_weight_position[col])
        #for col in self.columns:
        #    if col not in self.ohe["columns"]:
        #        columns_weight_position[col] = {'': offset}
        #        offset += 1

        regressor = self.ml_pipeline.get_regressor()
        features = regressor.feature_names
        i = 0
        while i < len(features):
            col = features[i]
            if col in self.columns:
                columns_weight_position[col] = {'': i}
                i += 1
            else:
                col_value_mapping, selected_col = self._get_ohe_feature_value_mapping(ohe_category_mapping, col, i)
                columns_weight_position[selected_col] = col_value_mapping
                i += len(col_value_mapping)

        return columns_weight_position

    def _transpose_columns_to_rows(self, id_column, table_name):

        query = ""
        union_pieces = []
        for col in self.columns:
            q = "select {}, '{}' as name, {} as value \n from {}\n".format(id_column, col, col, table_name)
            union_pieces.append(q)

        for i in range(len(union_pieces)-1):
            query += "{}\n UNION ALL \n".format(union_pieces[i])

        query += "\n {}".format(union_pieces[-1])

        return query

    def _generate_sql_weight_column_one_hot(self, weighting_dictionary, columns, name_column, value_column):
        query = "CASE \n"
        for col in columns:
            res = "1"
            if not weighting_dictionary[col]:
                res = value_column

            query += "WHEN {} = '{}' THEN {}\n".format(name_column, col, res)

        query += "\n END\n"
        return query

    def _generate_sql_feature_column_one_hot(self, value_mapping, columns, name_column, value_column):
        query = "CASE \n"

        for col in columns:
            if col in value_mapping:
                # if len(value_mapping[col]) == 1:
                if '' in value_mapping[col].keys():
                    first_key = list(value_mapping[col].keys())[0]
                    if first_key == "":
                        query += "WHEN {} = '{}' THEN {}\n".format(name_column, col, value_mapping[col][first_key])
                    else:
                        query += "WHEN {} = '{}' THEN {}\n".format(name_column, col, value_mapping[col][first_key])
                else:
                    for k in value_mapping[col].keys():
                        query += "WHEN {} = '{}' AND {} = '{}' THEN {}\n".format(name_column, col, value_column, k, value_mapping[col][k])
        query += "\n END\n"
        return query

    def _compute_weighting_and_feature_mapping(self, id_column, name_column, value_column, weight_column, feature_column, columns, table_name, weighting_dictionary, value_mapping):

        sql_one_hot_weights = self._generate_sql_weight_column_one_hot(weighting_dictionary, columns, name_column, value_column)
        sql_one_hot_features = self._generate_sql_feature_column_one_hot(value_mapping, columns, name_column, value_column)
        query = "select {},{},{},{} as {},{} as {}\n FROM {}".format(id_column, name_column, value_column, sql_one_hot_weights, weight_column, sql_one_hot_features, feature_column, table_name)

        return query

    def _join_features_with_weights(self, left_query, weight_table, join_left_param, join_right_param, left_weight, right_weight, select_columns):
        selection = ""
        for col in select_columns:
            selection += "{},".format(col)

        selection += " ( {} * {} ) as dot_product ".format(left_weight, right_weight)

        query = "select {} from".format(selection)

        query += "({}) AS F \n INNER JOIN {} ON ({}={})".format(left_query, weight_table, join_left_param, join_right_param)

        sub_query = "Select Id, (SUM(dot_product) + {} ) as PredictedScore\n from ({}) as F group by Id".format(self.bias,
                                                                                                       query)
        external_query = "select PredictedScore, L.Id, Score from ({}) AS L INNER JOIN {} on (L.Id={}.Id)".format(sub_query, self.table_name, self.table_name)

        return external_query

    def generate_sql_queries(self):

        # apply mean-and-variance normalization to original data table
        normalize_table = self._generate_normalize_table()

        # define the position of each one-hot encoder feature in the transformed dataset
        columns_weight_position = self._create_value_mapping_one_hot_enconding()

        # create a dictionary that identifies the features to which apply the one-hot enconding
        column_need_weight = {}
        for col in self.columns:
            if col in self.ohe["columns"]:
                column_need_weight[col] = True
            else:
                column_need_weight[col] = False

        # save transformed data in a transposed way
        data_transpose_query = self._transpose_columns_to_rows("Id", " ( " + normalize_table + " ) as F ")

        query = self._compute_weighting_and_feature_mapping("Id", "name", "value", "l_w", "feature", self.columns,
                                                            "(" + data_transpose_query + ") AS F", column_need_weight,
                                                            columns_weight_position)

        select_columns = ["Id", "name", "value", "l_w", "feature"]
        final_query = self._join_features_with_weights(query, self.weight_table, "feature", "label", "l_w", "weight",
                                                       select_columns)

        return [final_query]

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



