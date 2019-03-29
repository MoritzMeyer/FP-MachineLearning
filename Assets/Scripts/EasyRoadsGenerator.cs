using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyRoads3Dv3;
using System.Linq;
using System;
using Assets.Scripts.Extensions;
using Assets.Scripts.Classes;
using System.IO;
using System.Text;
using System.Globalization;

public class EasyRoadsGenerator : MonoBehaviour
{
    public ERRoadNetwork network;
    public GameObject cameraCar;
    public GameObject car;
    [Range(0.001f, 0.2f)]
    public float percentageEven;
    public List<CustomEasyRoad> customEasyRoads;
    [HideInInspector]
    public bool isGenerated = false;
    public bool isSelfDriving = false;
    [HideInInspector]
    public bool isPlaced = false;
    public int carSpeed = 50;
    public int numberOfTracks = 0;
    private CultureInfo culture;

    void Start()
    {
        network = new ERRoadNetwork();
        network.BuildRoadNetwork();
        customEasyRoads = new List<CustomEasyRoad>();
        ProjectOnCamera2D projectOnCamera2D = car.GetComponent<ProjectOnCamera2D>();
        projectOnCamera2D.cam = Camera.main;

        culture = CultureInfo.CreateSpecificCulture("en-US");
    }

    void Update()
    {
        
    }

    #region FixedUpdate
    void FixedUpdate()
    {
        if (isGenerated)
        {
            if (!isPlaced)
            {
                this.PlaceCameraCar();
            }

            // Wenn das Auto nicht selber fährt, dann das Fahren simulieren
            if (this.isSelfDriving)
            {
                SimulateCar();
            }

            // Den ScreenRecorder aktivieren
            ScreenRecorder screenRecorder = Camera.main.GetComponent<ScreenRecorder>();
            screenRecorder.isGenerated = true;
            screenRecorder.updateCounter++;

            if (screenRecorder.updateCounter % screenRecorder.takePictureEveryXFrame == 0 && screenRecorder.capture)
            {
                // Über alle Autos iterieren und die Koordinaten der sichtbaren speichern.
                //string textToAppend = "Picture " + screenRecorder.counter + ":";
                string textToAppend = string.Empty;
                List<Tuple<GameObject, int>> visibleCars = new List<Tuple<GameObject, int>>();
                foreach (CustomEasyRoad ceRoad in customEasyRoads)
                {
                    foreach (Tuple<GameObject, int> carOnLane in ceRoad.CarsOnLanes)
                    {
                        if (carOnLane == null || carOnLane.First == null)
                        {
                            continue;
                        }

                        ProjectOnCamera2D projectOnCamera2D = carOnLane.First.GetComponent<ProjectOnCamera2D>();

                        // Wenn das Auto auf dem Screen sichtbar ist, die Koordinaten speichern.
                        if (projectOnCamera2D.IsVisible)
                        {
                            visibleCars.Add(carOnLane);
                        }
                    }
                }

                if (visibleCars.Count != 1)
                {
                    return;
                }

                Tuple<GameObject, int> visibleCar = visibleCars.First();
                textToAppend +=
                    visibleCar.Second + "," +
                    visibleCar.First.GetComponent<ProjectOnCamera2D>()
                        .getRelativeBoxCoords()
                        .Select(c => c.First.ToString("G", culture) + "," + c.Second.ToString("G", culture))
                        .Aggregate((a, b) => a + "," + b)
                    + ";";
                screenRecorder.TakePicture(textToAppend);
            }

            DestroyColliderCars();
        }
    }
    #endregion

    #region SetUpRoadType
    /// <summary>
    /// Methode zum Aufsetzen des RoadTypes anhand der Lanes.
    /// </summary>
    public void SetUpRoadType()
    {
        // Die Strecke neu holen
        this.network = new ERRoadNetwork();
        this.network.BuildRoadNetwork();

        foreach (ERRoadType roadType in this.network.GetRoadTypes())
        {

            switch (this.numberOfTracks)
            {
                case 8:
                    // Falls acht Spuren benötigt werden (roadWidth wurde angepasst)
                    roadType.roadMaterial = Resources.Load<Material>("Road8Lanes");
                    roadType.roadWidth = 30;
                    roadType.Update();
                    break;
                case 6:
                    // Falls sechs Spuren benötigt werden (roadWidth wurde angepasst)
                    roadType.roadMaterial = Resources.Load<Material>("Road6Lanes");
                    roadType.roadWidth = 20;
                    roadType.Update();
                    break;
                case 4:
                    // Falls vier Spuren benötigt werden
                    roadType.roadMaterial = Resources.Load<Material>("Road4Lanes");
                    roadType.roadWidth = 12;
                    roadType.Update();
                    break;
                case 2:
                default:
                    // Falls zwei Spuren benötigt werden (roadMaterial stimmt schon)
                    roadType.roadWidth = 6;
                    roadType.Update();
                    break;
            }
        }
    }
    #endregion

    #region CreateCurve
    /// <summary>
    /// Erstellt eine Kurve anhand eines Winkels, der Länge der Kurve und den Positionen des aktuellen und vorherigen Straßen Elementes.
    /// </summary>
    /// <param name="angle">Der Winkel.</param>
    /// <param name="length">Die Länge des Straßenelementes.</param>
    /// <param name="heightDifference">Die Höhendifferenz für den Streckenabschnitt.</param>
    /// <param name="minCars">Die minimale Anzahl an Autos auf diesem Streckenabschnitt.</param>
    /// <param name="maxCars">Die maximale Anzahl an Autos auf diesem Streckenabschnitt.</param>
    /// <param name="seed">Der Seed des Random-Generators.</param>
    /// <returns>Die Kurve.</returns>
    public ERRoad CreateCurve(float angle, float length, float? heightDifference, int minCars, int maxCars, string seed)
    {
        // Die Strecke neu holen
        this.network = new ERRoadNetwork();
        this.network.BuildRoadNetwork();

        // hole die Höhendifference
        float fixHeightDifference = heightDifference ?? 0f;

        // Die StartPosition initialisieren.
        Vector3 startPosition = new Vector3(0, 0, 0);

        // Die Ausrichtung initialisieren (default ist z-Richtung).
        Vector3 heading = new Vector3(0, 0, 1);

        // Den RoadType holen
        ERRoadType roadType = this.GetRandomRoadType();

        // Hole die Position des letzten Streckenabschnitts, wenn vorhanden.
        ERRoad lastRoad = null;
        if (network.GetRoads().Length > 0)
        {
            lastRoad = network.GetRoads().Last();
            Vector3[] markers = lastRoad.GetMarkerPositions();
            Vector3 lastPosition = markers.Last();

            // Die Startposition an den letzten Streckenabschnitt anpassen.
            startPosition = lastPosition;

            // Die Ausrichtung in Bezug auf den vorherigen Streckenabschnitt holen.
            Vector3 secondToLast = markers[markers.Count() - 2];
            heading = lastPosition - secondToLast;
            heading.y = 0;
        }

        // Den (geraden) Richtungsvektor berechnen.
        Vector3 direction = heading / heading.magnitude;

        // Der Vektor der y-Achse
        Vector3 yAxis = new Vector3(0, 1, 0);

        // Die Anzahl an zu berechnenden Positionen für die Kurve
        int numbPositions = Convert.ToInt32(Math.Abs(angle));
        float positionPercentage = numbPositions * percentageEven;

        // Das Array mit den neuen Positionen.
        Vector3[] curvePositions = new Vector3[numbPositions];
        curvePositions[0] = startPosition;

        // es werden in 1-Grad-Schritten Positionen berechnet.
        float anglePart = angle / Math.Abs(angle);
        float lengthPart = length / numbPositions;
        float heightPart = fixHeightDifference / (numbPositions - (2 * positionPercentage));

        // Die Positionen berechnen.
        for (int i = 1; i < numbPositions; i++)
        {
            // Die direction für den nächsten Schritt berechnen
            if (i > 1)
            {
                heading = curvePositions[i - 1] - curvePositions[i - 2];
                heading.y = 0;
                direction = heading / heading.magnitude;
            }           

            // Die letzte Position holen.
            Vector3 oldPosition = curvePositions[i - 1];

            // innerhalb des Prozent-Bereiches die Höhe anwenden.
            if (i > positionPercentage && i < (numbPositions - positionPercentage))
            {
                oldPosition.y += heightPart.Truncate(5);
            }
            
            // Die neue Position berechnen.
            curvePositions[i] = oldPosition + Quaternion.AngleAxis(anglePart, yAxis) * direction * lengthPart;
        }

        // Die Kurve erzeugen.
        ERRoad thisRoad = this.network.CreateRoad("Curve" + network.GetRoads().Count(), roadType, curvePositions);
        customEasyRoads.Add(new CustomEasyRoad(car, thisRoad, minCars, maxCars, numberOfTracks));
        return thisRoad;
    }
    #endregion

    #region CreateStraight
    /// <summary>
    /// Methode zum Zeichnen einer geraden Straße.
    /// </summary>
    /// <param name="length">Die Länge der Straße.</param>
    /// <param name="minCars">Die minimale Anzahl der Autos auf dem Straßenteil.</param>
    /// <param name="maxCars">Die maximale Anzahl der Autos auf dem Straßenteil.</param>
    /// <param name="heightDifference">Der Höhenunterschied.</param>
    /// <param name="seed">Der Seed.</param>
    /// <returns>Die Straße.</returns>
    public ERRoad CreateStraight(float length, int minCars, int maxCars, float? heightDifference, string seed)
    {
        // Die Strecke neu holen
        this.network = new ERRoadNetwork();
        this.network.BuildRoadNetwork();

        // Den RoadType holen
        ERRoadType roadType = this.GetRandomRoadType();

        // Hole die akutellen Streckenteile
        ERRoad[] currentRoads = network.GetRoads();

        // Hole die Höhe der Strecke
        float fixHeightDifference = heightDifference ?? 0;

        // Lege die Positionen der Strecke an
        Vector3 startPosition = new Vector3(0, 0, 0);
        Vector3 middlePosition = new Vector3(0, fixHeightDifference / 2, length / 2);
        Vector3 endPosition = new Vector3(0, fixHeightDifference, length);

        ERRoad lastRoad = null;
        ERRoad road = null;

        if (currentRoads.Length > 0)
        {
            // Hole die letzte Strecke
            lastRoad = currentRoads.Last();

            // Hole den letzten Punkt der Strecke
            Vector3[] markers = lastRoad.GetMarkerPositions();
            Vector3 lastMarker = markers.Last();

            // Die richtige Rotation ausrechnen
            Vector3 heading = (lastMarker - markers[markers.Length - 2]);
            Vector3 direction = heading / heading.magnitude;
            direction.y = 0;

            // Das Verhältnis zwischen x und z-Achse ausrechnen
            float x = direction.x / (direction.magnitude);
            float z = direction.z / (direction.magnitude);

            Vector3[] streetVectors = new Vector3[(int)length];
            float heightPart = fixHeightDifference / length;
            for (int lengthPart = 0; lengthPart < length; lengthPart++)
            {
                streetVectors[lengthPart] = lastMarker + new Vector3(x * lengthPart, heightPart * lengthPart, z * lengthPart);
            }

            // Generiere Straße
            road = network.CreateRoad("Straight" + currentRoads.Length, roadType, streetVectors);
        }
        else
        {
            // Generiere erste Straße
            road = network.CreateRoad("Straight" + currentRoads.Length, roadType, new Vector3[] { startPosition, middlePosition, endPosition });
        }

        // Erstelle die Strecke mit einem eindeutigen Namen
        customEasyRoads.Add(new CustomEasyRoad(car,road,minCars,maxCars, numberOfTracks));
        return road;
    }
    #endregion

    #region SimulateCar
    /// <summary>
    /// Methode zum Fahren des Autos (Simulation).
    /// </summary>
    public void SimulateCar()
    {
        // Die Collider des Autos holen
        Collider[] colliders = Physics.OverlapBox(cameraCar.gameObject.transform.position, (cameraCar.gameObject.transform.localScale / 5f), cameraCar.gameObject.transform.rotation);
        ERRoad road = null;
        foreach (Collider collider in colliders)
        {
            if (collider.tag == "Street")
            {
                road = network.GetRoadByName(collider.name);
            }
        }

        Vector3 heading = new Vector3(0, 0, 1);
        if (road != null)
        {
            // Hole den letzten Punkt der Strecke
            Vector3[] markers = road.GetMarkerPositions();
            Vector3 lastMarker = markers.Last();

            // Die richtige Rotation ausrechnen
            heading = (lastMarker - markers[markers.Length - 2]).normalized;
            heading.y = 0;
            CustomEasyRoad customEasy = null;
            foreach (CustomEasyRoad customEasyRoad in customEasyRoads)
            {
                if (customEasyRoad.Road.GetName() == road.GetName())
                {
                    customEasy = customEasyRoad;
                    break;
                }
            }

            //Tuple<Vector3, Vector3> markers = customEasy.GetIncludingMarkers(cameraCar.gameObject.transform.position);
            //heading = (markers.Second - markers.First).normalized;
            //heading.y = 0;
        }

        // Geschwindigkeit setzen
        Rigidbody rigidbody = cameraCar.GetComponent<Rigidbody>();
        cameraCar.transform.Translate(Vector3.forward * (carSpeed / 3.6f) * Time.deltaTime);
        //cameraCar.transform.rotation.SetLookRotation(heading);
        cameraCar.transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading), 2.5f * Time.deltaTime);
    }
    #endregion

    #region PlaceCameraCar
    /// <summary>
    /// Methode zum Platzieren des Autos am Anfang.
    /// </summary>
    private void PlaceCameraCar()
    {
        // Das Auto auf die rechte erste Spur platzieren
        ERRoad firstRoad = network.GetRoads().First();
        Vector3 firstSplineRightSide = firstRoad.GetSplinePointsRightSide()[0];
        Vector3 firstMarker = firstRoad.GetMarkerPosition(0);
        Vector3 rightMiddleLane = Vector3.zero;

        // Das Position je nach Anzahl der Spuren interpolieren
        switch(this.numberOfTracks)
        {
            case 8:
                rightMiddleLane = Vector3.Slerp(firstMarker, firstSplineRightSide, 0.85f);
                break;
            case 6:
                rightMiddleLane = Vector3.Slerp(firstMarker, firstSplineRightSide, 0.825f);
                break;
            case 4:
                rightMiddleLane = Vector3.Slerp(firstMarker, firstSplineRightSide, 0.75f);
                break;
            case 2:
            default:
                rightMiddleLane = Vector3.Slerp(firstMarker, firstSplineRightSide, 0.5f);
                break;
        }

        // Das Auto an die richtige Positions etzen
        cameraCar.gameObject.transform.position = rightMiddleLane;
        cameraCar.gameObject.transform.rotation = Quaternion.identity;

        // Ist platziert setzen
        isPlaced = true;
    }
    #endregion

    #region GetRandomRoadType
    /// <summary>
    /// Liefert einen zufälligen RoadType des aktuellen Netzwerkes zurück.
    /// </summary>
    /// <returns>Der zufällige RoadType</returns>
    ERRoadType GetRandomRoadType()
    {
        int index = UnityEngine.Random.Range(0, this.network.GetRoadTypes().Count());

        return this.network.GetRoadTypes()[index];
    }
    #endregion GetRandomRoadType

    #region DestroyColliderCars
    /// <summary>
    /// Methode zum Zerstören der Autos im Weg.
    /// </summary>
    private void DestroyColliderCars()
    {
        // Die Collider des Autos holen
        Collider[] colliders = Physics.OverlapBox(cameraCar.gameObject.transform.position, (cameraCar.gameObject.transform.localScale / 2.5f), cameraCar.gameObject.transform.rotation);
        List<Collider> carColliders = new List<Collider>();
        foreach (Collider collider in colliders)
        {
            if (collider.tag == "Car")
            {
                carColliders.Add(collider);
            }
        }

        // Die Autos im Weg entfernen
        foreach (Collider collider in carColliders)
        {
            Destroy(collider.gameObject);
        }
    }

    #endregion
}
