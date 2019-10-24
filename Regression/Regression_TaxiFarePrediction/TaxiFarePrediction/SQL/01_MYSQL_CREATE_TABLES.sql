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

create table weights_taxi(
label varchar(100) PRIMARY KEY,
weight float
);

create table taxi_fare_with_score_output(
Id int,
Score float
);