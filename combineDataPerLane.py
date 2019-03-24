import inspect, os, sys, shutil
'Wenn ein Pfad als Argument übergeben würde, nehme diesen.'
'Sonst den Pfad des Ordners in dem das Skript liegt'
if len(sys.argv) == 1:
    path = os.path.dirname(os.path.abspath(inspect.getfile(inspect.currentframe())))
else:
    path = sys.argv[1]

'Der übergebene Pfad muss ein Ordner sein'
if not os.path.isdir(path):
    raise Exception("The given path has to be a directory!")

combinedDataFolder = "AllPicturesPerLane"
extractDataFolder = "PicturesPerLane"
combinedDataPath = os.path.join(path, combinedDataFolder)

def combineDataFromFolder(folderPath):
    extractDataDirectory = os.path.join(folderPath, extractDataFolder)
    for root, dirs, files in os.walk(extractDataDirectory):
        for name in dirs:
            lane = int(name[-1:])
            carCoords = []
            carCoordsInCombinedDirectory = []

            'Crate folder in combinedDataFolder'
            combinedDataPerLanePath = os.path.join(combinedDataPath, name)
            if not os.path.isdir(combinedDataPerLanePath):
                os.mkdir(combinedDataPerLanePath)

            extractDataPathPerLane = os.path.join(extractDataDirectory, name)
            with open(os.path.join(extractDataPathPerLane, "carCoords.txt")) as f:
                carCoords.extend(f.readlines())
                carCoords = [x.strip() for x in carCoords]

            'carCoords.txt erstellen wenn nicht vorhanden'
            if not os.path.isfile(os.path.join(combinedDataPerLanePath, "carCoords.txt")):
                #os.mknod(os.path.join(combinedDataPerLanePath, "carCoords.txt"))
                with open(os.path.join(combinedDataPerLanePath, "carCoords.txt"), 'w'): pass

            with open(os.path.join(combinedDataPerLanePath, "carCoords.txt")) as f:
                carCoordsInCombinedDirectory.extend(f.readlines())
                carCoordsInCombinedDirectory = [x.strip() for x in carCoordsInCombinedDirectory]

            'Get all Pictures'
            pictures = []
            for root, dirs, files in os.walk(extractDataPathPerLane, topdown=False):
                pictures.extend(files)

            pictures = [name for name in pictures if '.jpg' in name]

            'Die Bilder kopieren'
            indexOffset = len(carCoordsInCombinedDirectory)
            for fileName in pictures:
                'hole die Teile des Dateinamens'
                fileNameParts = fileName.split('.')
                nameParts = fileNameParts[0].split('_')
                index = int(nameParts[2])

                'Die line mit carCoords zum Bild'
                coordData = carCoords[index]

                'Die einzelnen Daten der Bounding Boxes extrahieren'
                pictureData = coordData.split(';')

                'erst das Bild kopieren'
                shutil.copy2(os.path.join(extractDataPathPerLane, fileName), os.path.join(combinedDataPerLanePath, nameParts[0] + "_" + nameParts[1] + "_" + str(indexOffset + index).zfill(5) + "." + fileNameParts[1]))

                'dann die Daten in den txt-File schreiben'
                with open(os.path.join(combinedDataPerLanePath, "carCoords.txt"), "a") as carCoordsFile:
                    carCoordsFile.write(pictureData[0] + ";" + "\n")


'wenn der AllPicturesPerLane bereits existiert löschen'
if os.path.isdir(combinedDataPath):
    shutil.rmtree(combinedDataPath)
os.mkdir(combinedDataPath)

print("Folders to be combined:")
for root, dirs, files in os.walk(path, topdown=False):
    print(dirs, path)
    for name in dirs:
        print(os.path.join(root, name))
        if (not name == extractDataFolder) and (not name == combinedDataFolder):
            combineDataFromFolder(os.path.join(root, name))