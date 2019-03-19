import pickle as pkl
import numpy as np
from PIL import Image
import inspect, os, sys
import gzip

'Wenn ein Pfad als Argument übergeben würde, nehme diesen.'
'Sonst den Pfad des Ordners in dem das Skript liegt'
if len(sys.argv) == 1:
    path = os.path.dirname(os.path.abspath(inspect.getfile(inspect.currentframe())))
else:
    path = sys.argv[1]

'Der übergebene Pfad muss ein Ordner sein'
if not os.path.isdir(path):
    raise Exception("The given path has to be a directory!")

'Diese Funktion liest aus einem Ordner alle Dateien aus und wandelt jedes Bild' \
'Die dazugehörige Zeile in einen Datenvektor um'
def readFolder(path, trainlBox, trainlLane, traind):
    carCoords = []
    with open(os.path.join(path, "carCoords.txt")) as f:
        carCoords.extend(f.readlines())
        carCoords = [x.strip() for x in carCoords]

    pictures = []
    for root, dirs, files in os.walk(path, topdown=False):
        pictures.extend(files)

    pictures = [name for name in pictures if '.jpg' in name]

    for fileName in pictures:
        'load Image into numpy array'
        img = Image.open(os.path.join(path, fileName))

        'hole den Index des Bildes um die entsprechenden Daten aus carCoords.txt zu holen'
        fileNameParts = fileName.split('.')
        nameParts = fileNameParts[0].split('_')
        index = int(nameParts[2])

        'Wenn zu dem Bild keine Informationen getrackt wurden abbrechen'
        if not index < len(carCoords):
            continue

        traind.append(np.asarray(img))
        actualPicIndex = len(traind) - 1
        traind[actualPicIndex] = traind[actualPicIndex][:, :, 0:1]

        'Die line mit carCoords zum Bild'
        coordData = carCoords[index]

        'Die einzelnen Daten der Bounding Boxes extrahieren'
        pictureData = coordData.split(';')

        'zunächst wird nur eine Bounding Box erkannt (die erste)'
        pictureValues = pictureData[0].split(',')
        boxValues = np.empty([8])
        laneValues = np.zeros([8])

        if len(pictureValues) > 1:
            'Zunächst die Lane extrahieren'
            laneValues[int(pictureValues[0])] = 1

            'Danach die Daten der BoundingBox holen'
            for i in range(1, 9):
                bbCoordValue = float(pictureValues[i])
                boxValues[i-1] = bbCoordValue
        trainlLane.append(np.asarray(laneValues))
        trainlBox.append(np.asarray(boxValues))



print("Folders to be converted:")
trainlBox = []
trainlLane = []
traind = []
for root, dirs, files in os.walk(path, topdown=False):
    for name in dirs:
        print(os.path.join(root, name))
        readFolder(os.path.join(root, name), trainlBox, trainlLane, traind)

filename = 'gulasch.pkl.gz'
traind = np.asarray(traind)
trainlBox = np.asarray(trainlBox)
trainlLane = np.asarray(trainlLane)
dataObj = {"trainlBox": trainlBox, "trainlLane": trainlLane, "traind":traind}
with gzip.open(os.path.join(path, filename), 'wb') as handle:
    pkl.dump(dataObj, handle, protocol=pkl.HIGHEST_PROTOCOL)