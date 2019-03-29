namespace Assets.Scripts.Configuration
{
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Klasse repräsentiert die Konfiguration einer Strecke.
    /// </summary>
    [Serializable]
    public class RoadConfig
    {
        /// <summary>
        /// Die Anzahl der Spuren auf der Straße (Momentane Optionen: 2, 4, 6, 8).
        /// </summary>
        public int NumberOfTracks;

        /// <summary>
        /// Die Geschwindigkeit des Autos in Km/h.
        /// </summary>
        public int CarSpeed;

        /// <summary>
        /// Ob, der Nutzer das Auto selber steuern möchte.
        /// </summary>
        public bool IsSelfDriving;

        /// <summary>
        /// Der optionale Seed, falls die gleiche Strecke wieder generiert werden soll.
        /// </summary>
        public string Seed;

        /// <summary>
        /// Die Streckenabschnitte.
        /// </summary>
        public List<RoadPartConfig> RoadItems;
    }
}