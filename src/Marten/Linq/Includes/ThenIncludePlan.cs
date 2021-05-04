using System;
using System.Text.RegularExpressions;
using Marten.Internal.Storage;
using Marten.Linq.Fields;

namespace Marten.Linq.Includes
{
    public class ThenIncludePlan<T> : IncludePlan<T>
    {
        public ThenIncludePlan(IDocumentStorage<T> storage, IField connectingField, Action<T> callback) : base(storage, connectingField, callback)
        {
        }

        public override int Index
        {
            set
            {
                IdAlias = "id" + (value + 1);
                ExpressionName = "include"+ (value + 1);
                ChainExpressionName = $"include{value}";

                TempTableSelector = $"{ExpressionName}.id as {IdAlias}";
            }
        }

        public override string LeftJoinExpression => $"LEFT JOIN {FromObject} {ExpressionName} ON {Regex.Replace(ConnectingField.RawLocator, "^d.", $"{ChainExpressionName}.")} = {ExpressionName}.id";

        public string ChainExpressionName { get; private set; }
    }
}
