using System;
using CliniaPOC.Models;

namespace CliniaPOC.Extensions
{
    public static class Extensions
    {
        public static bool HasSpeciality(this HealthService healthService, string specialityId)
        {
            return (healthService.Values["Speciality"] as string)?.Equals(specialityId, StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}
