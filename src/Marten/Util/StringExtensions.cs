using System;
using Baseline;
using Newtonsoft.Json.Serialization;
using Npgsql;
#nullable enable
namespace Marten.Util
{
    public static class StringExtensionMethods
    {
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string UseParameter(this string text, NpgsqlParameter parameter)
        {
            return text.ReplaceFirst("?", ":" + parameter.ParameterName);
        }

        private static readonly SnakeCaseNamingStrategy _snakeCaseNamingStrategy = new SnakeCaseNamingStrategy();

        public static string ToSnakeCase(this string s)
        {
            return _snakeCaseNamingStrategy.GetPropertyName(s, false);
        }

        public static string FormatCase(this string s, Casing casing)
        {
            switch (casing)
            {
                case Casing.CamelCase:
                    return s.ToCamelCase();

                case Casing.SnakeCase:
                    return s.ToSnakeCase();

                default:
                    return s;
            }
        }
    }
}
