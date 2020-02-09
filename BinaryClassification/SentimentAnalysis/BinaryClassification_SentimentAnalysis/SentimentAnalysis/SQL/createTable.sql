create table sentiment_analysis_detection_with_score (
Label Boolean ,
Comment varchar(10000),
Id int,
PredictedLabel Boolean,
Probability float,
Score float
);

create table weights_sentiment_analysis(
label varchar(10000),
weight float
);

create table sentiment_with_score_output(
Id int,
Score float
);