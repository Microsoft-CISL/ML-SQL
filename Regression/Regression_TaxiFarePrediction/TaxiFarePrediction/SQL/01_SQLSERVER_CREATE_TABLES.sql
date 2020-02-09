use mltosql
go
create table taxi_fare_with_score (
VendorId varchar(100),
RateCode varchar(100),
PassengerCount float,
TripTime float,
TripDistance float,
PaymentType varchar(100),
FareAmount float,
Id int,
Score float
);
go
create table weights_taxi(
label varchar(100) PRIMARY KEY,
weight float
);
go
create table taxi_fare_with_score_output(
Id int,
Score float
);
go