delimiter //
set names utf8;
drop procedure if exists MLtoSQL.taxi_fare;
CREATE procedure MLtoSQL.taxi_fare(num INT, chuncksize INT)
wholeblock:BEGIN
SET @id = 1;
WHILE @id <= num DO
 select * from (Select Id, (SUM(dot_product) + 14.71098 ) as Score
 from ( select Id,name,value,l_w,feature, ( l_w * weight ) as dot_product  from(select Id,name,value,CASE 
WHEN name = 'VendorId' THEN 1
WHEN name = 'RateCode' THEN 1
WHEN name = 'PaymentType' THEN 1
WHEN name = 'PassengerCount' THEN value
WHEN name = 'TripTime' THEN value
WHEN name = 'TripDistance' THEN value

 END
 as l_w,CASE 
WHEN name = 'VendorId' THEN 0
WHEN name = 'RateCode' AND value = '1' THEN 1
WHEN name = 'RateCode' AND value = '2' THEN 2
WHEN name = 'PaymentType' AND value = 'CRD' THEN 3
WHEN name = 'PaymentType' AND value = 'CSH' THEN 4
WHEN name = 'PaymentType' AND value = 'NOC' THEN 5
WHEN name = 'PassengerCount' THEN 6
WHEN name = 'TripTime' THEN 7
WHEN name = 'TripDistance' THEN 8

 END
 as feature
 FROM (select id,'VendorId' as name ,VendorId as value 
 from  ( select Id,VendorId,RateCode,PaymentType,( ( PassengerCount - 0 ) * 0.6958339 ) as PassengerCount,( ( TripTime - 0 ) * 0.001289538 ) as TripTime,( ( TripDistance - 0 ) * 0.299262 ) as TripDistance  from  taxi_fare_with_score 
 where Id >= @id  and Id < ( @id + chuncksize ) 
 ) as F 

 UNION ALL 
select id,'RateCode' as name ,RateCode as value 
 from  ( select Id,VendorId,RateCode,PaymentType,( ( PassengerCount - 0 ) * 0.6958339 ) as PassengerCount,( ( TripTime - 0 ) * 0.001289538 ) as TripTime,( ( TripDistance - 0 ) * 0.299262 ) as TripDistance  from  taxi_fare_with_score 
 where Id >= @id  and Id < ( @id + chuncksize ) 
 ) as F 

 UNION ALL 
select id,'PaymentType' as name ,PaymentType as value 
 from  ( select Id,VendorId,RateCode,PaymentType,( ( PassengerCount - 0 ) * 0.6958339 ) as PassengerCount,( ( TripTime - 0 ) * 0.001289538 ) as TripTime,( ( TripDistance - 0 ) * 0.299262 ) as TripDistance  from  taxi_fare_with_score 
 where Id >= @id  and Id < ( @id + chuncksize ) 
 ) as F 

 UNION ALL 
select id,'PassengerCount' as name ,PassengerCount as value 
 from  ( select Id,VendorId,RateCode,PaymentType,( ( PassengerCount - 0 ) * 0.6958339 ) as PassengerCount,( ( TripTime - 0 ) * 0.001289538 ) as TripTime,( ( TripDistance - 0 ) * 0.299262 ) as TripDistance  from  taxi_fare_with_score 
 where Id >= @id  and Id < ( @id + chuncksize ) 
 ) as F 

 UNION ALL 
select id,'TripTime' as name ,TripTime as value 
 from  ( select Id,VendorId,RateCode,PaymentType,( ( PassengerCount - 0 ) * 0.6958339 ) as PassengerCount,( ( TripTime - 0 ) * 0.001289538 ) as TripTime,( ( TripDistance - 0 ) * 0.299262 ) as TripDistance  from  taxi_fare_with_score 
 where Id >= @id  and Id < ( @id + chuncksize ) 
 ) as F 

 UNION ALL 

select id,'TripDistance' as name ,TripDistance as value 
 from  ( select Id,VendorId,RateCode,PaymentType,( ( PassengerCount - 0 ) * 0.6958339 ) as PassengerCount,( ( TripTime - 0 ) * 0.001289538 ) as TripTime,( ( TripDistance - 0 ) * 0.299262 ) as TripDistance  from  taxi_fare_with_score 
 where Id >= @id  and Id < ( @id + chuncksize ) 
 ) as F 
) AS F) AS F 
 INNER JOIN weights_taxi
 ON (feature = label )) as F group by Id ) AS F;
SET @id = @id + chuncksize;
  END WHILE;
END//
