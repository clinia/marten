using System;
using System.Text.RegularExpressions;
using Marten.Internal.Storage;
using Marten.Linq.Fields;

namespace Marten.Linq.Includes
{
    public class InvertedIncludePlan<T> : IncludePlan<T>
    {
        public InvertedIncludePlan(IDocumentStorage<T> storage, IField connectingField, Action<T> callback) : base(storage, connectingField, callback)
        {
        }

        public override string LeftJoinExpression => $"LEFT JOIN {FromObject} {ExpressionName} ON {Regex.Replace(ConnectingField.RawLocator, "^d.", $"{ExpressionName}.")} = d.id";
    }
}
