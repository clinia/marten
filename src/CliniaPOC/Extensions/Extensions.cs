using System;
using System.Collections.Generic;
using CliniaPOC.Models;

namespace CliniaPOC.Extensions
{
    public static class Extensions
    {
        // We don't need to implement if this is only used to build SQL queries.
        public static bool HasSpeciality(this HealthService healthService, string specialityId)
        {
            return false;
        }

        // We don't need to implement if this is only used to build SQL queries.
        public static bool EqualsString(this Entity entity, string[] path, string value)
        {
            return false;
        }
    }
}
