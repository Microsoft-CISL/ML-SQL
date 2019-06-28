import numpy as np
import graphviz
from sklearn.tree import export_graphviz


class StandardScalerSQL(object):

    def __init__(self, scaler, table_name):
        self.scaler = scaler
        self.table_name = table_name
        self.select_params = ["Id", "Score"]
        self.params = None

    def get_params(self):
        scaler_mean = self.scaler.mean_
        scaler_var = self.scaler.var_

        params = {"avgs": scaler_mean, "vars": scaler_var}
        self.params = params

        return params

    def generate_sql_query(self):

        self.get_params()

        avgs = self.params["avgs"]
        vars = self.params["vars"]
        columns = self.scaler.feature_names
        suffix = ""

        query = "select "

        for i in range(len(columns)):
            col = columns[i]
            query += "({}-{})*1.0*({}) as {}{} ,".format(col, avgs[i], vars[i], col, suffix)

        for param in self.select_params:
            query += param + ", "

        query = query[:-1]

        query += " from " + self.table_name

        return query


class GradientBoostingClassifierSQL(object):

    def __init__(self, gbm, table_name):
        self.gbm = gbm
        self.table_name = table_name
        self.select_params = ["Id", "Score"]
        self.params = None

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

                # FIXME: i have empirically determined a threshold of 100
                predicted_class = -1
                if tree_score >= 100:
                    predicted_class = 1
                else:
                    predicted_class = 0

                rule_string += " THEN {}".format(predicted_class)
                rules_strings.append(rule_string)
                rule_string = "WHEN "
            else:
                op = item[1]
                if op == 'l':
                    op = '<='
                else:
                    op = '>'
                thr = item[2]
                feature_name = item[3]
                rule_string += "{} {} {} and ".format(feature_name, op, thr)

        return rules_strings

    def plot_tree_into_file(self, tree_index, file_name):

        trees = self.gbm.estimators_
        tree = trees[tree_index]
        dot_data = export_graphviz(tree[0], out_file=None, feature_names=self.gbm.feature_names,
                                   class_names=self.gbm.class_labels,
                                   filled=True, rounded=True, special_characters=True)
        graph = graphviz.Source(dot_data)
        graph.render(file_name)

    def get_params(self):
        # extract decision rules from regressor decision trees
        trees = self.gbm.estimators_
        trees_parameters = []
        for index, tree in enumerate(trees):  # loop over trees

            estimator, decision_tree_rules = self._get_dtc_rules(tree[0], self.gbm.feature_names)
            rules_strings = self._convert_dtc_rules_in_string(tree[0], decision_tree_rules)
            # FIXME: weight set to 1. It seems that the weights are given by the evolving learning rate
            tree_params = {"estimator": estimator, "string_rules": ' '.join(rules_strings), "weight": 1, "rules": decision_tree_rules}
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

        query = query[:-1]

        query += " from " + table_name

        return query

    def generate_sql_query(self, table_name=None):

        trees_parameters = self.get_params()

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
        # TODO
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

    def generate_sql_query(self):
        # TODO
        pass
