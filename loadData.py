import numpy as np
import pickle as pkl
import gzip
import os

path = "C:\\Users\\Moritz\\Pictures\\unity\\gulasch8LanesWithCargulasch.pkl.gz"

if os.path.getsize(path) > 0:
    with gzip.open(path, 'rb') as handle:
        data = pkl.load(handle)

trainlBox = data["trainlBox"]
trainlLane = data["trainlLane"]
traind = data["traind"]
trainlPictureBB_full = data["trainlPictureBB_full"]
trainlPictureBB_canvas = data["trainlPictureBB_full"]
trainlPictureBB_corners = data["trainlPictureBB_corners"]

print("trainlBox.shape: ", trainlBox.shape)
print("trainlLane.shape: ", trainlLane.shape)
print("traind.shape: ", traind.shape)
print("trainlPictureBB_full.shape: ", trainlPictureBB_full.shape)
print("trainlPictureBB_canvas.shape: ", trainlPictureBB_canvas.shape)
print("trainlPictureBB_corners.shape: ", trainlPictureBB_corners.shape)