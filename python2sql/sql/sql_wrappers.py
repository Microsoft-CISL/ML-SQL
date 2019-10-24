import numpy as np
import os
#import graphviz
#from sklearn.tree import export_graphviz
import pandas as pd


class StandardScalerSQL(object):

    def __init__(self, scaler, table_name):
        self.scaler = scaler
        self.table_name = table_name
        self.select_params = ["Id", "Score"]
        #self.select_params = ["Id", "PredictedLabel"]
        self.params = None

    def get_params(self):
        # mean
        # scaler_mean = self.scaler.mean_
        # variance
        # scaler_var = self.scaler.var_
        # std
        scaler_std = [1.0/std_val for std_val in self.scaler.scale_]
        # it seems that the ML.NET model doesn't center the data with respect the mean; so I set the means to 0.
        scaler_mean = np.zeros(len(scaler_std))

        params = {"avgs": scaler_mean, "stds": scaler_std}
        # print("params: {}".format(params))
        self.params = params

        return params

    def generate_sql_query(self, target_columns=None):

        self.get_params()

        avgs = self.params["avgs"]
        stds = self.params["stds"]
        columns = self.scaler.feature_names
        suffix = ""

        query = "select "

        index = 0
        for i in range(len(columns)):
            col = columns[i]
            if target_columns:
                if col in target_columns:
                    query += "({}-{})*1.0*({}) as {}{} ,".format(col, avgs[index], stds[index], col, suffix)
                    index += 1
                else:
                    query += "{} ,".format(col)
            else:
                query += "({}-{})*1.0*({}) as {}{} ,".format(col, avgs[i], stds[i], col, suffix)

        for param in self.select_params:
            query += param + ", "

        query = query[:-2]

        query += " from " + self.table_name

        return query


class GradientBoostingClassifierSQL(object):

    def __init__(self, gbm, table_name):
        self.gbr_sql = GradientBoostingRegressorSQL(gbm, table_name)

    # def plot_tree_into_file(self, tree_index, file_name):
    #
    #     self.gbr_sql.plot_tree_into_file(tree_index, file_name)

    def get_params(self):
        return self.gbr_sql.get_params()

    def generate_sql_query(self, table_name=None):

        return self.gbr_sql.generate_sql_query(table_name)


class GradientBoostingRegressorSQL(object):

    def __init__(self, gbm, table_name):
        self.gbm = gbm
        self.table_name = table_name
        self.select_params = ["Id", "Score"]
        #self.select_params = ["Id", "PredictedLabel"]
        self.params = None
        self.init_score = getattr(gbm, 'init_score', 0)

    def _get_dtc_rules(self, estimator, X_features):

        n_nodes = estimator.tree_.node_count
        children_left = estimator.tree_.children_left
        children_right = estimator.tree_.children_right
        stack = [0]  # seed is the root node id
        n_leafs = 0
        leafs = []
        while len(stack) > 0:
            node_id = stack.pop()

            # If we have a test node
            if (children_left[node_id] != children_right[node_id]):
                stack.append(children_left[node_id])
                stack.append(children_right[node_id])
            else:
                n_leafs += 1
                leafs.append(node_id)

        # print("Num. decision tree rules: {}".format(n_leafs))

        decision_tree_rules = []

        def _get_lineage(tree, feature_names):
            left = tree.tree_.children_left
            right = tree.tree_.children_right
            threshold = tree.tree_.threshold
            features = [feature_names[i] for i in tree.tree_.feature]
            progress_rule = 0

            # get ids of child nodes
            idx = np.argwhere(left == -1)[:, 0]

            def recurse(left, right, child, lineage=None):
                if lineage is None:
                    lineage = [child]
                if child in left:
                    parent = np.where(left == child)[0].item()
                    split = 'l'
                else:
                    parent = np.where(right == child)[0].item()
                    split = 'r'

                lineage.append((parent, split, threshold[parent], features[parent]))

                if parent == 0:
                    lineage.reverse()
                    return lineage
                else:
                    return recurse(left, right, parent, lineage)

            for child in idx:
                for node in recurse(left, right, child):
                    # print(node)
                    decision_tree_rules.append(node)
                    progress_rule += 1
                    # print("{}/{}".format(progress_rule, n_nodes))

        _get_lineage(estimator, X_features)

        return estimator, decision_tree_rules

    def _convert_dtc_rules_in_string(self, tree, decision_tree_rules):

        rules_strings = []
        rule_string = "CASE WHEN "
        for item in decision_tree_rules:

            if not isinstance(item, tuple):  # the item is the index of a leaf node
                rule_string = rule_string[:-5]
                tree_score = tree.tree_.value[item][0][0]

                predicted_class = tree_score

                rule_string += " THEN {}".format(predicted_class)
                rules_strings.append(rule_string)
                rule_string = "WHEN "
            else:
                op = item[1]
                thr = item[2]
                if op == 'l':
                    op = '<='
                elif op == 'r':
                    op = '>'
                else:               # when op is equals to '=' or '<>' the thr is a string
                    thr = "'{}'".format(thr)
                feature_name = item[3]
                rule_string += "{} {} {} and ".format(feature_name, op, thr)

        return rules_strings

    # def plot_tree_into_file(self, tree_index, file_name):
    #
    #     trees = self.gbm.estimators_
    #     tree = trees[tree_index]
    #     #dot_data = export_graphviz(tree[0], out_file=None, feature_names=self.gbm.feature_names,
    #     #                           class_names=self.gbm.class_labels,
    #     #                           filled=True, rounded=True, special_characters=True)
    #     dot_data = export_graphviz(tree[0], out_file=None, feature_names=self.gbm.feature_names,
    #                               filled=True, rounded=True, special_characters=True)
    #     graph = graphviz.Source(dot_data)
    #     graph.render(file_name)

    def get_params(self):
        # extract decision rules from regressor decision trees
        trees = self.gbm.estimators_
        trees_parameters = []
        for index, tree in enumerate(trees):  # loop over trees

            estimator, decision_tree_rules = self._get_dtc_rules(tree[0], self.gbm.feature_names)
            rules_strings = self._convert_dtc_rules_in_string(tree[0], decision_tree_rules)
            # setting the weight of each tree as the learning rate
            tree_params = {"estimator": estimator, "string_rules": ' '.join(rules_strings), "weight": self.gbm.learning_rate, "rules": decision_tree_rules}
            trees_parameters.append(tree_params)

        self.params = trees_parameters

        return trees_parameters

    def _manage_one_hot_encoding_features_in_trees_rules(self, one_hot_encoder_feature_mapping):

        new_params = []
        for tree_index, tree_params in enumerate(self.get_params()):
            new_tree_params = tree_params.copy()
            tree = self.gbm.estimators_[tree_index][0]

            rules = tree_params["rules"]

            new_rules = []
            for rule in rules:
                if isinstance(rule, tuple):
                    op = rule[1]
                    thr = rule[2]
                    feature = rule[3]

                    if feature in one_hot_encoder_feature_mapping:
                        feature_mapping = one_hot_encoder_feature_mapping[feature]
                        new_feature = feature_mapping['col']
                        new_val = feature_mapping['value']
                        new_op = '='
                        if op == 'l':
                            if 0 <= thr:
                                new_op = '<>'
                        else:
                            if 0 > thr:
                                new_op = '<>'

                        new_rule = (rule[0], new_op, new_val, new_feature)
                    else:
                        new_rule = rule
                else:
                    new_rule = rule

                new_rules.append(new_rule)

            rules_strings = self._convert_dtc_rules_in_string(tree, new_rules)

            new_tree_params["rules"] = new_rules
            new_tree_params["string_rules"] = ' '.join(rules_strings)
            new_params.append(new_tree_params)

        self.params = new_params

    def _generate_sql_regression_tree(self, trees_parmas, table_name):

        query = "select "

        for i in range(len(trees_parmas)):
            tree_params = trees_parmas[i]
            tree_weight = tree_params["weight"]
            sql_case = tree_params["string_rules"]

            sql_case += " END AS tree_{},".format(i)

            query += sql_case

        for param in self.select_params:
            query += param + ", "

        query = query[:-2]

        query += " from " + table_name

        external_query = " select ("
        for i in range(len(trees_parmas)):
            tree_weight = trees_parmas[i]["weight"]
            external_query += "{} * tree_{} + ".format(tree_weight, i)
            #external_query += "tree_{} + ".format(i)

        external_query += "{} + ".format(self.init_score)

        external_query = external_query[:-2]
        external_query += ") AS PredictedScore,"

        for param in self.select_params:
            external_query += param + ","

        external_query = external_query[:-1]

        final_query = external_query + " from (" + query + " ) AS F"

        return final_query

    def generate_sql_query(self, table_name=None):

        #self.plot_tree_into_file(0, "/home/matteo/Scrivania/FlightDelayFirstTree")

        trees_parameters = self.params

        if not table_name:
            table_name = self.table_name
        query = self._generate_sql_regression_tree(trees_parameters, table_name)

        return query


class TfidfVectorizerSQL(object):

    def __init__(self, tfidf_transformer, table_name):
        self.tfidf_transformer = tfidf_transformer
        self.table_name = table_name
        self.select_params = ["Id", "Score"]
        self.params = None

    def get_params(self):
        vocabulary = self.tfidf_transformer.vocabulary_

        params = {"vocabulary": vocabulary}
        self.params = params

        return params

    def generate_sql_query(self):

        pass


class FeatureUnionSQL(object):
    def __init__(self, tfidf_transformer, table_name):
        self.tfidf_transformer = tfidf_transformer
        self.table_name = table_name
        self.select_params = ["Id", "Score"]
        self.params = None

    def get_params(self):

        final_vocabulary = {}
        reverse_final_vocabulary = {}
        w = 0
        for item in self.tfidf_transformer.get_params()["transformer_list"]:
            feature_model_name = item[0]
            feature_model = item[1]

            for token in feature_model.vocabulary_:

                if feature_model_name == "words":
                    w += 1
                    token_key = "w.{}".format(token)
                    final_vocabulary[token_key] = feature_model.vocabulary_[token]
                    reverse_final_vocabulary[feature_model.vocabulary_[token]] = token_key
                else:
                    token_key = "t.{}".format(token)
                    final_vocabulary[token_key] = feature_model.vocabulary_[token] + w
                    reverse_final_vocabulary[feature_model.vocabulary_[token] + w] = token_key

        params = {"vocabulary": final_vocabulary, "reverse_vocabulary": reverse_final_vocabulary}
        self.params = params

        return params

    def generate_sql_query(self):

        pass


class SGDClassifierSQL(object):

    def __init__(self, logistic, table_name):
        self.logistic = logistic
        self.table_name = table_name
        self.select_params = ["Id", "Score"]
        self.params = None

    def get_params(self):
        weights = self.logistic.coef_.ravel()
        bias = self.logistic.intercept_[0]

        params = {"weights": weights, "bias": bias}
        self.params = params

        return params

    def generate_sql_query(self, tablename):
        pass


class SGDRegressorSQL(object):

    def __init__(self, sgd, table_name):
        self.sgd = sgd
        self.table_name = table_name
        self.select_params = ["Id", "Score"]
        self.params = None

    def get_params(self):
        weights = self.sgd.coef_.ravel()
        bias = self.sgd.intercept_[0]

        params = {"weights": weights, "bias": bias}
        self.params = params

        return params

    def _generate_sql_multiply_by_linear_combination(self, weights, columns, suffix, table_name):
        query = "select "
        for i in range(len(columns)):
            col = columns[i]
            query += "({} * {}) as {}{} ,".format(col, weights[i], col, suffix)

        for param in self.select_params:
            query += param + ","

        query = query[:-1]

        if not table_name:
            table_name = self.table_name
        query += " from {}".format(table_name)

        return query

    def _generare_sql_sum(self, bias, columns, table_name=None):

        query = "select "
        query += " ( "
        for col in columns:
            query += "{} +".format(col)

        query += "{}".format(bias)

        query += ") as PredictedScore,"

        for param in self.select_params:
            query += param + ","

        query = query[:-1]

        if not table_name:
            table_name = self.table_name
        query += " from {}".format(table_name)

        return query

    def generate_sql_query(self, table_name=None, suffix=''):

        params = self.get_params()
        weights = params["weights"]
        bias = params["bias"]
        columns = self.sgd.feature_names

        subquery = self._generate_sql_multiply_by_linear_combination(weights, columns, suffix, table_name)
        query = self._generare_sql_sum(bias, columns, "({}) as F ".format(subquery))

        return query


class SDCARegressorSQL(object):
    def __init__(self, sdca_regressor, table_name):
        self.sdca_regressor = sdca_regressor
        self.table_name = table_name
        self.select_params = ["Id", "Score"]
        self.params = None

    def get_params(self):
        #weights = self.sdca_regressor.coef_.ravel()
        weights = self.sdca_regressor.coef_.T.ravel()
        bias = 0
        if hasattr(self.sdca_regressor, "intercept_"):
            bias = self.sdca_regressor.intercept_
            # bias = self.sdca_regressor.intercept_[0]

        params = {"weights": weights, "bias": bias}
        self.params = params

        return params

    def _generate_sql_multiply_by_linear_combination(self, weights, columns, suffix, table_name):
        query = "select "
        for i in range(len(columns)):
            col = columns[i]
            query += "({} * {}) as {}{} ,".format(col, weights[i], col, suffix)

        for param in self.select_params:
            query += param + ","

        query = query[:-1]

        if not table_name:
            table_name = self.table_name
        query += " from {}".format(table_name)

        return query

    def _generare_sql_sum_and_exp_columns(self, bias, columns, table_name=None):

        query = "select "
        query += " ( "
        for col in columns:
            query += "{} +".format(col)

        query += "{}".format(bias)

        query += ") as TotScore,"

        for param in self.select_params:
            query += param + ","

        query = query[:-1]

        if not table_name:
            table_name = self.table_name
        query += " from {}".format(table_name)

        return query

    def generate_sql_query(self, table_name=None, suffix=''):

        params = self.get_params()
        weights = params["weights"]
        bias = params["bias"]
        columns = self.sdca_regressor.feature_names
        print(weights)
        print(bias)
        print(columns)

        subquery = self._generate_sql_multiply_by_linear_combination(weights, columns, suffix, table_name)
        query = self._generare_sql_sum_and_exp_columns(bias, columns, "({}) as F ".format(subquery))

        return query


class PoissonRegressionSQL(object):
    def __init__(self, regressor, table_name):
        self.regressor = regressor
        self.table_name = table_name
        self.select_params = ["Id", "Score"]
        self.params = None

    def get_params(self):
        weights = self.regressor.params
        bias = getattr(self.regressor, "offset", 0)
        #exposure = getattr(self.regressor, "exposure", 0)

        params = {"weights": weights, "bias": bias}
        self.params = params

        return params

    def _generate_sql_multiply_by_linear_combination(self, weights, columns, suffix, table_name):
        query = "select "
        for i in range(len(columns)):
            col = columns[i]
            query += "({} * {}) as {}{} ,".format(col, weights[i], col, suffix)

        for param in self.select_params:
            query += param + ","

        query = query[:-1]

        if not table_name:
            table_name = self.table_name
        query += " from {}".format(table_name)

        return query

    def _generare_sql_sum_and_exp_columns(self, bias, columns, table_name=None):

        query = "select "
        query += " EXP ( "
        for col in columns:
            query += "{} +".format(col)

        query += "{}".format(bias)

        query += ") as PredictedScore,"

        for param in self.select_params:
            query += param + ","

        query = query[:-1]

        if not table_name:
            table_name = self.table_name
        query += " from {}".format(table_name)

        return query

    def generate_sql_query(self, table_name=None, suffix=''):

        params = self.get_params()
        weights = params["weights"]
        bias = params["bias"]
        columns = self.regressor.feature_names

        subquery = self._generate_sql_multiply_by_linear_combination(weights, columns, suffix, table_name)
        query = self._generare_sql_sum_and_exp_columns(bias, columns, "({}) as F ".format(subquery))

        return query


class LogisticRegressionSQL(object):
    def __init__(self, classifier, table_name):
        self.classifier = classifier
        self.table_name = table_name
        self.select_params = ["Id"]
        self.params = None

    def get_params(self):
        multi_weights = self.classifier.coef_
        multi_bias = self.classifier.intercept_

        params = {"weights": multi_weights, "bias": multi_bias}
        self.params = params

        return params

    def _generate_linear_combination(self, weights, bias):

        query = ""
        columns = self.classifier.feature_names
        for i in range(len(columns)):
            c = columns[i]
            query += "(`{}`*{}) + ".format(c, weights[i])
        query = "{} {}".format(query, bias)

        return query

    def _get_raw_query(self, multi_weights, multi_bias, table_name=None, suffix='', include_where_clause=False):

        query_iternal = "select "
        wildcard = "class_"

        for param in self.select_params:
            query_iternal += param + ","

        for i in range(len(multi_weights)):
            weights = multi_weights[i]
            bias = multi_bias[i]

            q1 = self._generate_linear_combination(weights, bias)

            query_iternal += "({}) as {}{},".format(q1, wildcard, i)

        query_iternal = query_iternal[:-1]

        query_iternal += "\n from {}".format(self.table_name)

        #return query_iternal

        if include_where_clause:
            where_clause = "\n where Id >= @id  and Id < ( @id + chuncksize ); \n"
            query_iternal += where_clause

        query_iternal = " ( {} ) as F ".format(query_iternal)

        query = "select "
        for param in self.select_params:
            query += param + ","

        sum = "("
        for i in range(len(multi_bias)):
            sum += "EXP({}{})+".format(wildcard, i)
        sum = sum[:-1]
        sum += ")"

        for i in range(len(multi_bias)):
            query += "(" + "EXP({}{}) / {} ) as {}{},".format(wildcard, i, sum, wildcard, i)

        query = query[:-1]
        query = "{}\n from {}".format(query, query_iternal)

        return query

    def generate_sql_query(self, table_name=None, suffix='', include_where_clause=False):

        params = self.get_params()
        multi_weights = params["weights"]
        multi_bias = params["bias"]
        columns = self.classifier.feature_names

        query_dict = {}
        query = self._get_raw_query(multi_weights, multi_bias, table_name=table_name, suffix=suffix, include_where_clause=include_where_clause)
        query_dict["efficiency"] = query

        external_query = "select L.Id, "
        for i in range(len(multi_bias)):
            external_query += "class_{},".format(i)
        for i in range(len(multi_bias)):
            external_query += "Probability_{},".format(i)
        external_query = external_query[:-1]

        external_query = external_query + " from ({}) as L INNER JOIN {} ON (L.Id={}.Id)".format(query, self.table_name, self.table_name)
        query_dict["effectiveness"] = external_query

        return query_dict


#class BoosterSQL(object):
class XGBRegressorSQL(object):
    def __init__(self, model, table_name):
        self.model = model
        self.table_name = table_name
        self.select_params = ["Id", "Score"]
        #self.select_params = ["Id", "PredictedLabel"]
        self.params = None

    def _get_dtc_rules(self, tree_string):

        tree_rules = {}
        paths = []
        path = []
        last_depth = -1
        tree_index = -1
        for line in tree_string.split('\n'):

            if not line.startswith("booster") and line != '':   # the line corresponds to a condition of a tree rule or
                                                                # a tree score (that is located in a leaf of the tree)
                _id, rest = line.split(':')
                depth = _id.count('\t') # the number of tabs identifies the condition depth in the rule of the tree
                _id = int(_id.lstrip())

                if rest[:4] != 'leaf':                      # the line corresponds to a condition of a tree rule
                    feature = rest.split('<')[0][1:]
                    highest_lower_threshold = float(rest.split('<')[1].split(']')[0])

                    # compare the depth of the previous condition with the one of the current condition
                    if last_depth == -1 or last_depth <= depth: # if it is the first condition or the depth of the
                                                                # current condition is greater or equal than the
                                                                # previous one

                        # if the depths are equal:
                        # 1) terminate the previous rule and store it
                        # 2) copy the previous rule
                        # 3) invert its last condition and create a new rule
                        # 4) append to the new rule the current condition
                        if last_depth == depth:
                            item = list(path[-1])   # last condition of previous rule
                            item[1] = 'r'           # the condition are expressed only with <=, so substituting <= with > it is possible to invert the condition
                            path = path[:-1]        # create new rule
                            path.append(tuple(item))# append the inverted condition

                        path.append((_id, 'l', highest_lower_threshold, feature)) # add current condition

                    else:               # if the depth of the current condition is smaller than the previous one

                        # 1) terminate the previous rule and store it
                        # 2) copy the previous rule
                        # 3) identify the condition of the previous rule corresponding to the current level of depth
                        # 4) invert this condition and create a new rule
                        # 5) append to the new rule the current condition

                        depth_difference = abs((last_depth - depth) - 1)
                        path = path[:-1 - depth_difference]
                        item = list(path[-1])
                        item[1] = 'r'
                        path = path[:-1]
                        path.append(tuple(item))
                        path.append((_id, 'l', highest_lower_threshold, feature))

                else:               # the line corresponds to tree score (that is located in a leaf of the tree)
                    leaf_value = float(rest.split('=')[1])

                    # compare the depth of the previous condition with the one of the current condition
                    if last_depth == -1 or last_depth <= depth: # if it is the first condition or the depth of the
                                                                # current condition is greater or equal than the
                                                                # previous one

                        # if the depths are equal:
                        # 1) terminate the previous rule and store it
                        # 2) copy the previous rule
                        # 3) invert its last condition and create a new rule
                        # 4) append to the new rule the current condition
                        if last_depth == depth:
                            item = list(path[-1])
                            item[1] = 'r'
                            path = path[:-1]
                            path.append(tuple(item))

                        path.append(leaf_value)
                    else:           # if the depth of the current condition is smaller than the previous one

                        # 1) terminate the previous rule and store it
                        # 2) copy the previous rule
                        # 3) identify the condition of the previous rule corresponding to the current level of depth
                        # 4) invert this condition and create a new rule
                        # 5) append to the new rule the current condition
                        depth_difference = abs((last_depth - depth) - 1)
                        path = path[:-1 - depth_difference]
                        item = list(path[-1])
                        item[1] = 'r'
                        path = path[:-1]
                        path.append(tuple(item))
                        path.append(leaf_value)

                    paths.append(path)
                    path = path[:-1]

                last_depth = depth

            else:           # the line corresponds to an empty string or a string that identifies the considered tree

                if tree_index != -1:

                    flat_paths = []
                    for path in paths:
                        for p in path:
                            flat_paths.append(p)

                    tree_rules[tree_index] = flat_paths
                    paths = []
                    path = []
                    last_depth = -1

                tree_index += 1

        return tree_rules

    def _convert_dtc_rules_in_string(self, decision_tree_rules):

        rules_strings = []
        rule_string = "CASE WHEN "
        for item in decision_tree_rules:

            if not isinstance(item, tuple):  # the item is the score of a leaf node
                rule_string = rule_string[:-5]
                tree_score = item

                rule_string += " THEN {}".format(tree_score)
                rules_strings.append(rule_string)
                rule_string = "WHEN "
            else:
                op = item[1]
                if op == 'l':
                    #op = '<='
                    op = '<'
                else:
                    #op = '>'
                    op = '>='
                thr = item[2]
                feature_name = item[3]
                rule_string += "{} {} {} and ".format(feature_name, op, thr)

        return rules_strings

    def get_params(self):
        # extract decision rules from regressor decision trees
        model_file = os.path.abspath('assets/bike_sharing_tweedie_trees.txt')
        self.model.get_booster().dump_model(model_file)
        with open(model_file, 'r') as f:
            txt_model = f.read()
        tree_rules = self._get_dtc_rules(txt_model)
        #exit(1)

        #from xgboost import plot_tree
        #import matplotlib.pyplot as plt
        #plot_tree(self.model, num_trees=1)
        #plt.show()

        trees_parameters = []
        for tree_index in tree_rules:  # loop over trees

            rules_strings = self._convert_dtc_rules_in_string(tree_rules[tree_index])
            # setting the weight of each tree as the learning rate
            #tree_params = {"string_rules": ' '.join(rules_strings), "weight": self.model.learning_rate}
            tree_params = {"estimator": self.model, "string_rules": ' '.join(rules_strings), "weight": self.model.learning_rate,
             "rules": tree_rules}
            trees_parameters.append(tree_params)

        self.params = trees_parameters

        return trees_parameters

    def _generate_sql_regression_tree(self, trees_parmas, table_name):

        query = "select "

        for i in range(len(trees_parmas)):
            tree_params = trees_parmas[i]
            tree_weight = tree_params["weight"]
            sql_case = tree_params["string_rules"]

            sql_case += " END AS tree_{},".format(i)

            query += sql_case

        for param in self.select_params:
            query += param + ", "

        query = query[:-2]

        query += " from " + table_name

        external_query = " select (EXP("
        for i in range(len(trees_parmas)):
            tree_weight = trees_parmas[i]["weight"]
            #external_query += "{} * tree_{} + ".format(tree_weight, i)
            external_query += "tree_{} + ".format(i)
            #external_query += "EXP(tree_{}) + ".format(i)

        external_query = external_query[:-2]
        external_query += ")/2) AS PredictedScore,"

        for param in self.select_params:
            external_query += param + ","

        external_query = external_query[:-1]

        final_query = external_query + " from (" + query + " ) AS F"

        return final_query

    def generate_sql_query(self, table_name=None):

        trees_parameters = self.get_params()

        if not table_name:
            table_name = self.table_name
        query = self._generate_sql_regression_tree(trees_parameters, table_name)
        print(query)

        return query
