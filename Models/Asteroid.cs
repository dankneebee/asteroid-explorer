using System;

namespace AsteroidExplorer.Models
{
    public class Asteroid
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double EstimatedDiameter { get; set; }
        public bool IsPotentiallyHazardous { get; set; }
        public DateTime CloseApproachDate { get; set; }
        public double MissDistance { get; set; }
        public string OrbitingBody { get; set; }
    }
} 