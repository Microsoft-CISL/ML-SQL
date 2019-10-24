import pprint

def load_tokens_with_frequency(file_name, ml_net=False):
    all_tokens_freq = {}
    all_tokens = {}
    with open(file_name, "r") as file:
        for line in file:
            line = line.replace("\n", "")
            line_tokens = line.split("\t")
            row_number = int(line_tokens[0])
            token = line_tokens[1]
            if ml_net:
                token = token.replace("\\", "")
                token = token.replace('"""', '"')
            freq = line_tokens[2].replace(",", ".")
            freq = float(freq)
            all_tokens[token] = 1

            if row_number not in all_tokens_freq:
                all_tokens_freq[row_number] = {token: freq}
            else:
                all_tokens_freq[row_number][token] = freq
    print(len(all_tokens))

    return all_tokens_freq, all_tokens

print("SKLEARN")
all_tokens_freq_sklearn, all_tokens_sklearn = load_tokens_with_frequency("/home/matteo/Scrivania/NEW_token_with_frequency_sklearn.txt")
print("ML.NET")
all_tokens_freq_ml_net, all_tokens_ml_net = load_tokens_with_frequency("/home/matteo/Scrivania/NEW_token_with_frequency_ml_net.txt", ml_net=True)

shared_items = {k: 1 for k in all_tokens_sklearn if k in all_tokens_ml_net}
print("Common elements: {}".format(len(shared_items)))

print("Element only in sklearn")
p = 0
for item in all_tokens_sklearn:
    if item not in shared_items:
        print(item)
        p += 1
print(p)
print("\n"*20)
print("Element only in ml.net")
for item in all_tokens_ml_net:
    if item not in shared_items:
        print(item)
print("\n\n\n\n")

# for i in range(len(all_tokens_freq_sklearn)):
#     print("Row #{}".format(i))
#     sklearn_tokens = all_tokens_freq_sklearn[i]
#     ml_net_tokens = all_tokens_freq_ml_net[i]
#     top_distant_tokens = []
#
#     for key in sklearn_tokens:
#         if key in ml_net_tokens:
#             top_distant_tokens.append((abs(sklearn_tokens[key]-ml_net_tokens[key]), key, sklearn_tokens[key], ml_net_tokens[key]))
#
#     for top in sorted(top_distant_tokens, reverse=True, key=lambda x: x[0]):
#         print(top)
#     print("\n\n\n")


