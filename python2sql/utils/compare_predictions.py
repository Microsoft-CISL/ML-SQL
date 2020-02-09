import pandas as pd
from sklearn.metrics import classification_report

# SENTIMENT ANALYSIS
#pred_data = pd.read_csv("/home/matteo/PycharmProjects/mlsql/BinaryClassification/SentimentAnalysis/sentiment_analysis_sklearn/assets/output/chunk_predictions_new.csv")
#true_data = pd.read_csv("/home/matteo/Scrivania/predictions_ml_net.csv", header=None, names=["prediction"])

# CREDIT CARD
#sklearn_pred_data = pd.read_csv("/home/matteo/PycharmProjects/mlsql/BinaryClassification/CreditCardFraudDetection/credit_card_scikit_learn/assets/output/chunk_predictions_new.csv")
#ml_net_pred_data = pd.read_csv("/home/matteo/Scrivania/credit_card_predictions_ml_net.csv", header=None, names=["prediction", "true"])

# HEART DISEASE
sklearn_pred_data = pd.read_csv("/home/matteo/PycharmProjects/mlsql/BinaryClassification/HeartDiseaseDetection/heart_disease_sklearn/assets/output/chunk_predictions_new.csv")
ml_net_pred_data = pd.read_csv("/home/matteo/Scrivania/heart_disease_predictions_ml_net.csv", header=None, names=["prediction", "true"])

sklearn_pred = sklearn_pred_data["prediction"]
sklearn_true = sklearn_pred_data["true"]
ml_net_pred = ml_net_pred_data["prediction"]
ml_net_true = ml_net_pred_data["true"]

sklearn_match = 0
ml_net_match = 0
for index in range(len(sklearn_true)):
    sklearn_pred1 = sklearn_pred[index]
    ml_net_pred2 = ml_net_pred[index]
    true = sklearn_true[index]
    if sklearn_pred1 == true:
        sklearn_match += 1
    if ml_net_pred2 == true:
        ml_net_match += 1
print("[SKLEARN] Number of correct predicted samples: {}/{}".format(sklearn_match, len(sklearn_true)))
print("[ML.NET] Number of correct predicted samples: {}/{}".format(ml_net_match, len(sklearn_true)))
print()

print("[SKLEARN] Number of samples predicted as positive: {}".format(sum(sklearn_pred.values)))
print("[ML.NET] Number of samples predicted as positive: {}".format(sum(ml_net_pred.values)))
print("Number of positive samples: {}".format(sum(sklearn_true.values)))
print()

sklearn_positive_match = 0
ml_net_positive_match = 0
for index in range(len(sklearn_pred)):
    sklearn_pred1 = sklearn_pred[index]
    ml_net_pred2 = ml_net_pred[index]
    true = sklearn_true[index]
    if sklearn_pred1 == true and sklearn_pred1 == 1:
        sklearn_positive_match += 1
    if ml_net_pred2 == true and ml_net_pred2 == 1:
        ml_net_positive_match += 1
print("[SKLEARN] Number of correct positive-predicted samples: {}".format(sklearn_positive_match))
print("[ML.NET] Number of correct positive-predicted samples: {}".format(ml_net_positive_match))
print()

print("Comparison SKLEARN-ML.NET (target)")
print(classification_report(ml_net_pred, sklearn_pred, target_names=["0", "1"]))
print()
print("Evaluation SKLEARN")
print(classification_report(sklearn_true, sklearn_pred, target_names=["0", "1"]))
print()
print("Evaluation ML.NET")
print(classification_report(ml_net_true, ml_net_pred, target_names=["0", "1"]))






