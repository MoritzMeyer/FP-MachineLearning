using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetGeneratorScript : MonoBehaviour {

    public GameObject twoLaneRoad;
    public GameObject fourLaneRoad;

    public GameObject car;

    private Road road;
    private RoadPart twoLaneRoadPart;
    private RoadPart fourLaneRoadPart;

    private List<Car> randomCars;

	// Use this for initialization
	void Start ()
    {
        this.road = ScriptableObject.CreateInstance<Road>();
        this.twoLaneRoadPart = new RoadPart(2, 2.75f, 5.5f, 10f);

        this.randomCars = new List<Car>()
        {
            new Car(2, 2, false),
            new Car(4, 1, true)
        };


        this.GenerateStraight(twoLaneRoad, twoLaneRoadPart, 10);
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    /// <summary>
    /// Erzeugt eine gerade Straße in der angegebenen Anzahl.
    /// </summary>
    /// <param name="straightStreet">Das Prefab, welches gerendert werden soll.</param>
    /// <param name="numbParts">Die Anzahl an Instanzen, die von dem Prefab gerendert werden sollen.</param>
    private void GenerateStraight(GameObject straightStreet, RoadPart roadPart, int numbParts)
    {
        for (int i = 0; i < numbParts; i++)
        {
            road.InstantiateRoadPart(straightStreet, roadPart, i);
        }

        for (int j = 0; j < randomCars.Count; j++)
        {
            road.PositionCar(car, randomCars[j]);
        }

        //GameObject instantiatedCar = Instantiate(car);
        //instantiatedCar.transform.localScale += new Vector3(12f, 12f, 12f);
        //instantiatedCar.transform.SetPositionAndRotation(new Vector3(0f, 0.428f, -1.25f), Quaternion.Euler(0, 90, 0));
    }
}
