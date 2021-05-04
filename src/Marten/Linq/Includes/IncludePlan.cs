using System;
using Marten.Internal;
using Marten.Internal.Storage;
using Marten.Linq.Fields;
using Marten.Linq.Selectors;
using Marten.Linq.SqlGeneration;
using Marten.Util;

namespace Marten.Linq.Includes
{
    public class IncludePlan<T> : IIncludePlan
    {
        protected readonly IDocumentStorage<T> Storage;
        protected readonly Action<T> Callback;

        public IncludePlan(IDocumentStorage<T> storage, IField connectingField, Action<T> callback)
        {
            Storage = storage;
            ConnectingField = connectingField;
            Callback = callback;
        }

        public IField ConnectingField { get; }

        public virtual int Index
        {
            set
            {
                IdAlias = "id" + (value + 1);
                ExpressionName = "include"+ (value + 1);

                TempTableSelector = $"{ExpressionName}.id as {IdAlias}";
            }
        }

        public bool RequiresLateralJoin()
        {
            // TODO -- dont' think this is permanent. Or definitely shouldn't be
            return ConnectingField is ArrayField;
        }

        public virtual string LeftJoinExpression => $"LEFT JOIN {FromObject} {ExpressionName} ON {ConnectingField.RawLocator} = {ExpressionName}.id";

        public string ExpressionName { get; protected set; }

        public string FromObject => Storage.FromObject;

        public string IdAlias { get; protected set; }
        public string TempTableSelector { get; protected set; }

        public Statement BuildStatement(string tempTableName)
        {
            return new IncludedDocumentStatement(Storage, this, tempTableName);
        }

        public IIncludeReader BuildReader(IMartenSession session)
        {
            var selector = (ISelector<T>) Storage.BuildSelector(session);
            return new IncludeReader<T>(Callback, selector);
        }

        public class IncludedDocumentStatement : SelectorStatement
        {
            public IncludedDocumentStatement(IDocumentStorage<T> storage, IncludePlan<T> includePlan,
                string tempTableName) : base(storage, storage.Fields)
            {
                var initial = new InTempTableWhereFragment(tempTableName, includePlan.IdAlias);
                Where = storage.FilterDocuments(null, initial);
            }

            protected override void configure(CommandBuilder sql)
            {
                base.configure(sql);
                sql.Append(";\n");
            }
        }
    }


}
