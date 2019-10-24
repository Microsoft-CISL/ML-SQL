CREATE TABLE creditcard_with_prediction (
Tim int,
V1 FLOAT,
V2 FLOAT,
V3 FLOAT,
V4 FLOAT,
V5 FLOAT,
V6 FLOAT,
V7 FLOAT,
V8 FLOAT,
V9 FLOAT,
V10 FLOAT,
V11 FLOAT,
V12 FLOAT,
V13 FLOAT,
V14 FLOAT,
V15 FLOAT,
V16 FLOAT,
V17 FLOAT,
V18 FLOAT,
V19 FLOAT,
V20 FLOAT,
V21 FLOAT,
V22 FLOAT,
V23 FLOAT,
V24 FLOAT,
V25 FLOAT,
V26 FLOAT,
V27 FLOAT,
V28 FLOAT,
Amount FLOAT,
Label boolean,
Id int PRIMARY KEY,
PredictedLabel boolean,
Probability FLOAT,
Score FLOAT
);


drop table creditcard_with_prediction;


CREATE TABLE credit_card_with_score_output (
Id int,
Score FLOAT);
