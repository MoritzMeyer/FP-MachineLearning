using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour {

    public GameObject twoLanes;

    public GameObject fourLanes;

    public GameObject eightlanes;

    public GameObject car;

    public Car cameraCar = new Car(0, 1, true);

    public int numberOfLanes = 4;

    public const int numberOfRoadParts = 10;

    public const float laneWidth = 3f;

    public const float roadLength = 12f;

    /// <summary>
    /// Das Dictionary, wo die entgegenkommenden Autos gespawned werden sollen.
    /// </summary>
    public List<Car> cars = new List<Car>
    {
        new Car(2, 2, false),
        new Car(4, 3, false)
    };

	// Use this for initialization
	void Start () {
		
        // Generate 
        switch(numberOfLanes)
        {
            case 2:
                this.GenerateTwoLaneStreet();
                break;
            case 4:
                this.GenerateFourLanesStreet();
                break;
            case 8:
                this.GenerateEightLanesStreet();
                break;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Methode zum Generieren einer Straße mit zwei Bahnen.
    /// </summary>
    private void GenerateTwoLaneStreet()
    {
        for (int i = 0; i < numberOfRoadParts; i++)
        {
            Instantiate(twoLanes, new Vector3(i * roadLength, 0, 0), twoLanes.transform.rotation);
        }

        Instantiate(car);
    }

    /// <summary>
    /// Methode zum Generieren einer Straße mit vier Bahnen.
    /// </summary>
    private void GenerateFourLanesStreet()
    {
        for (int i = 0; i < numberOfRoadParts; i++)
        {
            // Die Straße spawnen
            Instantiate(fourLanes, new Vector3(i * roadLength, 0, 0), twoLanes.transform.rotation);

            // Die Autos für diese Straße suchensuchen
            List<Car> streetCars = cars.FindAll((car) => car.LanePosition == i);
            if(streetCars.Count > 0)
            {
                // Die Autos für die aktuelle Straße durchlaufen und an der richtigen Stelle spawnen
                foreach(Car streetCar in streetCars)
                {
                    Quaternion rotation = (streetCar.ForwardDirection) ? car.transform.rotation : Quaternion.Euler(0, 270, 0);
                    float zCoord = laneWidth * streetCar.LanePosition;
                    Instantiate(car, new Vector3(i * roadLength, car.transform.position.y, car.transform.position.z + zCoord), rotation);
                }
            }
        }

        // Das CameraAuto spawnen
        float zCoordCar = laneWidth * cameraCar.LanePosition;
        Instantiate(car, new Vector3(car.transform.position.x + (cameraCar.RoadPosition * roadLength), car.transform.position.y, car.transform.position.z + zCoordCar), car.transform.rotation);
    }

    /// <summary>
    /// Methode zum Generieren einer Straße mit acht Bahnen.
    /// </summary>
    private void GenerateEightLanesStreet()
    {
        for (int i = 0; i < numberOfRoadParts; i++)
        {
            Instantiate(eightlanes, new Vector3(i * roadLength, 0, 0), eightlanes.transform.rotation);
        }

        Instantiate(car);
    }
}
