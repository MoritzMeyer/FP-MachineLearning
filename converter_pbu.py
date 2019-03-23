import pickle as pkl
import numpy as np
from PIL import Image
import inspect, os, sys
import gzip
import time
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
def readFolder(path, trainlBox, trainlLane, traind, trainlPictureBB_full, trainlPictureBB_canvas, trainlPictureBB_corners):
    carCoords = []
    with open(os.path.join(path, "carCoords.txt")) as f:
        carCoords.extend(f.readlines())
        carCoords = [x.strip() for x in carCoords]

    pictures = []
    for root, dirs, files in os.walk(path, topdown=False):
        pictures.extend(files)

    pictures = [name for name in pictures if 'screen_120x100_00133.jpg' in name]

    for fileName in pictures:
        'load Image into numpy array'
        img = Image.open(os.path.join(path, fileName))
        print(fileName)
        'hole den Index des Bildes um die entsprechenden Daten aus carCoords.txt zu holen'
        fileNameParts = fileName.split('.')
        nameParts = fileNameParts[0].split('_')
        index = int(nameParts[2])

        'Wenn zu dem Bild keine Informationen getrackt wurden abbrechen'
        if not index < len(carCoords):
            continue

        traind.append(np.asarray(img))
        actualPicIndex = len(traind) - 1
        #traind[actualPicIndex] = traind[actualPicIndex][:, :, 0:1]
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

def calcBBPicture(traind, trainlBox, width, height, trainlPictureBB_full, trainlPictureBB_canvas, trainlPictureBB_corners):
    for i in range(len(traind)):
        picutreBB_full = np.pad(traind[i], [(0, 0), (0, traind[i].shape[1]), (0, 0)], mode="constant")
        picutreBB_canvas = np.pad(traind[i], [(0, 0), (0, traind[i].shape[1]), (0, 0)], mode="constant")
        picutreBB_corners = np.pad(traind[i], [(0, 0), (0, traind[i].shape[1]), (0, 0)], mode="constant")
        print (picutreBB_full.shape)
        x1 = int(round(trainlBox[i][0] * width))
        x2 = int(round(trainlBox[i][4] * width))
        y1 = int(round(trainlBox[i][1] * height))
        y2 = int(round(trainlBox[i][3] * height))

        'picture_full'
        for x in range(x1-1, x2):
            for y in range(y1-1, y2):
                picutreBB_full[y][x + width][0:2] = 255

        'picture_canvas'
        for x in range(x1-1, x2):
            picutreBB_canvas[y1-1][x + width][0:2] = 255
            picutreBB_canvas[y2-1][x + width][0:2] = 255

        for y in range(y1-1, y2):
            picutreBB_canvas[y][x1-1 + width][0:2] = 255
            picutreBB_canvas[y][x2-1 + width][0:2] = 255

        'picture_corners'
        picutreBB_corners[y1-1][x1-1 + width][0:2] = 255
        picutreBB_corners[y1-1][x2-1 + width][0:2] = 255
        picutreBB_corners[y2-1][x1-1 + width][0:2] = 255
        picutreBB_corners[y2-1][x2-1 + width][0:2] = 255


        imgs = []
        imgs.append(Image.fromarray(picutreBB_canvas,'RGB')) # Create a PIL image

        imgs.append(Image.fromarray(picutreBB_corners,'RGB')) # Create a PIL image

        imgs.append(Image.fromarray(picutreBB_full,'RGB')) # Create a PIL image
        outImg = Image.new('RGB', (imgs[0].size[0], imgs[0].size[1] * 3))
        y = 0
        print ('here')
        for image in imgs:
            outImg.paste(image,(0,y))
            y+=image.size[1]

        outImg.show()
        outImg.save('out_compare.png', format='png')
        exit(0)
        time.sleep(30)



        trainlPictureBB_full.append(np.asarray(picutreBB_full))

print("Folders to be converted:")
trainlBox = []
trainlLane = []
traind = []
trainlPictureBB_full = []
trainlPictureBB_canvas = []
trainlPictureBB_corners = []
for root, dirs, files in os.walk(path, topdown=False):
    print (dirs, path)
    for name in dirs:
        print(os.path.join(root, name))
        readFolder(os.path.join(root, name), trainlBox, trainlLane, traind, trainlPictureBB_full, trainlPictureBB_canvas, trainlPictureBB_corners)

calcBBPicture(traind, trainlBox, 100, 120, trainlPictureBB_full, trainlPictureBB_canvas, trainlPictureBB_corners)

filename = 'gulasch.pkl.gz'
traind = np.asarray(traind)
trainlBox = np.asarray(trainlBox)
trainlLane = np.asarray(trainlLane)
trainlPictureBB_full = np.asarray(trainlPictureBB_full)
trainlPictureBB_canvas = np.asarray(trainlPictureBB_canvas)
trainlPictureBB_corners = np.asarray(trainlPictureBB_corners)
dataObj = {"trainlBox": trainlBox, "trainlLane": trainlLane, "traind": traind, "trainlPictureBB_full": trainlPictureBB_full, "trainlPictureBB_canvas": trainlPictureBB_canvas, "trainlPictureBB_corners": trainlPictureBB_corners}
#dataObj = {"trainlPictureBB_full": trainlPictureBB_full, "trainlPictureBB_canvas": trainlPictureBB_canvas, "trainlPictureBB_corners": trainlPictureBB_corners}
with gzip.open(os.path.join(path, filename), 'wb') as handle:
    pkl.dump(dataObj, handle, protocol=pkl.HIGHEST_PROTOCOL)