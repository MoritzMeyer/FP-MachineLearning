namespace Assets.Scripts.Configuration
{
    using System;

    /// <summary>
    /// Klasse repräsentiert einen Teil einer Strecke.
    /// </summary>
    [Serializable]
    public class RoadPartConfig
    {
        /// <summary>
        /// Die Länge des Streckenteils.
        /// </summary>
        public float Length;

        /// <summary>
        /// Der Typ der Strecke (0 = Gerade, 1 = Kurve);
        /// </summary>
        public RoadPartType Type;

        /// <summary>
        /// Die minimale Anzahl der Autos auf diesem Streckenteil.
        /// </summary>
        public int MinCars;

        /// <summary>
        /// Die maximale Anzahl der Autos auf diesem Streckenteil.
        /// </summary>
        public int MaxCars;

        /// <summary>
        /// Der optionale Höhenunterschied zwischen Anfang und Ende.
        /// </summary>
        public float HeightDifference;

        /// <summary>
        /// Der optionale Winkel, falls eine Kurve vorliegt.
        /// </summary>
        public int Angle;

        /// <summary>
        /// Der optionale Seed, falls die gleiche Strecke wieder geneiert werden soll.
        /// </summary>
        public string Seed;
    }
}