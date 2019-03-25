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

extractDataFolder = "PicturesPerLane"

def extractDataFromFolder(path):
    carCoords = []

    'Create Folder'
    os.mkdir(os.path.join(path, extractDataFolder))

    with open(os.path.join(path, "carCoords.txt")) as f:
        carCoords.extend(f.readlines())
        carCoords = [x.strip() for x in carCoords]

    pictures = []
    for root, dirs, files in os.walk(path, topdown=False):
        pictures.extend(files)

    pictures = [name for name in pictures if '.jpg' in name]

    laneDataIndizes = [0, 0, 0, 0, 0, 0, 0, 0]
    for fileName in pictures:
        'nachschauen ob daten für das Bild vorhanden und die Lane extrahieren'

        'hole den Index des Bildes um die entsprechenden Daten aus carCoords.txt zu holen'
        fileNameParts = fileName.split('.')
        nameParts = fileNameParts[0].split('_')
        index = int(nameParts[2])

        'Wenn zu dem Bild keine Informationen getrackt wurden abbrechen'
        if not index < len(carCoords):
            continue

        'Die line mit carCoords zum Bild'
        coordData = carCoords[index]

        'Die einzelnen Daten der Bounding Boxes extrahieren'
        pictureData = coordData.split(';')

        'Es werden nur Bilder mit einer BoundingBox berücksichtigt ("data;".split(;).len = 2)'
        if len(pictureData) != 2:
            continue

        'Die Lane des Autos holen'
        carValues = pictureData[0].split(',')
        lane = int(carValues[0])

        'Den Ordner + carCoords.txt für die Lane erstellen, wenn noch nicht vorhanden'
        lanePath = os.path.join(path, extractDataFolder, "Lane" + str(lane))
        if not os.path.isdir(lanePath):
            os.mkdir(lanePath)

        'erst das Bild kopieren und umbenenne'
        shutil.copy2(os.path.join(path, fileName), os.path.join(lanePath, nameParts[0] + "_" + nameParts[1] + "_" + str(laneDataIndizes[lane]).zfill(5) + "." + fileNameParts[1]))

        'dann die Textcoords schreiben (wenn die erst geschrieben würden und das Bild nicht kopiert werden sollte (warum auch immer), passen die lines nicht mehr)'
        with open(os.path.join(lanePath, "carCoords.txt"), "a") as carCoordsFile:
            carCoordsFile.write(pictureData[0] + ";" + "\n")
        laneDataIndizes[lane] += 1

print("Folders to be processed:")
for root, dirs, files in os.walk(path, topdown=False):
    #print(dirs, path)
    for name in dirs:
        print(os.path.join(root, name))
        if (not name == extractDataFolder) and (not name == "AllPicturesPerLane"):
            'Wenn PicturesPerLane bereits existiert, löschen'
            if os.path.isdir(os.path.join(root, name, extractDataFolder)):
                shutil.rmtree(os.path.join(root, name, extractDataFolder))
            extractDataFromFolder(os.path.join(root, name))