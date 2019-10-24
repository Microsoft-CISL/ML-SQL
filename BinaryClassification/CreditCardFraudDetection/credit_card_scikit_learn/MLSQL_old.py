

def generate_sql_normalize_by_variance_columns(columns, select_params, avgs, vars, suffix, table_name):
    query = "select "

    for i in range(len(columns)):
        col = columns[i]
        query += "({}-{})*1.0*({}) as {}{} ,".format(col, avgs[i], vars[i], col, suffix)

    for param in select_params:
        query += param + ", "

    query = query[:-1]

    query += " from " + table_name

    return query


def generate_sql_regression_tree(select_params, table_name, trees_parmas):

    query = "select "

    for i in range(len(trees_parmas)):
        tree_params = trees_parmas[i]
        tree_weight = tree_params["weight"]
        sql_case = tree_params["string_rules"]

        sql_case += " END AS tree_{},".format(i)

        query += sql_case

    for param in select_params:
        query += param + ", "

    query = query[:-1]

    query += " from " + table_name

    return query


def print_sql_transformation(transformation_name, query):

    print()
    print("==========================="+transformation_name+"===========================")
    print(""+query)
    print("============================================================================")


