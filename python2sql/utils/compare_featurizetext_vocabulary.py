import pprint
import pandas as pd
import pickle
import unidecode
import unicodedata

vocabulary = {}
with open("/home/matteo/Scrivania/tokens.txt", "r") as file:
    line_index = 0
    for line in file:
        if line.startswith("Char"):
            token = line.replace("Char.", "")
            #token = '<␠>|{||'
            #token = '<␠>|||<␠>'
            #token = '||<␠>|s'
            #token = '||5|0'
            #token = '||||5'
            token = token.split("|")
            #print(token)
            new_token = []
            if len(token) > 3:
                found = False
                for t in token:
                    if t != '':
                        new_token.append(t)
                    else:
                        if not found:
                            new_token.append('|')
                            found = True
                        else:
                            found = False
            else:
                new_token = token
            token = ''.join(new_token)
            #print(token)
            #exit(1)
        elif line.startswith("Word"):
            token = line.replace("Word.", "")

        token = token.replace("\n", "")
        if token not in vocabulary:
            vocabulary[token] = line_index
            line_index += 1
print(len(vocabulary))

#sklearn_vocabulary = {}
#data = pd.read_csv("/home/matteo/Scrivania/vocabulary_sklearn.csv", sep="\t", header=None, names=["token", "id"])
#for row_index,row in data.iterrows():
#    token = row["token"].replace("\\", "")
#    num = row["id"]
#    sklearn_vocabulary[token] = num
with open('/home/matteo/Scrivania/vocabulary_sklearn.pickle', 'rb') as handle:
    voc = pickle.load(handle)
sklearn_vocabulary = {}
for key in voc:
    #new_key = key.replace("\\", "")
    #new_key = unidecode.unidecode(new_key)
    #new_key = unicodedata.normalize('NFKD', new_key)
    #new_key = new_key.encode('ascii', 'ignore')
    #new_key = unidecode.unidecode(new_key.decode('utf-8'))
    sklearn_vocabulary[key] = voc[key]

print(len(sklearn_vocabulary))
#pprint.pprint(sklearn_vocabulary)

#shared_items = {k: vocabulary[k] for k in vocabulary if k in sklearn_vocabulary and vocabulary[k] == sklearn_vocabulary[k]}
shared_items = {k: 1 for k in vocabulary if k in sklearn_vocabulary}
print("Common elements: {}".format(len(shared_items)))

print("Element only in sklearn")
p = 0
for item in sklearn_vocabulary:
    if item not in shared_items:
        print(item)
        p += 1
print(p)
print("\n"*20)
print("Element only in ml.net")
c = 0
t = 0
for item in vocabulary:
    if item not in shared_items:
        t += 1
        print(item)
        #if u'<␃>' not in item:
        #    c += 1
        #if item.startswith("maudoodi"):
        #    print(item)
print("{}/{}".format(c, t))
