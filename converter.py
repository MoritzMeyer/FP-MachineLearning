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
def readFolder(path, dataVec):
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
        dataVec[0].append(np.asarray(img))

        actualPicIndex = len(dataVec[0]) - 1
        dataVec[0][actualPicIndex] = dataVec[0][actualPicIndex][:,:,0:1]

        'hole den Index des Bildes um die entsprechenden Daten aus carCoords.txt zu holen'
        fileNameParts = fileName.split('.')
        nameParts = fileNameParts[0].split('_')
        index = int(nameParts[2])

        'Wenn zu dem Bild keine Informationen getrackt wurden abbrechen'
        if not index < len(carCoords):
            break

        'Die line mit carCoords zum Bild'
        coordData = carCoords[index]

        'Die einzelnen Daten der Bounding Boxes extrahieren'
        boxesData = coordData.split(';')

        'zunächst wird nur eine Bounding Box erkannt (die erste)'
        boxValues = boxesData[0].split(',')
        values = []
        if len(boxValues) > 1:
            for value in boxValues:
                values.append(float(value))
        dataVec[1].append(values)

    test = "test"

print("Folders to be converted:")
dataVec = [[], []]
for root, dirs, files in os.walk(path, topdown=False):
    for name in dirs:
        print(os.path.join(root, name))
        readFolder(os.path.join(root, name), dataVec)

filename = 'gulasch.pkl.gz'
with gzip.open(os.path.join(path, filename), 'wb') as handle:
    pkl.dump(dataVec, handle, protocol=pkl.HIGHEST_PROTOCOL)