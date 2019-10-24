use mltosql
go
CREATE TABLE iris_with_score (
Label float,
SepalLength float,
SepalWidth float,
PetalLength float,
PetalWidth float,
Id int,
score_0 float,
score_1 float,
score_2 float
);
go

CREATE TABLE iris_with_score_output (
Id int,
score_0 float,
score_1 float,
score_2 float
);
go