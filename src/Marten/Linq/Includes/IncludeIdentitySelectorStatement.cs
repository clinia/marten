using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Marten.Internal;
using Marten.Linq.Filters;
using Marten.Linq.Parsing;
using Marten.Linq.QueryHandlers;
using Marten.Linq.Selectors;
using Marten.Linq.SqlGeneration;
using Marten.Util;

namespace Marten.Linq.Includes
{

    // This is also used as an ISelectClause inside the statement structure
    public class IncludeIdentitySelectorStatement : Statement, ISelectClause
    {
        private readonly IList<IIncludePlan> _includes;
        private readonly SelectorStatement _innerEnd;
        private readonly SelectorStatement _clonedEnd;

        public IncludeIdentitySelectorStatement(Statement original, IList<IIncludePlan> includes,
            IMartenSession session) : base(null)
        {
            ExportName = session.NextTempTableName();

            _includes = includes;

            _innerEnd = (SelectorStatement)original.Current();
            FromObject = _innerEnd.SelectClause.FromObject;

            _clonedEnd = _innerEnd.UseAsEndOfTempTableAndClone(this);
            _clonedEnd.SingleValue = _innerEnd.SingleValue;

            Inner = original;

            Offset = original.Offset;
            Limit = original.Limit;

            Statement current = this;
            foreach (var include in includes)
            {
                var includeStatement = include.BuildStatement(ExportName);

                current.InsertAfter(includeStatement);
                current = includeStatement;
            }

            current.InsertAfter(_clonedEnd);
        }

        public override void CompileLocal(IMartenSession session)
        {
            Inner.CompileStructure(session);
            Inner = Inner.Top();
            Where = Inner.Where;
        }

        public Type SelectedType => typeof(void);

        public Statement Inner { get; private set; }

        protected override void configure(CommandBuilder sql)
        {
            sql.Append("create temp table ");
            sql.Append(ExportName);
            sql.Append(" as (\n");
            Inner.Configure(sql);
            sql.Append("\n);");
        }

        public bool IncludeDataInTempTable { get; set; }

        public void WriteSelectClause(CommandBuilder sql)
        {
            if (IncludeDataInTempTable)
            {
                // Basically if the data for the Include is coming from a
                // SelectMany() clause
                sql.Append("select data, ");
            }
            else
            {
                sql.Append($"select d.id, ");
            }

            sql.Append(_includes.Select(x => x.TempTableSelector).Join(", "));
            sql.Append($"\nfrom (select * from {FromObject} d ");
            sql.Append("where ");
            Where.Apply(sql);
            if (Offset > 0 || Limit > 0)
            {
                var offsetParam = sql.AddParameter(Offset);
                var limitParam = sql.AddParameter(Limit);
                sql.Append(" OFFSET :");
                sql.Append(offsetParam.ParameterName);
                sql.Append(" LIMIT :");
                sql.Append(limitParam.ParameterName);
            }
            sql.Append(") d\n");
            sql.Append(_includes.Select(x => x.LeftJoinExpression).Join("\n"));
        }

        public string[] SelectFields()
        {
            return _includes.Select(x => x.TempTableSelector).ToArray();
        }

        public ISelector BuildSelector(IMartenSession session)
        {
            throw new System.NotSupportedException();
        }

        public IQueryHandler<T> BuildHandler<T>(IMartenSession session, Statement topStatement,
            Statement currentStatement)
        {
            // It's wrapped in LinqHandlerBuilder
            return _clonedEnd.SelectClause.BuildHandler<T>(session, topStatement, currentStatement);
        }

        public ISelectClause UseStatistics(QueryStatistics statistics)
        {
            throw new System.NotSupportedException();
        }
    }
}
