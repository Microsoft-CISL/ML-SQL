use mltosql;
go
CREATE TABLE flight_delay_FastTree(
Year int,
Month int,
DayofMonth int,
DayOfWeek int,
DepTime float,
CRSDepTime int,
ArrTime float,
CRSArrTime int,
UniqueCarrier varchar(255),
FlightNum int,
ActualElapsedTime float,
CRSElapsedTime float,
AirTime float,
ArrDelay float,
DepDelay float,
Origin varchar(255),
Dest varchar(255),
Distance int,
TaxiIn float,
TaxiOut float,
Cancelled int,
Diverted int,
CarrierDelay float,
WeatherDelay float,
NASDelay float,
SecurityDelay float,
Label float,
Id int PRIMARY KEY,
Score float
);
go

CREATE TABLE flight_delay_with_output (
Id int,
Score FLOAT);
go


CREATE TABLE flight_delay_tree_scores (
TreeId varchar(255),
TreeScore double,
Id int
);
go