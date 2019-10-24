delimiter //
set names utf8;
drop procedure if exists MLtoSQL.bike_sharing_sdca;
CREATE procedure MLtoSQL.bike_sharing_sdca(num INT, chuncksize INT)
wholeblock:BEGIN
SET @id = 1;
WHILE @id <= num DO
 select * from (select  ( Season+Year+Month+Hour+Holiday+Weekday+WorkingDay+Weather+Temperature+NormalizedTemperature+Humidity+Windspeed+-37.69627) as Score,Id from (select (Season * 12.08165 )  as Season ,(Year * 55.666 )  as Year ,(Month * 0.6893473 )  as Month ,(Hour * 9.060568 )  as Hour ,(Holiday * -22.39898 )  as Holiday ,(Weekday * 3.908327 )  as Weekday ,(WorkingDay * 1.788557 )  as WorkingDay ,(Weather * -4.239179 )  as Weather ,(Temperature * 85.047 )  as Temperature ,(NormalizedTemperature * 129.354 )  as NormalizedTemperature ,(Humidity * -89.03579 )  as Humidity ,(Windspeed * 9.685305 )  as Windspeed ,Id from bike_sharing_Sdca) as F  ) AS F
 where Id >= @id  and Id < ( @id + chuncksize ); 

SET @id = @id + chuncksize;
  END WHILE;
END//
