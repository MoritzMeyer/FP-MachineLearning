/// <summary>
/// Klasse für Hilfsfunktionen.
/// </summary>
public static class RoadUtils
{
    /// <summary>
    /// Holt den Slerp zu einer Lane anhand der Anzahl der Spuren vom mittleren bis zum äußeren Punkt.
    /// </summary>
    /// <param name="numberOfTracks">Die Anzahl der Spuren.</param>
    /// <param name="lane">Die aktuelle Spur von links (0 - 7).</param>
    /// <returns>Den Slerp.</returns>
    public static float GetRoadSlerpByLane(int numberOfTracks, int lane)
    {
        // Je nach Anzahl der Spuren, die den Slerp zwischen Mitte, außen(linke ode rechte Marker) und richtiger Spur setzen
        switch (numberOfTracks)
        {
            case 4:
                // Entweder außen(0.75) oder innen (0.25)
                if (lane == 0 || lane == 3) return 0.75f;
                return 0.25f;
            case 6:
                // Entweder außen, mitte oder innen
                if (lane == 0 || lane == 5) return 0.825f;
                if (lane == 1 || lane == 4) return 0.5f;
                return 0.175f;
            case 8:
                // Entweder außen, linke mitte, rechte mitte, oder innen
                if (lane == 0 || lane == 5) return 0.85f;
                if (lane == 1 || lane == 4) return 0.6f;
                if (lane == 2 || lane == 3) return 0.4f;
                return 0.15f;
            case 2:
            default:
                // Hat nur eine Spur
                return 0.5f;
        }
    }
}
