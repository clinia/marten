using System;
using System.Collections.Generic;
using System.Diagnostics;
using CliniaPOC.Models;
using Marten.Linq;

namespace CliniaPOC.Infrastructure
{
    public class HealthFacilityIncludeMapper: IncludeMapper<HealthFacility>
    {
        public HealthFacilityIncludeMapper(IMartenQueryable<HealthFacility> queryable): base(queryable)
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
                "practices" => queryable.ThenIncludeInverted<HealthFacility, Practice, string>(x => x.HealthFacility, mapper.Practices),
                "healthServices" => queryable.ThenIncludeInverted<HealthFacility, HealthService, string>(x => x.HealthFacilityId, mapper.HealthServices),
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
