using System;
using Marten.Schema.Identity;
using Marten.Storage;

namespace Marten.Schema
{
    internal interface IDocumentMapping
    {
        IDocumentMapping Root { get; }

        Type DocumentType { get; }

        Type IdType { get; }
        DbObjectName TableName { get; }
    }

}
