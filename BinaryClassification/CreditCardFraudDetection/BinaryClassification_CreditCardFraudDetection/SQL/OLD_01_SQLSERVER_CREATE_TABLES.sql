use mltosql;
go
CREATE TABLE creditcard_with_prediction (
Tim float,
V1 float,
V2 float,
V3 float,
V4 float,
V5 float,
V6 float,
V7 float,
V8 float,
V9 float,
V10 float,
V11 float,
V12 float,
V13 float,
V14 float,
V15 float,
V16 float,
V17 float,
V18 float,
V19 float,
V20 float,
V21 float,
V22 float,
V23 float,
V24 float,
V25 float,
V26 float,
V27 float,
V28 float,
Amount float,
Label int,
Id float PRIMARY KEY,
PredictedLabel int,
Probability float,
Score float
);
go

CREATE TABLE credit_card_with_score_output (
Id float,
Score float);
go