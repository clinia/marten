namespace CliniaPOC.Models
{
    public class HealthService : Entity
    {
        public HealthService(string type) : base(type)
        {
        }

        public string HealthFacilityId { get; set; }

        public string PractitionerId { get; set; }

        public string PracticeId { get; set; }

        public string DepartmentId { get; set; }

        public string HealthServiceId { get; set; }
    }
}
