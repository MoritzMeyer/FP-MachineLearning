import numpy as np
import pickle as pkl
import gzip

with gzip.open("C:\\Users\\Moritz\\Pictures\\unity\\gulasch.pkl.gz", 'rb') as handle:
    data = pkl.load(handle)

trainlBox = data["trainlBox"]
trainlLane = data["trainlLane"]
traind = data["traind"]

print("trainlBox.shape: ", trainlBox.shape)
print("trainlLane.shape: ", trainlLane.shape)
print("traind.shape: ", traind.shape)