using UnityEngine;
using EasyRoads3Dv3;
using System.Collections;
using Assets.Scripts.Classes;
using System.Collections.Generic;


public class CustomEasyRoad
{
    /// <summary>
    /// Alle Autos auf der Strecke.
    /// </summary>
    public List<Tuple<GameObject, int>> CarsOnLanes  { get; private set; }

    /// <summary>
    /// Das RoadNetwork.
    /// </summary>
    private readonly ERRoadNetwork Network;
    
    /// <summary>
    /// Die Road.
    /// </summary>
    public ERRoad Road { get; private set; }

    /// <summary>
    /// Die Autos pro Spur.
    /// </summary>
    public Dictionary<int, List<GameObject>> CarsPerLane { get; private set; } 

    /// <summary>
    /// Methode zum Erstellen einer CustomEasyRoad.
    /// </summary>
    /// <param name="car">Das Gameobject des Autos.</param>
    /// <param name="road">Die Straße.</param>
    /// <param name="minCars">Die Mindestanzahl von Autos auf dem Streckenpart.</param>
    /// <param name="maxCars">Die Maximalanzahl von Autos auf dem Streckenpart.</param>
    /// <param name="numberOfTracks">Die Anzahl der Spuren.</param>
    public CustomEasyRoad(GameObject car, ERRoad road, int minCars, int maxCars, int numberOfTracks)
    {
        road.SetTag("Street");

        this.Road = road;
        this.CarsOnLanes = new List<Tuple<GameObject, int>>();
        this.CarsPerLane = new Dictionary<int, List<GameObject>>();
        for (int i = 0; i < numberOfTracks; i++)
        {
            List<GameObject> carsOnLane = new List<GameObject>();
            CarsPerLane.Add(i, carsOnLane);
        }

        Vector3[] markers = road.GetSplinePointsCenter();
        Vector3[] markersR = road.GetSplinePointsRightSide();
        Vector3[] markersL = road.GetSplinePointsLeftSide();
        
        int carCount = Random.Range(minCars, maxCars);
        if (carCount > 0)
        {      
            int increment = markers.Length / carCount;
            Vector3 look = Vector3.zero;
            GameObject newCar = null;

            for (int i = 0; i < markers.Length; i+= increment)
            {
                // Die Spur bestimmen
                int lane = Random.Range(0, numberOfTracks);
                Vector3[] directionMarkers = null;

                // Die Richtung des Autos/Lane holen und setzen
                if (lane < (numberOfTracks / 2))
                {
                    directionMarkers = markersL;
                    look = (markers[Mathf.Max(0, i - 1)] - markers[Mathf.Min(markers.Length - 1, i + 1)]);
                }
                else
                {
                    directionMarkers = markersR;
                    look = (markers[Mathf.Min(markers.Length - 1, i + 1)] - markers[Mathf.Max(0, i - 1)]);
                }

                // Den RoadSlerp holen
                float roadSlerp = RoadUtils.GetRoadSlerpByLane(numberOfTracks, lane);

                // Das Car mit der Richtung und der Spur spawnen
                newCar = GameObject.Instantiate(car, Vector3.Slerp(markers[i], directionMarkers[i], roadSlerp) + new Vector3(0, 1, 0), Quaternion.LookRotation(look));

                // Das Auto den Listen hinzufügen
                this.AddToIndexOnLane(lane, newCar);
                CarsOnLanes.Add(new Tuple<GameObject, int>(newCar, numberOfTracks - lane - 1));
            }
        }
    }

    #region GetIncludingMarkers
    public Tuple<Vector3, Vector3> GetIncludingMarkers(Vector3 position)
    {
        Vector3[] markers = Road.GetMarkerPositions();

        // TODO: Marker zwischen der Position zurückgeben
        for (int i = 0; i < markers.Length - 1; i++)
        {
            Vector3 currentMarker = markers[i];
            Vector3 nextMarker = markers[i + 1];
            if (currentMarker.x <= position.x && currentMarker.y <= position.z && nextMarker.x >= position.x && nextMarker.z >= position.z
                || currentMarker.x >= position.x && currentMarker.y >= position.z && nextMarker.x <= position.x && nextMarker.z <= position.z)
            {
                return new Tuple<Vector3, Vector3>(currentMarker, nextMarker);
            }
        }

        return new Tuple<Vector3, Vector3>(Vector3.zero, Vector3.one);
    }
    #endregion

    #region AddToIndexOnLane
    /// <summary>
    /// Methode zum Hinzufügen eines Autos zum Dictionary auf einer bestimmten Strecke.
    /// </summary>
    /// <param name="i">Der Index der Lane.</param>
    /// <param name="car">Das Auto.</param>
    private void AddToIndexOnLane(int i, GameObject car)
    {
        // Die Liste der Spur holen
        List<GameObject> carsOnLane = CarsPerLane[i];

        // Das Auto hinzufügen
        carsOnLane.Add(car);

        // Die Autos der Spur neu setzen
        CarsPerLane[i] = carsOnLane;
    }
    #endregion
}
