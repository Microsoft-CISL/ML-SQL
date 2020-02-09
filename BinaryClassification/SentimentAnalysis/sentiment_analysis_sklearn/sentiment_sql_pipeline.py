from python2sql.sql.sql_wrappers import *
from python2sql.utils.db_connection import DatabaseConnectionManager


class SentimentSQLPipeline(object):

    def __init__(self, data, table_name, ml_pipeline, predictions, probabilities, scores):
        self.ml_pipeline = ml_pipeline
        self.table_name = table_name
        self.predictions = predictions
        self.probabilities = probabilities
        self.scores = scores
        self.queries = []
        self.numbers = None
        self.weight_table = None
        self.num_data = len(data)

        db_manager = DatabaseConnectionManager()
        self.engine = db_manager.create_db_connection("mysql", "new_ml_sql_test", charset='utf8mb4')

        # create table with original data and predictions, probabilities and scores
        self._create_data_table(table_name, data, predictions, probabilities, scores)

        # create a table with the first n integers
        # this table will be used to create combinations of tokens in SQL and to ri-create the FeaturizeText function
        n = 10000
        self._create_numbers_table(n)

        # create a table with all tokens and their weigths
        self._create_weights_table()


    def _create_data_table(self, table_name, data, predictions, probabilities, scores):

        try:

            string_db = "create table {}(".format(table_name) + \
                "Label Boolean," + \
                "Comment varchar(10000) CHARACTER SET utf8mb4," + \
                "Id int," + \
                "PredictedLabel Boolean," + \
                "Probability float," + \
                "Score float" + \
            ");"

            if not self.engine.dialect.has_table(self.engine, table_name):
                print("PRE CREATION")
                self.engine.execute(string_db)
                print("POST CREATION")
            else:
                print("PRE DROP AND CREATION")
                self.engine.execute("drop table {};".format(table_name))
                self.engine.execute(string_db)
                print("POST DROP AND CREATION")


            data["PredictedLabel"] = predictions
            data["Probability"] = probabilities
            data["Score"] = scores
            data = data.astype({"PredictedLabel": bool, "Probability": float, "Score": float})
            data.to_sql(table_name, con=self.engine, index=False, if_exists="append")
        except Exception as e:
            print("Error in data table creation.")
            print(e)
            exit(1)

    def _create_numbers_table(self, n):

        self.numbers = "numbers"
        try:
            numbers = list(range(0,n))
            numbers_df = pd.DataFrame(data=numbers, columns=["n"])
            numbers_df.to_sql(self.numbers, con=self.engine, index=False, if_exists="replace")
        except ValueError:
            print("Numbers table already existing.")


    def _create_weights_table(self):

        try:
            classifier = self.ml_pipeline.get_classifier()
            classifier_name = type(classifier).__name__
            features = classifier.feature_names

            sql_wrapper_class = "{}SQL".format(classifier_name)
            sql_wrapper = eval(sql_wrapper_class)(classifier, "")

            params = sql_wrapper.get_params()
            weights = params["weights"]
            bias = params["bias"]
            self.weights = weights
            self.bias = bias
            self.weight_table = "weights_sentiment_analysis"

            weight_table_string = "create table {}(".format(self.weight_table) + \
                "label varchar(10000) CHARACTER SET utf8mb4," + \
                "weight float);"
            with self.engine.connect() as con:
                results = con.execute(weight_table_string)

            try:
                data = []
                for i in range(len(weights)):
                    data.append([features[i], weights[i]])
                data_weights = pd.DataFrame(data=data, columns=["label", "weight"])
                print(data_weights.head())
                data_weights.to_sql("weights_sentiment_analysis", con=self.engine, index=False, if_exists="append")
            except ValueError:
                print("Weigths table already existing.")
        except Exception as e:
            print(e)


    def check_token_frequency(self):

        id_param = "Id"
        text_param = "Comment"
        chunksize = self.num_data
        id = 0

        sb = "SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from " + \
             "(SELECT id, feature, count(*) * (1) as count FROM " + \
             "(SELECT" + \
             "  {}.Id, CONCAT(\"t.\",REPLACE(REPLACE(REPLACE(lower(SUBSTRING(REPLACE({}.{},' ','␠'), {}.n,3)),'␠','<␠>'),'␂','<␂>'),'␃','<␃>')) as feature".format(
                 self.table_name, self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {} ".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH({}.{})-3 >= {}.n-1".format(self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {} )".format(self.table_name, id, self.table_name, id_param, id,
                                                                chunksize) + \
             ") " + \
             "AS F1 " + \
             "group by id,feature) AS F1" + \
             " LEFT JOIN " + \
             "(SELECT id,SQRT(SUM(count)) as ww FROM" + \
             "(SELECT id,feature, POW(count(*),2) as count FROM " + \
             "(SELECT" + \
             "  {}.Id, ".format(self.table_name) + \
             "  CONCAT(\"t.\",REPLACE(REPLACE(lower(SUBSTRING(REPLACE({}.{},' ','␠'), {}.n,3)),'␠','<␠>'),'␂','<␂>')) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH({}.{})- 3 >= {}.n-1".format(self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ") " + \
             "AS F1" + \
             " group by id,feature) " + \
             "AS F2" + \
             " group by id) AS F2 " + \
             "ON (F2.id = F1.id)" + \
             " UNION ALL" + \
             " SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from " + \
             "(SELECT id, feature, count(*) * (1) as count FROM " + \
             "(SELECT" + \
             "  {}.Id,".format(self.table_name) + \
             "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', {}.n), ' ', -1) ) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )))".format(
                 self.table_name, text_param) + \
             " -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= {}.n-1".format(
                 self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ")" + \
             " AS F1" + \
             " group by id,feature) AS F1" + \
             " LEFT JOIN " + \
             "(SELECT id, SQRT(SUM(count)) as ww FROM" + \
             "(SELECT id,feature, POW(count(*),2) as count FROM " + \
             "(SELECT" + \
             "  {}.Id,".format(self.table_name) + \
             "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', {}.n), ' ', -1) ) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )))".format(
                 self.table_name, text_param) + \
             " -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= {}.n-1".format(
                 self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ") " + \
             "AS F1" + \
             " group by id,feature)" + \
             " AS F2" + \
             " group by id) AS F2" + \
             " ON (F2.id = F1.id) "

        return sb


    def get_token_weights(self):

        id_param = "Id"
        text_param = "Comment"
        chunksize = self.num_data
        id = 0

        sb = " SELECT id, (SUM( F1.count * {}.weight) + {}) AS weight FROM".format(self.weight_table, self.bias) + \
             "(" + \
             "SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from " + \
             "(SELECT id, feature, count(*) * (1) as count FROM " + \
             "(SELECT" + \
             "  {}.Id, CONCAT(\"t.\",REPLACE(REPLACE(REPLACE(lower(SUBSTRING(REPLACE({}.{},' ','␠'), {}.n,3)),'␠','<␠>'),'␂','<␂>'),'␃','<␃>')) as feature".format(
                 self.table_name, self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {} ".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH({}.{})-3 >= {}.n-1".format(self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {} )".format(self.table_name, id, self.table_name, id_param, id,
                                                                chunksize) + \
             ") " + \
             "AS F1 " + \
             "group by id,feature) AS F1" + \
             " LEFT JOIN " + \
             "(SELECT id,SQRT(SUM(count)) as ww FROM" + \
             "(SELECT id,feature, POW(count(*),2) as count FROM " + \
             "(SELECT" + \
             "  {}.Id, ".format(self.table_name) + \
             "  CONCAT(\"t.\",REPLACE(REPLACE(lower(SUBSTRING(REPLACE({}.{},' ','␠'), {}.n,3)),'␠','<␠>'),'␂','<␂>')) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH({}.{})- 3 >= {}.n-1".format(self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ") " + \
             "AS F1" + \
             " group by id,feature) " + \
             "AS F2" + \
             " group by id) AS F2 " + \
             "ON (F2.id = F1.id)" + \
             " UNION ALL" + \
             " SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from " + \
             "(SELECT id, feature, count(*) * (1) as count FROM " + \
             "(SELECT" + \
             "  {}.Id,".format(self.table_name) + \
             "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', {}.n), ' ', -1) ) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )))".format(
                 self.table_name, text_param) + \
             " -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= {}.n-1".format(
                 self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ")" + \
             " AS F1" + \
             " group by id,feature) AS F1" + \
             " LEFT JOIN " + \
             "(SELECT id, SQRT(SUM(count)) as ww FROM" + \
             "(SELECT id,feature, POW(count(*),2) as count FROM " + \
             "(SELECT" + \
             "  {}.Id,".format(self.table_name) + \
             "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', {}.n), ' ', -1) ) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )))".format(
                 self.table_name, text_param) + \
             " -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= {}.n-1".format(
                 self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ") " + \
             "AS F1" + \
             " group by id,feature)" + \
             " AS F2" + \
             " group by id) AS F2" + \
             " ON (F2.id = F1.id) " + \
             ")" + \
             " as F1 INNER JOIN {} ON ( {}.label = F1.feature) group by id".format(self.weight_table,
                                                                                   self.weight_table)


    def get_scores(self):

        id_param = "Id"
        text_param = "Comment"
        chunksize = self.num_data
        id = 0

        sb = "SELECT F.id, weight, score, ABS(score - weight) as difference FROM (" + \
             " SELECT id, (SUM( F1.count * {}.weight) + {}) AS weight FROM".format(self.weight_table, self.bias) + \
             "(" + \
             "SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from " + \
             "(SELECT id, feature, count(*) * (1) as count FROM " + \
             "(SELECT" + \
             "  {}.Id, CONCAT(\"t.\",REPLACE(REPLACE(REPLACE(lower(SUBSTRING(REPLACE({}.{},' ','␠'), {}.n,3)),'␠','<␠>'),'␂','<␂>'),'␃','<␃>')) as feature".format(
                 self.table_name, self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {} ".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH({}.{})-3 >= {}.n-1".format(self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {} )".format(self.table_name, id, self.table_name, id_param, id,
                                                                chunksize) + \
             ") " + \
             "AS F1 " + \
             "group by id,feature) AS F1" + \
             " LEFT JOIN " + \
             "(SELECT id,SQRT(SUM(count)) as ww FROM" + \
             "(SELECT id,feature, POW(count(*),2) as count FROM " + \
             "(SELECT" + \
             "  {}.Id, ".format(self.table_name) + \
             "  CONCAT(\"t.\",REPLACE(REPLACE(lower(SUBSTRING(REPLACE({}.{},' ','␠'), {}.n,3)),'␠','<␠>'),'␂','<␂>')) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH({}.{})- 3 >= {}.n-1".format(self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ") " + \
             "AS F1" + \
             " group by id,feature) " + \
             "AS F2" + \
             " group by id) AS F2 " + \
             "ON (F2.id = F1.id)" + \
             " UNION ALL" + \
             " SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from " + \
             "(SELECT id, feature, count(*) * (1) as count FROM " + \
             "(SELECT" + \
             "  {}.Id,".format(self.table_name) + \
             "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', {}.n), ' ', -1) ) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )))".format(
                 self.table_name, text_param) + \
             " -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= {}.n-1".format(
                 self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ")" + \
             " AS F1" + \
             " group by id,feature) AS F1" + \
             " LEFT JOIN " + \
             "(SELECT id, SQRT(SUM(count)) as ww FROM" + \
             "(SELECT id,feature, POW(count(*),2) as count FROM " + \
             "(SELECT" + \
             "  {}.Id,".format(self.table_name) + \
             "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', {}.n), ' ', -1) ) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )))".format(
                 self.table_name, text_param) + \
             " -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= {}.n-1".format(
                 self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ") " + \
             "AS F1" + \
             " group by id,feature)" + \
             " AS F2" + \
             " group by id) AS F2" + \
             " ON (F2.id = F1.id) " + \
             ")" + \
             " as F1 INNER JOIN {} ON ( {}.label = F1.feature) group by id".format(self.weight_table,
                                                                                   self.weight_table) + \
             ") AS F INNER JOIN {} ON (F.id = {}.Id)".format(self.table_name, self.table_name)

        return sb


    def generate_sql_queries(self):

        queries = []

        id_param = "Id"
        text_param = "Comment"
        chunksize = self.num_data
        id = 0

        query = "SELECT weight, F.id, score FROM (" + \
             " SELECT id, (SUM( F1.count * {}.weight) + {}) AS weight FROM".format(self.weight_table, self.bias) + \
             "(" + \
             "SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from " + \
             "(SELECT id, feature, count(*) * (1) as count FROM " + \
             "(SELECT" + \
             "  {}.Id, CONCAT(\"t.\",REPLACE(REPLACE(REPLACE(lower(SUBSTRING(REPLACE({}.{},' ','␠'), {}.n,3)),'␠','<␠>'),'␂','<␂>'),'␃','<␃>')) as feature".format(
                 self.table_name, self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {} ".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH({}.{})-3 >= {}.n-1".format(self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {} )".format(self.table_name, id, self.table_name, id_param, id,
                                                                chunksize) + \
             ") " + \
             "AS F1 " + \
             "group by id,feature) AS F1" + \
             " LEFT JOIN " + \
             "(SELECT id,SQRT(SUM(count)) as ww FROM" + \
             "(SELECT id,feature, POW(count(*),2) as count FROM " + \
             "(SELECT" + \
             "  {}.Id, ".format(self.table_name) + \
             "  CONCAT(\"t.\",REPLACE(REPLACE(lower(SUBSTRING(REPLACE({}.{},' ','␠'), {}.n,3)),'␠','<␠>'),'␂','<␂>')) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH({}.{})- 3 >= {}.n-1".format(self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ") " + \
             "AS F1" + \
             " group by id,feature) " + \
             "AS F2" + \
             " group by id) AS F2 " + \
             "ON (F2.id = F1.id)" + \
             " UNION ALL" + \
             " SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from " + \
             "(SELECT id, feature, count(*) * (1) as count FROM " + \
             "(SELECT" + \
             "  {}.Id,".format(self.table_name) + \
             "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', {}.n), ' ', -1) ) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )))".format(
                 self.table_name, text_param) + \
             " -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= {}.n-1".format(
                 self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ")" + \
             " AS F1" + \
             " group by id,feature) AS F1" + \
             " LEFT JOIN " + \
             "(SELECT id, SQRT(SUM(count)) as ww FROM" + \
             "(SELECT id,feature, POW(count(*),2) as count FROM " + \
             "(SELECT" + \
             "  {}.Id,".format(self.table_name) + \
             "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', {}.n), ' ', -1) ) as feature".format(
                 self.table_name, text_param, self.numbers) + \
             " FROM" + \
             "  {} INNER JOIN {}".format(self.numbers, self.table_name) + \
             "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '), '  ', ' ' )))".format(
                 self.table_name, text_param) + \
             " -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( {}.{},'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= {}.n-1".format(
                 self.table_name, text_param, self.numbers) + \
             " WHERE {}.Id >= {} and {}.{} < ({} + {})".format(self.table_name, id, self.table_name, id_param, id,
                                                               chunksize) + \
             ") " + \
             "AS F1" + \
             " group by id,feature)" + \
             " AS F2" + \
             " group by id) AS F2" + \
             " ON (F2.id = F1.id) " + \
             ")" + \
             " as F1 INNER JOIN {} ON ( {}.label = F1.feature) group by id".format(self.weight_table,
                                                                                   self.weight_table) + \
             ") AS F INNER JOIN {} ON (F.id = {}.Id)".format(self.table_name, self.table_name)

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



