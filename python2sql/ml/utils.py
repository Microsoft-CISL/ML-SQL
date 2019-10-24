from sklearn.metrics import *
from sklearn.metrics.classification import unique_labels
from math import sqrt
import pandas as pd


def _save_evaluation_metrics(metric_scores, output_dir, suffix):
    metrics_scores_df = pd.DataFrame.from_dict(metric_scores, orient='index', columns=["score"])
    metrics_scores_df.to_csv("{}/{}_evaluation_metrics.csv".format(output_dir, suffix))


def _get_binary_classification_metric_scores(y_test, y_pred):

    metric_scores = {}
    labels = unique_labels(y_test, y_pred)
    target_names = [u'%s' % l for l in labels]
    p, r, f1, s = precision_recall_fscore_support(y_test, y_pred,
                                                  labels=labels,
                                                  average=None,
                                                  sample_weight=None)
    rows = zip(target_names, p, r, f1, s)
    neg_prec = 0
    neg_rec = 0
    pos_prec = 0
    pos_rec = 0
    for row in rows:
        if row[0] == '0':
            neg_prec = row[1]
            neg_rec = row[2]
        elif row[0] == '1':
            pos_prec = row[1]
            pos_rec = row[2]
    auc = roc_auc_score(y_test, y_pred)
    accuracy = accuracy_score(y_test, y_pred)
    # precision, recall, thresholds = precision_recall_curve(y_test, y_pred)
    f1 = f1_score(y_test, y_pred)
    loss = log_loss(y_test, y_pred)

    metric_scores["accuracy"] = accuracy
    metric_scores["positive_precision"] = pos_prec
    metric_scores["positive_recall"] = pos_rec
    metric_scores["negative_precision"] = neg_prec
    metric_scores["negative_recall"] = neg_rec
    metric_scores["f1"] = f1
    metric_scores["auc"] = auc
    metric_scores["log_loss"] = loss

    return metric_scores


def _print_binary_classification_metrics_on_console(metric_scores, classifier_name):
    accuracy = metric_scores["accuracy"]
    pos_prec = metric_scores["positive_precision"]
    pos_rec = metric_scores["positive_recall"]
    neg_prec = metric_scores["negative_precision"]
    neg_rec = metric_scores["negative_recall"]
    f1 = metric_scores["f1"]
    auc = metric_scores["auc"]
    loss = metric_scores["log_loss"]

    print("{}".format("*" * 60))
    print("*       Metrics for {} binary classification model      ".format(classifier_name))
    print("*{}".format("-" * 59))
    print("*       Accuracy: {}".format(accuracy))
    print("*       PositivePrecision:  {}".format(pos_prec))
    print("*       PositiveRecall:  {}".format(pos_rec))
    print("*       NegativePrecision:  {}".format(neg_prec))
    print("*       NegativeRecall:  {}".format(neg_rec))
    print("*       F1Score:  {}".format(f1))
    print("*       Area Under Curve:      {}".format(auc))
    print("*       LogLoss:  {}".format(loss))
    print("*" * 60)


def evaluate_binary_classification_results(classifier_name, y_test, y_pred, output_dir):

    print("[BEGIN] STARTING CLASSIFIER EVALUTATION...")

    metric_scores = _get_binary_classification_metric_scores(y_test, y_pred)
    _print_binary_classification_metrics_on_console(metric_scores, classifier_name)
    _save_evaluation_metrics(metric_scores, output_dir, classifier_name)

    print("[END] CLASSIFIER EVALUTATION COMPLETED.\n")


def _get_regression_metric_scores(y_test, y_pred):
    metric_scores = {}
    # R squared
    r2score = r2_score(y_test, y_pred)
    # mean absolute error
    absolute_loss = mean_absolute_error(y_test, y_pred)
    # mean squared error
    squared_loss = mean_squared_error(y_test, y_pred)
    # root mean squared error
    rms_loss = sqrt(mean_squared_error(y_test, y_pred))

    metric_scores["r2_score"] = r2score
    metric_scores["absolute_loss"] = absolute_loss
    metric_scores["squared_loss"] = squared_loss
    metric_scores["rms_loss"] = rms_loss

    return metric_scores


def _print_regression_metrics_on_console(metric_scores, regression_method):
    r2score = metric_scores["r2_score"]
    absolute_loss = metric_scores["absolute_loss"]
    squared_loss = metric_scores["squared_loss"]
    rms_loss = metric_scores["rms_loss"]

    print("{}".format("*" * 60))
    print("*       Metrics for {} regression model      ".format(regression_method))
    print("*{}".format("-" * 59))
    print("*       R2 Score:      {}".format(r2score))
    print("*       Absolute loss:  {}".format(absolute_loss))
    print("*       Squared loss:  {}".format(squared_loss))
    print("*       RMS loss:  {}".format(rms_loss))
    print("*" * 60)


def evaluate_regression_results(regression_method, y_test, y_pred, output_dir):

    print("[BEGIN] STARTING REGRESSION EVALUTATION...")

    metric_scores = _get_regression_metric_scores(y_test, y_pred)
    _print_regression_metrics_on_console(metric_scores, regression_method)
    _save_evaluation_metrics(metric_scores, output_dir, regression_method)

    print("[END] REGRESSION EVALUTATION COMPLETED.\n")


def _get_multi_classification_metric_scores(y_test, y_pred, pred_prob):
    metric_scores = {}
    accuracy_macro = recall_score(y_test, y_pred, average='macro')
    accuracy_micro = recall_score(y_test, y_pred, average='micro')
    log_loss_score = log_loss(y_test, pred_prob)

    metric_scores["accuracy_macro"] = accuracy_macro
    metric_scores["accuracy_micro"] = accuracy_micro
    metric_scores["log_loss"] = log_loss_score

    return metric_scores


def _print_multi_classification_metrics_on_console(metric_scores, multi_classifcation_method):
    accuracy_macro = metric_scores["accuracy_macro"]
    accuracy_micro = metric_scores["accuracy_micro"]
    log_loss_score = metric_scores["log_loss"]

    print("{}".format("*" * 60))
    print("*       Metrics for {} multi-classifier model      ".format(multi_classifcation_method))
    print("*{}".format("-" * 59))
    print("*       AccuracyMacro: {}".format(accuracy_macro))
    print("*       AccuracyMicro:      {}".format(accuracy_micro))
    print("*       LogLoss:  {}".format(log_loss_score))
    print("*" * 60)


def evaluate_multi_classification_results(multi_classifcation_method, y_test, y_pred, pred_prob, output_dir):

    print("[BEGIN] STARTING MULTI-CLASSIFIER EVALUTATION...")

    metric_scores = _get_multi_classification_metric_scores(y_test, y_pred, pred_prob)
    _print_multi_classification_metrics_on_console(metric_scores, multi_classifcation_method)
    _save_evaluation_metrics(metric_scores, output_dir, multi_classifcation_method)

    print("[END] MULTI-CLASSIFIER EVALUTATION COMPLETED.\n")

