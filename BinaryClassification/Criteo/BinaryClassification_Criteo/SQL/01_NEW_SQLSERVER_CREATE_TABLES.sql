use mltosql;
go
CREATE TABLE criteo_FastTree(
Label int,
FeatureInteger1 float,
FeatureInteger2 float,
FeatureInteger3 float,
FeatureInteger4 float,
FeatureInteger5 float,
FeatureInteger6 float,
FeatureInteger7 float,
FeatureInteger8 float,
FeatureInteger9 float,
FeatureInteger10 float,
FeatureInteger11 float,
FeatureInteger12 float,
FeatureInteger13 float,
CategoricalFeature1 varchar(255),
CategoricalFeature2 varchar(255),
CategoricalFeature3 varchar(255),
CategoricalFeature4 varchar(255),
CategoricalFeature5 varchar(255),
CategoricalFeature6 varchar(255),
CategoricalFeature7 varchar(255),
CategoricalFeature8 varchar(255),
CategoricalFeature9 varchar(255),
CategoricalFeature10 varchar(255),
CategoricalFeature11 varchar(255),
CategoricalFeature12 varchar(255),
CategoricalFeature13 varchar(255),
CategoricalFeature14 varchar(255),
CategoricalFeature15 varchar(255),
CategoricalFeature16 varchar(255),
CategoricalFeature17 varchar(255),
CategoricalFeature18 varchar(255),
CategoricalFeature19 varchar(255),
CategoricalFeature20 varchar(255),
CategoricalFeature21 varchar(255),
CategoricalFeature22 varchar(255),
CategoricalFeature23 varchar(255),
CategoricalFeature24 varchar(255),
CategoricalFeature25 varchar(255),
CategoricalFeature26 varchar(255),
Id int PRIMARY KEY,
Score float
);
go


CREATE TABLE criteo_with_output (
Id int,
Score FLOAT);
go

CREATE TABLE criteo_tree_scores (
TreeId varchar(255),
TreeScore double,
Id int
);
go