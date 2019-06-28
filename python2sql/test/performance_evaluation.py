import os
import pandas as pd
import time
from collections import Counter


class EvaluatePredictionTimes(object):
    def __init__(self, data_file, data_size, data_features, class_attribute, chunk_sizes, output_methods, ml_pipeline, sep=',', header=True, transformation_target_attribute=None):
        self.data_file = data_file
        self.data_size = data_size
        self.data_features = data_features
        self.class_attribute = class_attribute
        self.chunk_sizes = chunk_sizes
        self.output_methods = output_methods
        self.ml_pipeline = ml_pipeline
        self.sep = sep
        self.header = header
        self.transformation_target_attribute = transformation_target_attribute

        self.output_data_header_level2 = []
        self.total_times = None
        self.avg_times = None

    def _evaluate_chunk_pipeline_times(self, chunk_index, chunk_size, output_method):
        times_chunk = {}

        # load chunk
        start_load_time = time.time()
        skip = chunk_index * chunk_size + 1
        if not self.header and chunk_index == 0:
            skip = chunk_index * chunk_size
        chunk = pd.read_csv(self.data_file, sep=self.sep, nrows=chunk_size, skiprows=skip, header=None,
                            names=self.data_features)
        if len(chunk) == 0:
            return {}

        end_load_time = time.time() - start_load_time
        times_chunk["loading data"] = end_load_time * 1000

        # execute prediction pipeline
        y_test = chunk[self.class_attribute]
        X_test = chunk.drop([self.class_attribute], axis=1)
        if self.transformation_target_attribute != None:
            self.ml_pipeline.execute_prediction_pipeline(X_test, y_test, output_method, transformation_target_attribute=self.transformation_target_attribute)
        else:
            self.ml_pipeline.execute_prediction_pipeline(X_test, y_test, output_method)

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

        if len(self.output_data_header_level2) == 0:
            self.output_data_header_level2.append("loading data")
            self.output_data_header_level2.extend(self.ml_pipeline.get_prediction_step_names())
            self.output_data_header_level2.append("total")
            self.output_data_header_level2.append("total / row")

        return times_chunk

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
            for output_method in self.output_methods:

                if self.data_size % chunk_size == 0:
                    num_chunks = int(self.data_size / chunk_size)
                else:
                    num_chunks = int(self.data_size / chunk_size) + 1

                # load one chunk per time and perform on it the entire prediction pipeline
                # save prediction pipeline times
                chunk_cumulative_pipeline_times = {}
                for i in range(num_chunks):
                    times_chunk = self._evaluate_chunk_pipeline_times(i, chunk_size, output_method)
                    if len(list(times_chunk.keys())) == 0:
                        break

                    chunk_cumulative_pipeline_times = dict(
                        Counter(chunk_cumulative_pipeline_times) + Counter(times_chunk))

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

    def save_performance_to_file(self, output_dir, prefix_output_file="output_data"):

        # save performance into final dataframe
        output_data_header_level1 = self.chunk_sizes[:]
        if output_data_header_level1[-1] == self.data_size:
            output_data_header_level1[-1] = "all"

        output_index = self.output_methods[:]
        output_index[0] = "None"

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
