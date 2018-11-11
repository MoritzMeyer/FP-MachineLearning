
using System;
using System.Collections.Generic;
using UnityEngine;

public class Road : ScriptableObject
{
    #region fields
    public Dictionary<int, Tuple<RoadPart, GameObject>> RoadParts { get; set; }
    public List<Tuple<Car, GameObject>> Cars { get; set; }
    #endregion

    #region ctor
    public Road()
    {
        this.RoadParts = new Dictionary<int, Tuple<RoadPart, GameObject>>();
        this.Cars = new List<Tuple<Car, GameObject>>();
    }
    #endregion

    #region InstantiateRoadPart
    /// <summary>
    /// Instanziiert das übergebenen (Straßen-) Prefab und fügt es dem Straßenobjekt hinzu.
    /// </summary>
    /// <param name="roadPrefab">Das Prefab des Straßenteils, der gespawnt werden soll.</param>
    /// <param name="roadPart">Die Daten zu dem Straßenteil.</param>
    /// <param name="roadPartIndex">Der Index des Straßenteils.</param>
    public void InstantiateRoadPart(GameObject roadPrefab, RoadPart roadPart, int roadPartIndex)
    {
        GameObject instance = Instantiate(roadPrefab, new Vector3(Convert.ToSingle(roadPartIndex) * roadPart.RoadLength, 0, 0), roadPrefab.transform.rotation);
        this.RoadParts.Add(roadPartIndex, new Tuple<RoadPart, GameObject>(roadPart, instance));
    }
    #endregion

    #region PositionCar
    /// <summary>
    /// Positioniert ein Auto auf einem bestimmten Teil der Straße.
    /// </summary>
    /// <param name="carPrefab">Das Prefab des Autos.</param>
    /// <param name="car">Die Daten zu dem Auto.</param>
    public void PositionCar(GameObject carPrefab, Car car)
    {
        Quaternion rotation = (car.ForwardDirection) ? carPrefab.transform.rotation : Quaternion.Euler(0, 270, 0);
        float zCoord = carPrefab.transform.position.z + (this.RoadParts[car.RoadPosition].First.LaneWidth * car.LanePosition);
        float xCoord = this.RoadParts[car.RoadPosition].Second.transform.position.x;
        float yCoord = carPrefab.transform.position.y;

        GameObject positionedCar = Instantiate(carPrefab, new Vector3(xCoord, yCoord, zCoord), rotation);
        this.Cars.Add(new Tuple<Car, GameObject>(car, positionedCar));
    }
    #endregion
}
