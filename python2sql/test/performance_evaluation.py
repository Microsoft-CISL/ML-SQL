import os
import pandas as pd
import time
from collections import Counter
from python2sql.utils.db_connection import DatabaseConnectionManager
#import xgboost as xgb # pip install xgboost


class ResultDataFormat(object):
    def __init__(self, name, input_mode, output_mode, framework, batch_size, time_with_connection, time_without_connection, element_processed, execution_mode):
        self.name = name
        self.output_mode = output_mode
        self.input_mode = input_mode
        self.framework = framework
        self.batch_size = batch_size
        self.time_with_connection = time_with_connection
        self.time_without_connection = time_without_connection
        self.element_processed = element_processed
        self.execution_mode = execution_mode

    def get_result_data(self):
        result_data = {}
        result_data["name"] = self.name
        if self.output_mode == None:
            output_mode = "None"
        else:
            output_mode = self.output_mode
        result_data["inputMode"] = self.input_mode
        result_data["outputMode"] = output_mode
        result_data["dbms"] = self.framework
        result_data["batchSize"] = self.batch_size
        result_data["timeWithOutConnection"] = self.time_without_connection
        result_data["timeWithConnection"] = self.time_with_connection
        result_data["elementProcessed"] = self.element_processed
        result_data["executionMode"] = self.execution_mode

        return result_data


class EvaluatePredictionTimes(object):
    def __init__(self, name, data_file, data_size, data_features, class_attribute, chunk_sizes, input_output_methods, ml_pipeline, sep=',', header=True, transformation_target_attribute=None, ml_task='classification', custom_dataset_format=False):
        self.name = name
        self.data_file = data_file
        self.data_size = data_size
        self.data_features = data_features
        self.class_attribute = class_attribute
        self.chunk_sizes = chunk_sizes
        self.input_output_methods = input_output_methods
        self.ml_pipeline = ml_pipeline
        self.sep = sep
        self.header = header
        self.transformation_target_attribute = transformation_target_attribute
        self.custom_dataset_format = custom_dataset_format
        self.framework = "SKLEARN"
        self.execution_mode = "BATCH_MODE"

        self.output_data_header_level2 = []
        self.total_times = None
        self.avg_times = None
        self.ml_task = ml_task

        self.results = []

        db_manager = DatabaseConnectionManager()
        mysql_connection_start_time = time.time()
        self.mysql_db_string_connection = db_manager.get_string_db_connection("mysql", "new_ml_sql_test", charset="utf8mb4")
        mysql_connection = db_manager.create_db_connection("mysql", "new_ml_sql_test", charset="utf8mb4")
        mysql_connection_time = (time.time() - mysql_connection_start_time) * 1000

        #mysql_connection.execute('SET NAMES utf8mb4;')
        #mysql_connection.execute('SET CHARACTER SET utf8mb4;')
        #mysql_connection.execute('SET character_set_connection=utf8mb4;')

        sqlserver_connection_start_time = time.time()
        sqlserver_connection = db_manager.create_db_connection("sqlserver", "new_ml_sql_test", charset="utf8mb4")
        sqlserver_connection_time = (time.time() - sqlserver_connection_start_time) * 1000

        self.db_connection_times = {"mysql": mysql_connection_time, "sqlserver": sqlserver_connection_time}
        self.db_connections = {"mysql": mysql_connection, "sqlserver": sqlserver_connection}

        self.complete_input_output_methods = []
        for input_output_method in input_output_methods:
            in_m = input_output_method["input"]
            in_f = in_m = input_output_method["input_format"]
            out_m = in_m = input_output_method["output"]
            out_f = in_m = input_output_method["output_format"]

            if in_f and out_f:
                complete_m = "in {} ({}) out {} ({})".format(in_m, in_f, out_m, out_f)
            else:
                if in_f:
                    complete_m = "in {} ({}) out {}".format(in_m, in_f, out_m)
                elif out_f:
                    complete_m = "in {} out {} ({})".format(in_m, out_m, out_f)
                else:
                    complete_m = "in {} out {}".format(in_m, out_m)
            self.complete_input_output_methods.append(complete_m)

        #self._init_sql_tables()

    def _init_sql_tables(self):
        pass
        # if self.header:
        #     chunk = pd.read_csv(self.data_file, sep=self.sep, nrows=1)
        # else:
        #     chunk = pd.read_csv(self.data_file, sep=self.sep, nrows=1, header=None,
        #                     names=self.data_features)
        # chunk.to_sql('chunk', con=self.connection, if_exists="replace", index=False)
        # self.connection.execute("delete from chunk;")

        #chunk_prediction = pd.DataFrame(data={'prediction': [0]})
        #chunk_prediction.to_sql('chunk_predictions', con=self.connection, if_exists="replace", index=False)
        #self.connection.execute("delete from chunk_predictions;")

    def _evaluate_chunk_pipeline_times(self, chunk_index, chunk_size, input_output_method):
        times_chunk = {}

        input_method = input_output_method["input"]
        input_format = input_output_method["input_format"]
        if input_format:
            complete_input_method = input_format
        #else:
        #    complete_input_method = input_method

        output_method = input_output_method["output"]
        output_format = input_output_method["output_format"]
        if output_format:
            complete_output_method = output_format
        #else:
        #    complete_output_method = output_method

        input_db_connection = None
        if input_method == 'db':
            try:
                input_db_connection = self.db_connections[input_format.lower()]
            except KeyError:
                raise ValueError("Input database format not valid.")

        output_db_connection = None
        if output_format and output_method == 'db':
            try:
                output_db_connection = self.db_connections[output_format.lower()]
            except KeyError:
                raise ValueError("Output database format not valid.")

        if chunk_index == 0:
            if output_db_connection:
                if output_db_connection.dialect.has_table(output_db_connection, "chunk_predictions"):
                    output_db_connection.execute("drop table chunk_predictions;")

        # load chunk ------------------------------------------------------------------------------
        start_load_time = time.time()

        skip = chunk_index * chunk_size + 1
        if not self.header and chunk_index == 0:
            skip = chunk_index * chunk_size
        chunk = pd.read_csv(self.data_file, sep=self.sep, nrows=chunk_size, skiprows=skip, header=None,
                            names=self.data_features)
        if len(chunk) == 0:
            return {}, None

        if input_method == 'db':
            chunk.to_sql('chunk', con=self.db_connections[input_format.lower()], if_exists="replace", index=False)

            start_load_time = time.time()
            chunk = pd.read_sql_table('chunk', self.mysql_db_string_connection)

        end_load_time = time.time() - start_load_time
        times_chunk["loading data"] = end_load_time * 1000
        # -----------------------------------------------------------------------------------------

        # execute prediction pipeline
        y_test = chunk[self.class_attribute]
        X_test = chunk.drop([self.class_attribute], axis=1)

        #if self.custom_dataset_format:
        #    X_test = xgb.DMatrix(X_test, label=y_test)


        if self.transformation_target_attribute != None:
            self.ml_pipeline.execute_prediction_pipeline(X_test, y_test, output_method, transformation_target_attribute=self.transformation_target_attribute, ml_task_type=self.ml_task, evaluate_model=False, db_connection=output_db_connection)
        else:
            self.ml_pipeline.execute_prediction_pipeline(X_test, y_test, output_method, ml_task_type=self.ml_task, evaluate_model=False, db_connection=output_db_connection)

        # get prediction pipeline times
        times = self.ml_pipeline.get_prediction_pipeline_times()
        times_chunk.update(times)

        # compute total time
        total_time = sum(list(times_chunk.values()))
        times_chunk["total"] = total_time

        # compute total time / row
        total_time = times_chunk["total"]
        total_time_row = total_time / chunk_size
        times_chunk["total / row"] = total_time_row


        if output_method == 'db' or input_method == 'db':
            connection_time = total_time
        else:
            connection_time = 0
        #     if output_method == 'db':
        #         connection_time = total_time + self.db_connection_times[output_format.lower()]
        #     else:
        #         connection_time = total_time + self.db_connection_times[input_format.lower()]
        # else:
        #     connection_time = 0

        if chunk_size == self.data_size:
            chunk_dim = "ALL"
        else:
            chunk_dim = chunk_size

        result = ResultDataFormat(self.name, complete_input_method, complete_output_method, self.framework, chunk_dim, connection_time, total_time, self.data_size, self.execution_mode)
        #self.results.append(result)

        if len(self.output_data_header_level2) == 0:
            self.output_data_header_level2.append("loading data")
            self.output_data_header_level2.extend(self.ml_pipeline.get_prediction_step_names())
            self.output_data_header_level2.append("total")
            self.output_data_header_level2.append("total / row")

        return times_chunk, result

    def evaluate_pipeline_times(self):

        # measure pipeline prediction times in different scenarios
        # the scenarios are defined with respect:
        # 1) the chunk size
        # 2) the prediction output method

        # loop over chunk sizes
        total_times_per_chunk = []
        avg_times_per_chunk = []
        for chunk_size in self.chunk_sizes:

            # loop over output methods
            total_times_per_output = []
            avg_times_per_output = []
            for input_output_method in self.input_output_methods:

                if self.data_size % chunk_size == 0:
                    num_chunks = int(self.data_size / chunk_size)
                else:
                    num_chunks = int(self.data_size / chunk_size) + 1

                # load one chunk per time and perform on it the entire prediction pipeline
                # save prediction pipeline times
                chunk_cumulative_pipeline_times = {}

                chunk_results = []
                for i in range(num_chunks):
                    times_chunk, result = self._evaluate_chunk_pipeline_times(i, chunk_size, input_output_method)
                    if len(list(times_chunk.keys())) == 0:
                        break

                    chunk_cumulative_pipeline_times = dict(
                        Counter(chunk_cumulative_pipeline_times) + Counter(times_chunk))
                    chunk_results.append(result)

                definitive_chunk_res = chunk_results[0].get_result_data()
                for key in definitive_chunk_res.keys():
                    if key in ["timeWithOutConnection", "timeWithConnection"]:
                        definitive_chunk_res[key] = 0
                first = True
                for chunk_res in chunk_results:
                    chunk_res_dict = chunk_res.get_result_data()
                    if first:

                        if chunk_res_dict["inputMode"] in ['MYSQL', 'SQLSERVER'] or chunk_res_dict["outputMode"] in ['MYSQL', 'SQLSERVER']:
                            if chunk_res_dict["outputMode"] in ['MYSQL', 'SQLSERVER']:
                                chunk_res_dict["timeWithConnection"] = chunk_res_dict["timeWithConnection"] + self.db_connection_times[chunk_res_dict["outputMode"].lower()]
                            if chunk_res_dict["inputMode"] in ['MYSQL', 'SQLSERVER']:
                                chunk_res_dict["timeWithConnection"] = chunk_res_dict["timeWithConnection"] + \
                                                                       self.db_connection_times[
                                                                           chunk_res_dict["inputMode"].lower()]

                        first = False

                    #definitive_chunk_res["elementProcessed"] += chunk_res_dict["elementProcessed"]
                    definitive_chunk_res["timeWithOutConnection"] += chunk_res_dict["timeWithOutConnection"]
                    definitive_chunk_res["timeWithConnection"] += chunk_res_dict["timeWithConnection"]
                self.results.append(definitive_chunk_res)

                # compute average times from cumulative times
                total_chunk_data_times = []
                avg_chunk_data_times = []
                for key in self.output_data_header_level2:
                    val = chunk_cumulative_pipeline_times[key]
                    total_chunk_data_times.append(val)
                    val /= num_chunks
                    avg_chunk_data_times.append(val)
                total_times_per_output.append(total_chunk_data_times)
                avg_times_per_output.append(avg_chunk_data_times)

            total_times_per_chunk.append(total_times_per_output)
            avg_times_per_chunk.append(avg_times_per_output)

        self.total_times = total_times_per_chunk
        self.avg_times = avg_times_per_chunk

        return self.total_times, self.avg_times

    def _format_output_data(self, data, index, header1, header2):
        dfs = []
        for i in range(len(data)):
            df = pd.DataFrame(data[i], index=index,
                              columns=["{}_{}".format(col, i) for col in header2])
            dfs.append(df)

        # concatenate all the dataframe referring to different chunk sizes
        if len(dfs) == 1:
            df = dfs[0]
            output_data = pd.DataFrame(df.values, index=index, columns=pd.MultiIndex.from_product(
                [header1, header2]))
        else:
            concat_dfs = pd.concat(dfs, axis=1)
            output_data = pd.DataFrame(concat_dfs.values, index=index, columns=pd.MultiIndex.from_product(
                [header1, header2]))

        return output_data

    def save_detailed_performance_to_file(self, output_dir, prefix_output_file="output_data"):

        # save performance into final dataframe
        output_data_header_level1 = self.chunk_sizes[:]
        if output_data_header_level1[-1] == self.data_size:
            output_data_header_level1[-1] = "all"

        #output_index = self.output_methods[:]
        #output_index[0] = "None"
        output_index = self.complete_input_output_methods[:]

        # transform the data grouped by chunk size into a dataframe
        total_output_data = self._format_output_data(self.total_times, output_index, output_data_header_level1,
                                                     self.output_data_header_level2)
        #total_output_data.to_csv(os.path.join(output_dir, "{}_{}.csv".format(prefix_output_file, "total_partial")))
        for chunk in self.chunk_sizes:
            if chunk == self.data_size:
                chunk = "all"
            chunk_total = total_output_data[chunk]["total"]
            chunk_total_per_row = chunk_total / self.data_size
            total_output_data[chunk]["total / row"] = chunk_total_per_row
        avg_output_data = self._format_output_data(self.avg_times, output_index, output_data_header_level1,
                                                     self.output_data_header_level2)

        total_output_data.to_csv(os.path.join(output_dir, "{}_{}.csv".format(prefix_output_file, "total")))
        avg_output_data.to_csv(os.path.join(output_dir, "{}_{}.csv".format(prefix_output_file, "avg")))

    def save_performance_to_file(self, output_dir, prefix_output_file="times"):

        # results_df = pd.DataFrame([r.get_result_data() for r in self.results])
        results_df = pd.DataFrame(self.results)

        results_df.to_csv(os.path.join(output_dir, "{}_{}.csv".format(self.name, prefix_output_file)), index=False)
