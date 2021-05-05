using System;
using System.Collections.Generic;
using CliniaPOC.Models;
using Marten.Linq;

namespace CliniaPOC.Infrastructure
{
    public abstract class IncludeMapper<T> : IIncludeMapper<T>
    {
        public IncludeMapper(IMartenQueryable<T> queryable)
        {
            Queryable = queryable;
            HealthFacilities = new Dictionary<string, HealthFacility>();
            Practices = new Dictionary<string, Practice>();
            Practitioners = new Dictionary<string, Practitioner>();
            HealthServices = new Dictionary<string, HealthService>();
            Departments = new Dictionary<string, Department>();
        }

        protected IMartenQueryable<T> Queryable { get; set; }
        public Dictionary<string, HealthFacility> HealthFacilities { get; }
        public Dictionary<string, Practice> Practices { get; }
        public Dictionary<string, Practitioner> Practitioners { get; }
        public Dictionary<string, HealthService> HealthServices { get; }
        public Dictionary<string, Department> Departments { get; }
        protected abstract Type Include(string include);

        public IMartenQueryable<T> Include(IEnumerable<string> includes)
        {
            foreach (var include in includes)
            {
                var includePaths = include.Split('.');

                var includeType = Include(includePaths[0]);
                if (includeType == null)
                    continue;

                for (var i = 1; i < includePaths.Length; i++)
                {
                    includeType = ThenInclude(includeType, includePaths[i]);
                    if (includeType == null)
                        break;
                }
            }

            return Queryable;
        }

        protected Type ThenInclude(Type sourceType, string include)
        {
            if (sourceType == typeof(HealthFacility))
            {
                Queryable = HealthFacilityIncludeMapper.ThenInclude(Queryable, include, this);
                return HealthFacilityIncludeMapper.GetRelationshipType(include);
            }

            if (sourceType == typeof(Practitioner))
            {
                Queryable = PractitionerIncludeMapper.ThenInclude(Queryable, include, this);
                return PractitionerIncludeMapper.GetRelationshipType(include);
            }

            if (sourceType == typeof(Practice))
            {
                Queryable = PracticeIncludeMapper.ThenInclude(Queryable, include, this);
                return PracticeIncludeMapper.GetRelationshipType(include);
            }

            return null;
        }
    }
}
