// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Model
{
    public class CreateTableOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly IList<ColumnModel> _columns = new List<ColumnModel>();
        private AddPrimaryKeyOperation _primaryKey;
        private readonly IList<AddUniqueConstraintOperation> _uniqueConstraints = new List<AddUniqueConstraintOperation>();
        private readonly IList<AddForeignKeyOperation> _foreignKeys = new List<AddForeignKeyOperation>();
        private readonly IList<CreateIndexOperation> _indexes = new List<CreateIndexOperation>();

        public CreateTableOperation(SchemaQualifiedName tableName)
        {
            _tableName = tableName;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual IList<ColumnModel> Columns
        {
            get { return _columns; }
        }

        public virtual AddPrimaryKeyOperation PrimaryKey
        {
            get { return _primaryKey; }
            set { _primaryKey = value; }
        }

        public virtual IList<AddUniqueConstraintOperation> UniqueConstraints
        {
            get { return _uniqueConstraints; }
        }

        public virtual IList<AddForeignKeyOperation> ForeignKeys
        {
            get { return _foreignKeys; }
        }

        public virtual IList<CreateIndexOperation> Indexes
        {
            get { return _indexes; }
        }

        public override void Accept<TVisitor, TContext>(TVisitor visitor, TContext context)
        {
            Check.NotNull(visitor, "visitor");
            Check.NotNull(context, "context");

            visitor.Visit(this, context);
        }

        public override void GenerateSql(MigrationOperationSqlGenerator generator, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(stringBuilder, "stringBuilder");

            generator.Generate(this, stringBuilder);
        }

        public override void GenerateCode(MigrationCodeGenerator generator, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(stringBuilder, "stringBuilder");

            generator.Generate(this, stringBuilder);
        }
    }
}
