// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Model;

namespace Microsoft.Data.Entity.Migrations.Model
{
    public class MigrationOperationFactory
    {
        private readonly IRelationalMetadataExtensionProvider _extensions;
        private readonly RelationalObjectNameFactory _nameFactory;

        public MigrationOperationFactory([NotNull] RelationalObjectNameFactory nameFactory)
        {
            Check.NotNull(nameFactory, "nameFactory");

            _extensions = nameFactory.Extensions;
            _nameFactory = nameFactory;
        }

        public virtual IRelationalMetadataExtensionProvider Extensions
        {
            get { return _extensions; }
        }

        public virtual RelationalObjectNameFactory NameFactory
        {
            get { return _nameFactory; }
        }

        public virtual CreateTableOperation CreateCreateTableOperation([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            var extension = Extensions.EntityTypeExtension(entityType);

            var operation = new CreateTableOperation(extension.Table);

            foreach (var property in entityType.Properties)
            {
                operation.Columns.Add(CreateColumn(property));
            }

            var primaryKey = entityType.GetPrimaryKey();
            if (primaryKey != null)
            {
                operation.PrimaryKey = CreateAddPrimaryKeyOperation(primaryKey);
            }

            foreach (var uniqueConstraint in entityType.Keys.Except(new[] { primaryKey }))
            {
                operation.UniqueConstraints.Add(CreateAddUniqueConstraintOperation(uniqueConstraint));
            }

            foreach (var foreignKey in entityType.ForeignKeys)
            {
                operation.ForeignKeys.Add(CreateAddForeignKeyOperation(foreignKey));
            }

            foreach (var index in entityType.Indexes)
            {
                operation.Indexes.Add(CreateCreateIndexOperation(index));
            }

            return operation;
        }

        public virtual Column CreateColumn([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            var extension = Extensions.PropertyExtension(property);

            return
                new Column(NameFactory.ColumnName(property), property.PropertyType, extension.ColumnType)
                {
                    IsNullable = property.IsNullable,
                    DefaultValue = extension.DefaultValue,
                    DefaultSql = extension.DefaultExpression,
                    GenerateValueOnAdd = property.GenerateValueOnAdd,
                    IsComputed = property.IsStoreComputed,
                    IsTimestamp = property.PropertyType == typeof(byte[]) && property.IsConcurrencyToken,
                    MaxLength = property.MaxLength > 0 ? property.MaxLength : (int?)null
                };
        }

        public virtual AddPrimaryKeyOperation CreateAddPrimaryKeyOperation([NotNull] IKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            return new AddPrimaryKeyOperation(
                Extensions.EntityTypeExtension(primaryKey.EntityType).Table,
                NameFactory.PrimaryKeyName(primaryKey),
                primaryKey.Properties.Select(p => Extensions.PropertyExtension(p).Column).ToArray(),
                isClustered: false);
        }

        public virtual AddUniqueConstraintOperation CreateAddUniqueConstraintOperation([NotNull] IKey uniqueConstraint)
        {
            throw new NotImplementedException();
        }

        public virtual AddForeignKeyOperation CreateAddForeignKeyOperation([NotNull] IForeignKey foreignKey)
        {
            throw new NotImplementedException();
        }

        public virtual CreateIndexOperation CreateCreateIndexOperation([NotNull] IIndex index)
        {
            throw new NotImplementedException();
        }
    }
}
