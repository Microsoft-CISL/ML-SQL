import pandas as pd
# from sklearn.preprocessing import LabelEncoder
from sklearn.preprocessing import OrdinalEncoder
from sklearn.pipeline import Pipeline
from sklearn.compose import ColumnTransformer
from sklearn.impute import SimpleImputer
import seaborn as sns
from sklearn.preprocessing import StandardScaler
import matplotlib.pyplot as plt
from sklearn.model_selection import train_test_split
from sklearn.ensemble import GradientBoostingRegressor
import time
from sklearn.metrics import *
from sklearn.metrics.classification import unique_labels
from math import sqrt
import pandas as pd
from sklearn.preprocessing import MinMaxScaler
from sklearn.preprocessing import StandardScaler
import numpy as np

df = pd.read_csv("flight_data.csv")
df = df.iloc[0:1000000]
print(df.shape)

# [BEGIN] MISSING VALUES -------------------------------------------------

# rows with at least one missing value
rows_with_na = df[df.isna().any(axis=1)]
print("Num. rows with missing values: {}".format(len(rows_with_na)))

# missing value distribution in columns
print(df.isna().sum())
mask_col_with_null = df.isna().sum() > 0
cols_with_null = df.columns[mask_col_with_null].tolist()
print("Columns with null values: {}".format(cols_with_null))

# columns with null values analysis in order to understand which replace strategy to be adopted
numerical_col_with_null = []
categorical_col_with_null = []
for col in cols_with_null:
    print(col)
    print(df[col].dtypes)
    print(df[col].unique()[:2])
    print()
    if not (df[col].dtypes == object):
        numerical_col_with_null.append(col)
    else:
        categorical_col_with_null.append(col)

# removing 'CancellationCode' column which has a large number of missing values
print("Removing 'CancellationCode' column which has a large number of missing values")
df = df.drop(['CancellationCode'], axis=1)
categorical_col_with_null = list(set(categorical_col_with_null) - set(['CancellationCode']))

# replace null values with mean for numerical feature
print("Replacing null values with means for numerical feature {}".format(numerical_col_with_null))
df[numerical_col_with_null] = df[numerical_col_with_null].fillna(df[numerical_col_with_null].mean())

# replace null values with the most frequent value for categorical feature
print("Replacing null values with the most frequent values for categorical features {}".format(
    categorical_col_with_null))
# imp = SimpleImputer(strategy="most_frequent")
# df[categorical_col_with_null] = imp.fit_transform(df[categorical_col_with_null])
df[categorical_col_with_null] = df[categorical_col_with_null].fillna(df[categorical_col_with_null].mode().iloc[0])

# rows with at least one missing value
rows_with_na = df[df.isna().any(axis=1)]
print("Num. rows with missing values: {}".format(len(rows_with_na)))

df["Id"] = range(df.shape[0])

#df.to_csv("flight_complete_data.csv", index=False)

# [END] MISSING VALUES ---------------------------------------------------

attribute_to_predict = "LateAircraftDelay"
y = df[attribute_to_predict]
X = df.drop([attribute_to_predict], axis=1)
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.3, random_state=24)
print(X_train.shape)
print(y_train.shape)
print(X_test.shape)
print(y_test.shape)
# print(X_train.dtypes)
# print(df.dtypes)

#print("Starting saving training data...")
#train = X_train.copy()
#train[attribute_to_predict] = y_train
#train.to_csv("flight_delay_train_data.csv", index=False)

#print("Starting saving testing data...")
#test = X_test.copy()
#test[attribute_to_predict] = y_test
#test.to_csv("flight_delay_test_data.csv", index=False)

# Categorical boolean mask
print("Starting applying categorical encoder...")
categorical_feature_mask = X_train.dtypes == object

# filter categorical columns using mask and turn it into a list
categorical_cols = X_train.columns[categorical_feature_mask].tolist()
print(categorical_cols)