use mltosql
go
CREATE TABLE heart_disease_detection_with_score (
Age float,
Sex float,
Cp float,
TrestBps float,
Chol float,
Fbs float,
RestEcg float,
Thalac float,
Exang float,
OldPeak float,
Slope float,
Ca float,
Thal float,
Label bit,
Id int,
PredictedLabel  bit,
Probability float,
Score float
);
go

CREATE TABLE heart_disease_detection_with_score_output (
Id int,
Score float
);
go

-- drop table heart_disease_detection_with_score;
-- drop table heart_disease_detection_with_score_output;