using System.Collections.Generic;
using CliniaPOC.Models;
using Marten.Linq;

namespace CliniaPOC.Infrastructure
{
    public interface IIncludeMapper<T>
    {
        Dictionary<string, HealthFacility> HealthFacilities { get; }

        Dictionary<string, Practice> Practices { get; }

        Dictionary<string, Practitioner> Practitioners { get; }

        Dictionary<string, HealthService> HealthServices { get; }

        Dictionary<string, Department> Departments { get; }

        IMartenQueryable<T> Include(IEnumerable<string> includes);
    }
}
