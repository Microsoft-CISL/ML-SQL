class MLSQLPipeline(object):

    def __init__(self, ml_pipeline):
        self.ml_pipeline = ml_pipeline

    def generate_sql_queries(self):

        queries = []

        transformations = self.ml_pipeline.get_transformations()
        for transformation in transformations:

            transformation_name = type(transformation).__name__
            sql_wrapper_class = "{}SQL".format(transformation_name)
            sql_wrapper = eval(sql_wrapper_class)(transformation, "")

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

        return queries

