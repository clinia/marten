namespace CliniaPOC.Models
{
    public class Department : Entity
    {
        public Department(string type) : base(type)
        {
        }

        public string HealthFacility { get; set; }
    }
}
