using System;
using UnityEngine;

public class RoadPart
{
    public int NumberOfLanes { get; set; }
    public float LaneWidth { get; set; }
    public float RoadWidth { get; set; }
    public float RoadLength { get; set; }

    public RoadPart(int numberOfLanes, float laneWidth, float roadWidth, float roadLength)
    {
        this.NumberOfLanes = numberOfLanes;
        this.LaneWidth = laneWidth;
        this.RoadWidth = roadWidth;
        this.RoadLength = roadLength;
    }
}
