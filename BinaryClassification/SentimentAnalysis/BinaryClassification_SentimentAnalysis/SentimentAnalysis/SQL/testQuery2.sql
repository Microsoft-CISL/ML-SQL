SET @id =100;
SET @bias =-2.420864;
SELECT id, (SUM( F1.count * weights_sentiment_analysis.weight) +@bias) AS weight FROM
(


SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from 	
(SELECT id, feature, count(*) * (1) as count FROM 
(SELECT
  sentiment_analysis_detection_with_score.Id, CONCAT("t.",REPLACE(REPLACE(REPLACE(lower(SUBSTRING(REPLACE(sentiment_analysis_detection_with_score.Comment,' ','␠'), numbers.n,3)),'␠','<␠>'),'␂','<␂>'),'␃','<␃>')) as feature
FROM
  numbers INNER JOIN sentiment_analysis_detection_with_score
  ON CHAR_LENGTH(sentiment_analysis_detection_with_score.Comment)-3 >= numbers.n-1
WHERE sentiment_analysis_detection_with_score.Id <= @id
) 
AS F1 
group by id,feature) AS F1

LEFT JOIN 


(SELECT id,SQRT(SUM(count)) as ww FROM
(SELECT id,feature, POW(count(*),2) as count FROM 
(SELECT
  sentiment_analysis_detection_with_score.Id, 
  CONCAT("t.",REPLACE(REPLACE(lower(SUBSTRING(REPLACE(sentiment_analysis_detection_with_score.Comment,' ','␠'), numbers.n,3)),'␠','<␠>'),'␂','<␂>')) as feature
FROM
  numbers INNER JOIN sentiment_analysis_detection_with_score
  ON CHAR_LENGTH(sentiment_analysis_detection_with_score.Comment)- 3 >= numbers.n-1
WHERE sentiment_analysis_detection_with_score.Id <= @id
) 
AS F1
group by id,feature) 
AS F2
group by id) AS F2 
ON (F2.id = F1.id)

UNION ALL

SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from 	
(SELECT id, feature, count(*) * (1) as count FROM 
(SELECT
  sentiment_analysis_detection_with_score.Id,
  CONCAT("w.",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( sentiment_analysis_detection_with_score.Comment ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', numbers.n), ' ', -1) ) as feature
FROM
  numbers INNER JOIN sentiment_analysis_detection_with_score
  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( sentiment_analysis_detection_with_score.Comment ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )))
     -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( sentiment_analysis_detection_with_score.Comment ,'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= numbers.n-1
WHERE sentiment_analysis_detection_with_score.Id <= @id
)
AS F1
group by id,feature) AS F1

LEFT JOIN 

(SELECT id, SQRT(SUM(count)) as ww FROM
(SELECT id,feature, POW(count(*),2) as count FROM 
(SELECT
  sentiment_analysis_detection_with_score.Id,
  CONCAT("w.",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( sentiment_analysis_detection_with_score.Comment ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', numbers.n), ' ', -1) ) as feature
FROM
  numbers INNER JOIN sentiment_analysis_detection_with_score
  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( sentiment_analysis_detection_with_score.Comment ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )))
     -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( sentiment_analysis_detection_with_score.Comment ,'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= numbers.n-1
WHERE sentiment_analysis_detection_with_score.Id <= @id
)
AS F1
group by id,feature )
AS F2
group by id) AS F2
ON (F2.id = F1.id) 

#where F1.id < 1
)
as F1 INNER JOIN weights_sentiment_analysis ON (weights_sentiment_analysis.label = F1.feature) group by id;