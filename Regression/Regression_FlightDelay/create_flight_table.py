import pandas as pd
import numpy as np

df = pd.read_csv("FlightDelay/Data/flight_complete_data_very_small.csv")
print(df.head())
print(df.shape)
print(df.columns)

old_id_column = df["Id"]
df = df.drop(['Id'], axis=1)

label = 'LateAircraftDelay'
df["Label"] = df[label]
df = df.drop(['LateAircraftDelay'], axis=1)

df["Id"] = old_id_column.astype(np.int32)

df = df.drop(["TailNum"], axis=1)
df["Score"] = 0

df = df.iloc[:1]
print(df.head())
print(df.shape)
print(df.columns)

from sqlalchemy import create_engine

db_string = '{}://{}:{}@{}/{}'.format("mysql+pymysql", "root", "ndulsp+92+pgnll", "localhost", "MLtoSQL")
engine = create_engine(db_string, echo=False)
df.to_sql('flight_delay_FastTree', con=engine, if_exists='replace', index=False)
engine.execute("delete from flight_delay_FastTree;")

score_table = pd.DataFrame([[0,123456.123456789]], columns=["Id", "Score"])
score_table.to_sql('flight_delay_with_output', con=engine, if_exists='replace', index=False)
engine.execute("delete from flight_delay_with_output;")
#engine.execute("CREATE TABLE  flight_delay_with_output ( Id int, Score float);");

file = open("FlightDelay/SQL/02_MYSQL_INSERT_Flight_Delay_FastTree.sql")
engine.execute(file.read())