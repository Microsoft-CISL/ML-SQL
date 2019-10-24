SELECT id, feature, count(*) * (1) as count FROM 
(SELECT
  sentiment_analysis_detection_with_score.Id, CONCAT("t.",REPLACE(REPLACE(REPLACE(lower(SUBSTRING(REPLACE(sentiment_analysis_detection_with_score.Comment,' ','␠'), numbers.n,3)),'␠','<␠>'),'␂','<␂>'),'␃','<␃>')) as feature
FROM
  numbers INNER JOIN sentiment_analysis_detection_with_score
  ON CHAR_LENGTH(sentiment_analysis_detection_with_score.Comment)-3 >= numbers.n-1
WHERE sentiment_analysis_detection_with_score.Id = 1
) 
AS F1 
group by feature)