namespace Assets.Scripts.Configuration
{
    using System;
    using System.IO;
    using UnityEngine;

    public class ConfigurationLoader : MonoBehaviour
    {
        private const string configName = "config-big.json";

        public RoadConfig Config { get; set; }

        // Use this for initialization
        void Start() {
            string configPath = Path.Combine(Application.streamingAssetsPath, configName);
            EasyRoadsGenerator generator = GetComponent<EasyRoadsGenerator>();


            using (StreamReader r = new StreamReader(configPath))
            {
                string json = r.ReadToEnd();
                Debug.Log("Config loaded: " + json);
                this.Config = JsonUtility.FromJson<RoadConfig>(json);

                // Die Anzahl der Lanes setzen
                generator.numberOfTracks = this.Config.NumberOfTracks;
                generator.SetUpRoadType();

                foreach (RoadPartConfig roadPartConfig in Config.RoadItems)
                {
                    switch(roadPartConfig.Type)
                    {
                        case RoadPartType.Straight:
                            generator.CreateStraight(roadPartConfig.Length, roadPartConfig.MinCars, roadPartConfig.MaxCars, roadPartConfig.HeightDifference, roadPartConfig.Seed);
                            break;
                        case RoadPartType.Curve:
                            generator.CreateCurve(roadPartConfig.Angle, roadPartConfig.Length, roadPartConfig.HeightDifference, roadPartConfig.MinCars, roadPartConfig.MaxCars, roadPartConfig.Seed);
                            break;
                        default:
                            throw new Exception("Undefined Road Type '" + roadPartConfig.Type + "'");
                    }
                }
            }

            generator.isSelfDriving = Config.IsSelfDriving;
            generator.carSpeed = Config.CarSpeed;
            generator.isGenerated = true;

            ScreenRecorder screenRecorder = Camera.main.GetComponent<ScreenRecorder>();
            screenRecorder.ObjectsToHide = GameObject.FindGameObjectsWithTag("ObjectToHide");
        }

        // Update is called once per frame
        void Update() {

        }
    }
}