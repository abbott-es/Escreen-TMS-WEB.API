using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Coordinate : IEntity
    {
        public Guid CoordinateID { get; set; }
        public Guid ID => CoordinateID;
        public string CoordinateName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
