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
Score float
);
go

CREATE TABLE credit_card_with_score_output (
Id float,
Score float);
go

CREATE TABLE credit_card_normalized_data (
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
Id int PRIMARY KEY
);
go

CREATE TABLE credit_card_tree_scores (
TreeId varchar(255),
TreeScore float,
Id int
);
go