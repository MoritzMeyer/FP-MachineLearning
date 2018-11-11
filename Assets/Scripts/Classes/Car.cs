
public class Car
{

    public int RoadPosition { get; set; }
    public int LanePosition { get; set; }
    public bool ForwardDirection { get; set; }

    public Car(int roadPosition, int lanePosition, bool forwardDirection = true)
    {
        this.RoadPosition = roadPosition;
        this.LanePosition = lanePosition;
        this.ForwardDirection = forwardDirection;
    }
}

