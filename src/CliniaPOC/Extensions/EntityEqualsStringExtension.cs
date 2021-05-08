using System.Linq.Expressions;
using System.Text;
using Marten;
using Marten.Linq.Fields;
using Marten.Linq.Filters;
using Marten.Linq.Parsing;
using Marten.Linq.SqlGeneration;

namespace CliniaPOC.Extensions
{
    public class EntityEqualsStringExtension: IMethodCallParser
    {
        public bool Matches(MethodCallExpression expression)
        {
            return expression.Method.Name == nameof(Extensions.EqualsString);
        }

        public ISqlFragment Parse(IFieldMapping mapping, ISerializer serializer, MethodCallExpression expression)
        {
            var path = expression.Arguments[1].Value() as string[];
            var value = expression.Arguments[2].Value() as string;

            var queryBuilder = new StringBuilder("d.data -> 'Values'");
            for(var i = 0; i < path.Length - 1; i ++)
            {
                queryBuilder.Append($" -> '{path[i]}'");
            }

            queryBuilder.Append($" ->> '{path[^1]}' = '{value}'");
            return new WhereFragment(queryBuilder.ToString());
        }
    }
}
