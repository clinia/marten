namespace CliniaPOC.Models
{
    public class Practice : Entity
    {
        public Practice(string type) : base(type)
        {
        }
        
        public string HealthFacility { get; set; }
        
        public string Practitioner { get; set; }
    }
}