 select * from (select Id,(EXP(class_0) / (EXP(class_0)+EXP(class_1)+EXP(class_2))) as class_0,(EXP(class_1) / (EXP(class_0)+EXP(class_1)+EXP(class_2))) as class_1,(EXP(class_2) / (EXP(class_0)+EXP(class_1)+EXP(class_2))) as class_2
 from  ( select Id,((SepalLength * -0.4289824 ) + (SepalWidth * 3.17538 ) + (PetalLength * -9.21505 ) + (PetalWidth * -4.477479 ) + 0.694539487361908) as class_0,((SepalLength * -3.005857 ) + (SepalWidth * -1.387949 ) + (PetalLength * -2.270517 ) + (PetalWidth * -4.785861 ) + 9.32265186309814) as class_1,((SepalLength * -5.791594 ) + (SepalWidth * -6.081567 ) + (PetalLength * 4.168737 ) + (PetalWidth * 6.767995 ) + -10.8540859222412) as class_2
 from iris
 where Id >= @id  and Id < ( @id + chuncksize ); 
 ) as F  ) AS F;
