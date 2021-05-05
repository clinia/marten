using System;
using CliniaPOC.Models;
using Marten.Linq;

namespace CliniaPOC.Infrastructure
{
    public class PracticeIncludeMapper : IncludeMapper<Practice>
    {
        public PracticeIncludeMapper(IMartenQueryable<Practice> queryable) : base(queryable)
        {
        }

        protected override Type Include(string include)
        {
            Queryable = include switch
            {
                "healthFacility" => Queryable.Include(x => x.HealthFacility, HealthFacilities),
                "practitioner" => Queryable.Include(x => x.Practitioner, Practitioners),
                _ => Queryable
            };

            return GetRelationshipType(include);
        }

        public static IMartenQueryable<T> ThenInclude<T>(IMartenQueryable<T> queryable,  string include, IIncludeMapper<T> mapper)
        {
            queryable = include switch
            {
                "healthFacility" => queryable.ThenInclude<Practice, HealthFacility, string>(x => x.HealthFacility, mapper.HealthFacilities),
                "practitioner" => queryable.ThenInclude<Practice, Practitioner, string>(x => x.Practitioner, mapper.Practitioners),
                _ => queryable
            };

            return queryable;
        }

        public static Type GetRelationshipType(string include) => include switch
        {
            "healthFacility" => typeof(HealthFacility),
            "practitioner" => typeof(Practitioner),
            _ => null
        };
    }
}
