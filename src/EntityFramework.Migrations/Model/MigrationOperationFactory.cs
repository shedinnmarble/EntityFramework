// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.Migrations.Model
{
    public class MigrationOperationFactory
    {
        private readonly IRelationalMetadataExtensionProvider _extensionProvider;

        public MigrationOperationFactory([NotNull] IRelationalMetadataExtensionProvider extensionProvider)
        {
            Check.NotNull(extensionProvider, "extensionProvider");

            _extensionProvider = extensionProvider;
        }

        public virtual IRelationalMetadataExtensionProvider ExtensionProvider
        {
            get { return _extensionProvider; }
        }

        public virtual CreateTableOperation CreateCreateTableOperation([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            var entityTypeExtensions = ExtensionProvider.GetEntityTypeExtensions(entityType);

            var operation = new CreateTableOperation(entityTypeExtensions.Table);

            foreach (var property in entityType.Properties)
            {
                operation.Columns.Add(CreateColumnModel(property));
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

        public virtual ColumnModel CreateColumnModel([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return new ColumnModel();
        }

        public virtual AddPrimaryKeyOperation CreateAddPrimaryKeyOperation([NotNull] IKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            return new AddPrimaryKeyOperation(
                ExtensionProvider.GetEntityTypeExtensions(primaryKey.EntityType).Table,
                ExtensionProvider.GetKeyExtensions(primaryKey).Name,
                primaryKey.Properties.Select(p => ExtensionProvider.GetPropertyExtensions(p).Column).ToArray(),
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
