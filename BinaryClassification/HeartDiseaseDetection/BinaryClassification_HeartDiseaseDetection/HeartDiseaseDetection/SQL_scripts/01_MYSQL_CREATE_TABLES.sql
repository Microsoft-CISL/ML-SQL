create table heart_disease_detection_with_score (
Age int,
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
Label boolean,
Id int,
PredictedLabel  boolean,
Probability float,
Score float
);

#drop table heart_disease_detection_with_score;

create table heart_disease_detection_with_score_output (
Id int,
Score float
);