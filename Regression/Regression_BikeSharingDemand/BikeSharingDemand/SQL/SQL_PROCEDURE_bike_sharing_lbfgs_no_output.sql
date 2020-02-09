delimiter //
set names utf8;
drop procedure if exists MLtoSQL.bike_sharing_lbfgs_no_output;
CREATE procedure MLtoSQL.bike_sharing_lbfgs_no_output(num INT, chuncksize INT)
wholeblock:BEGIN
SET @id = 1;
WHILE @id <= num DO
 select count(1) from (select  EXP ( Season+Year+Month+Hour+Holiday+Weekday+WorkingDay+Weather+Temperature+NormalizedTemperature+Humidity+Windspeed+3.778778) as Score,Id from (select (Season * 0.03780045 )  as Season ,(Year * 0.4225986 )  as Year ,(Month * 0.02476057 )  as Month ,(Hour * 0.04666815 )  as Hour ,(Holiday * -0.1322837 )  as Holiday ,(Weekday * 0.005025696 )  as Weekday ,(WorkingDay * 0.008145074 )  as WorkingDay ,(Weather * -0.03184046 )  as Weather ,(Temperature * -0.4056959 )  as Temperature ,(NormalizedTemperature * 2.156224 )  as NormalizedTemperature ,(Humidity * -0.9345371 )  as Humidity ,(Windspeed * 0.2818703 )  as Windspeed ,Id from bike_sharing_Lbfgs) as F  ) AS F
 where Id >= @id  and Id < ( @id + chuncksize ); 

SET @id = @id + chuncksize;
  END WHILE;
END//
