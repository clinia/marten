using System.Linq.Expressions;
using Marten;
using Marten.Linq.Fields;
using Marten.Linq.Filters;
using Marten.Linq.Parsing;
using Marten.Linq.SqlGeneration;

namespace CliniaPOC.Extensions
{
    public class HealthServiceBySpecialityExtension : IMethodCallParser
    {
        public bool Matches(MethodCallExpression expression)
        {
            return expression.Method.Name == nameof(Extensions.HasSpeciality);
        }

        public ISqlFragment Parse(IFieldMapping mapping, ISerializer serializer, MethodCallExpression expression)
        {
            return new WhereFragment($"d.data -> 'Values' ->> 'SpecialityId' = '{expression.Arguments[1].Value()}'");
        }
    }
}
