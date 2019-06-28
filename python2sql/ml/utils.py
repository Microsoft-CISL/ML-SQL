from sklearn.metrics import *
from sklearn.metrics.classification import unique_labels


def evaluate_binary_classification_results(classifier_name, y_test, y_pred):
    # See https://scikit-learn.org/stable/modules/model_evaluation.html#classification-metrics

    print("[BEGIN] STARTING CLASSIFIER EVALUTATION...")

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
    aus = roc_auc_score(y_test, y_pred)
    accuracy = accuracy_score(y_test, y_pred)
    precision, recall, thresholds = precision_recall_curve(y_test, y_pred)
    f1 = f1_score(y_test, y_pred)
    loss = log_loss(y_test, y_pred)

    print("{}".format("*"*60))
    print("*       Metrics for {} binary classification model      ".format(classifier_name))
    print("*{}".format("-"*59))
    print("*       Accuracy: {}".format(accuracy))
    print("*       Area Under Curve:      {}".format(aus))
    print("*       Area under Precision recall Curve:  {}".format((precision, recall)))
    print("*       F1Score:  {}".format(f1))
    print("*       LogLoss:  {}".format(loss))
    print("*       LogLossReduction:  {}".format(loss))
    print("*       PositivePrecision:  {}".format(pos_prec))
    print("*       PositiveRecall:  {}".format(pos_rec))
    print("*       NegativePrecision:  {}".format(neg_prec))
    print("*       NegativeRecall:  {}".format(neg_rec))
    print("*" * 60)

    print("[END] CLASSIFIER EVALUTATION COMPLETED.\n")