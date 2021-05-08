using System;
using CliniaPOC.Models;
using Marten.Linq;

namespace CliniaPOC.Infrastructure
{
    public class PractitionerIncludeMapper : IncludeMapper<Practitioner>
    {
        public PractitionerIncludeMapper(IMartenQueryable<Practitioner> queryable): base(queryable)
        {
        }

        protected override Type Include(string include)
        {
            Queryable = include switch
            {
                "practices" => Queryable.IncludeInverted(x => x.HealthFacility, Practices),
                "healthServices" => Queryable.IncludeInverted(x => x.HealthFacilityId, HealthServices),
                _ => Queryable
            };

            return GetRelationshipType(include);
        }

        public static IMartenQueryable<T> ThenInclude<T>(IMartenQueryable<T> queryable,  string include, IIncludeMapper<T> mapper)
        {
            queryable = include switch
            {
                "practices" => queryable.ThenIncludeInverted<Practitioner, Practice, string>(x => x.HealthFacility, mapper.Practices),
                "healthServices" => queryable.ThenIncludeInverted<Practitioner, HealthService, string>(x => x.HealthFacilityId, mapper.HealthServices),
                _ => queryable
            };

            return queryable;
        }

        public static Type GetRelationshipType(string include) => include switch
        {
            "practices" => typeof(Practice),
            "healthServices" => typeof(HealthService),
            _ => null
        };
    }
}
