// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Model;

namespace Microsoft.Data.Entity.Migrations
{
    public class MigrationOperationFactory
    {
        private readonly IRelationalMetadataExtensionProvider _extensionProvider;
        private readonly RelationalNameGenerator _nameGenerator;

        public MigrationOperationFactory(
            [NotNull] IRelationalMetadataExtensionProvider extensionProvider,
            [NotNull] RelationalNameGenerator nameGenerator)
        {
            Check.NotNull(extensionProvider, "extensionProvider");
            Check.NotNull(nameGenerator, "nameGenerator");

            _extensionProvider = extensionProvider;
            _nameGenerator = nameGenerator;
        }

        public virtual CreateSequenceOperation CreateSequenceOperation([NotNull] ISequence sequence)
        {
            Check.NotNull(sequence, "sequence");

            return 
                new CreateSequenceOperation(
                    _nameGenerator.SchemaQualifiedSequenceName(sequence),
                    sequence.StartValue, 
                    sequence.IncrementBy, 
                    sequence.MinValue, 
                    sequence.MaxValue, 
                    sequence.Type);
        }

        public virtual MoveSequenceOperation MoveSequenceOperation([NotNull] ISequence source, [NotNull] ISequence target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return 
                new MoveSequenceOperation(
                    _nameGenerator.SchemaQualifiedSequenceName(source), 
                    target.Schema);
        }

        public virtual RenameSequenceOperation RenameSequenceOperation([NotNull] ISequence source, [NotNull] ISequence target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return
                new RenameSequenceOperation(
                    new SchemaQualifiedName(source.Name, target.Schema),
                    target.Name);
        }

        public virtual DropSequenceOperation DropSequenceOperation([NotNull] ISequence sequence)
        {
            Check.NotNull(sequence, "sequence");

            return 
                new DropSequenceOperation(
                    _nameGenerator.SchemaQualifiedSequenceName(sequence));
        }

        public virtual CreateTableOperation CreateTableOperation([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            var operation = new CreateTableOperation(_nameGenerator.SchemaQualifiedTableName(entityType));

            operation.Columns.AddRange(entityType.Properties.Select(Column));

            var primaryKey = entityType.GetPrimaryKey();
            if (primaryKey != null)
            {
                operation.PrimaryKey = AddPrimaryKeyOperation(primaryKey);
            }

            operation.UniqueConstraints.AddRange(entityType.Keys.Where(key => key != primaryKey).Select(AddUniqueConstraintOperation));
            operation.ForeignKeys.AddRange(entityType.ForeignKeys.Select(AddForeignKeyOperation));
            operation.Indexes.AddRange(entityType.Indexes.Select(CreateIndexOperation));

            return operation;
        }

        public virtual MoveTableOperation MoveTableOperation([NotNull] IEntityType source, [NotNull] IEntityType target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return
                new MoveTableOperation(
                    _nameGenerator.SchemaQualifiedTableName(source),
                    _extensionProvider.Extensions(target).Schema);
        }

        public virtual RenameTableOperation RenameTableOperation([NotNull] IEntityType source, [NotNull] IEntityType target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            var sourceExtensions = _extensionProvider.Extensions(source);
            var targetExtensions = _extensionProvider.Extensions(target);
            return 
                new RenameTableOperation(
                    new SchemaQualifiedName(sourceExtensions.Table, targetExtensions.Schema), 
                    targetExtensions.Table);
        }

        public virtual DropTableOperation DropTableOperation([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return 
                new DropTableOperation(
                    _nameGenerator.SchemaQualifiedTableName(entityType));
        }

        public virtual AddColumnOperation AddColumnOperation([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return 
                new AddColumnOperation(
                    _nameGenerator.SchemaQualifiedTableName(property.EntityType), 
                    Column(property));
        }

        public virtual RenameColumnOperation RenameColumnOperation([NotNull] IProperty source, [NotNull] IProperty target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return 
                new RenameColumnOperation(
                    _nameGenerator.SchemaQualifiedTableName(source.EntityType),
                    _extensionProvider.Extensions(source).Column,
                    _extensionProvider.Extensions(target).Column);
        }

        public virtual AlterColumnOperation AlterColumnOperation([NotNull] IProperty source, [NotNull] IProperty target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return
                new AlterColumnOperation(
                    _nameGenerator.SchemaQualifiedTableName(target.EntityType),
                    Column(target),
                    isDestructiveChange: true);

            // TODO: Add functionality to determine the value of isDestructiveChange.
        }

        public virtual DropColumnOperation DropColumnOperation([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return
                new DropColumnOperation(
                    _nameGenerator.SchemaQualifiedTableName(property.EntityType),
                    _extensionProvider.Extensions(property).Column);
        }

        public virtual AddDefaultConstraintOperation AddDefaultConstraintOperation([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            var extensions = _extensionProvider.Extensions(property);

            return
                new AddDefaultConstraintOperation(
                    _nameGenerator.SchemaQualifiedTableName(property.EntityType),
                    extensions.Column,
                    extensions.DefaultValue,
                    extensions.DefaultExpression);
        }

        public virtual DropDefaultConstraintOperation DropDefaultConstraintOperation([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return
                new DropDefaultConstraintOperation(
                    _nameGenerator.SchemaQualifiedTableName(property.EntityType),
                    _extensionProvider.Extensions(property).Column);
        }

        public virtual AddPrimaryKeyOperation AddPrimaryKeyOperation([NotNull] IKey key)
        {
            Check.NotNull(key, "key");

            return
                new AddPrimaryKeyOperation(
                    _nameGenerator.SchemaQualifiedTableName(key.EntityType),
                    _nameGenerator.KeyName(key),
                    key.Properties.Select(p => _extensionProvider.Extensions(p).Column).ToList(),
                    // TODO: Issue #879: Clustered is SQL Server-specific.
                    isClustered: false);
        }

        public virtual DropPrimaryKeyOperation DropPrimaryKeyOperation([NotNull] IKey key)
        {
            Check.NotNull(key, "key");

            return
                new DropPrimaryKeyOperation(
                    _nameGenerator.SchemaQualifiedTableName(key.EntityType),
                    _nameGenerator.KeyName(key));
        }

        public virtual AddUniqueConstraintOperation AddUniqueConstraintOperation([NotNull] IKey key)
        {
            Check.NotNull(key, "key");

            return
                new AddUniqueConstraintOperation(
                    _nameGenerator.SchemaQualifiedTableName(key.EntityType),
                    _nameGenerator.KeyName(key),
                    key.Properties.Select(p => _extensionProvider.Extensions(p).Column).ToList());
        }

        public virtual DropUniqueConstraintOperation DropUniqueConstraintOperation([NotNull] IKey key)
        {
            Check.NotNull(key, "key");

            return
                new DropUniqueConstraintOperation(
                    _nameGenerator.SchemaQualifiedTableName(key.EntityType),
                    _nameGenerator.KeyName(key));
        }

        public virtual AddForeignKeyOperation AddForeignKeyOperation([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return
                new AddForeignKeyOperation(
                    _nameGenerator.SchemaQualifiedTableName(foreignKey.EntityType),
                    _nameGenerator.ForeignKeyName(foreignKey),
                    foreignKey.Properties.Select(p => _extensionProvider.Extensions(p).Column).ToList(),
                    _nameGenerator.SchemaQualifiedTableName(foreignKey.ReferencedEntityType),
                    foreignKey.ReferencedProperties.Select(p => _extensionProvider.Extensions(p).Column).ToList(),
                    // TODO: Issue #333: Cascading behaviors not supported.
                    cascadeDelete: false);
        }

        public virtual DropForeignKeyOperation DropForeignKeyOperation([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return
                new DropForeignKeyOperation(
                    _nameGenerator.SchemaQualifiedTableName(foreignKey.EntityType),
                    _nameGenerator.ForeignKeyName(foreignKey));
        }

        public virtual CreateIndexOperation CreateIndexOperation([NotNull] IIndex index)
        {
            Check.NotNull(index, "index");

            return
                new CreateIndexOperation(
                    _nameGenerator.SchemaQualifiedTableName(index.EntityType),
                    _nameGenerator.IndexName(index),
                    index.Properties.Select(p => _extensionProvider.Extensions(p).Column).ToList(),
                    index.IsUnique,
                    // TODO: Issue #879: Clustered is SQL Server-specific.
                    isClustered: false);
        }

        public virtual RenameIndexOperation RenameIndexOperation([NotNull] IIndex source, [NotNull] IIndex target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return
                new RenameIndexOperation(
                    _nameGenerator.SchemaQualifiedTableName(source.EntityType),
                    _nameGenerator.IndexName(source),
                    _nameGenerator.IndexName(target));
        }

        public virtual DropIndexOperation DropIndexOperation([NotNull] IIndex index)
        {
            Check.NotNull(index, "index");

            return
                new DropIndexOperation(
                    _nameGenerator.SchemaQualifiedTableName(index.EntityType),
                    _nameGenerator.IndexName(index));
        }

        protected virtual Column Column([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            var extensions = _extensionProvider.Extensions(property);

            return
                new Column(extensions.Column, property.PropertyType, extensions.ColumnType)
                    {
                        IsNullable = property.IsNullable,
                        DefaultValue = extensions.DefaultValue,
                        DefaultSql = extensions.DefaultExpression,
                        GenerateValueOnAdd = property.GenerateValueOnAdd,
                        IsComputed = property.IsStoreComputed,
                        IsTimestamp = property.PropertyType == typeof(byte[]) && property.IsConcurrencyToken,
                        MaxLength = property.MaxLength > 0 ? property.MaxLength : (int?)null
                    };
        }
    }
}
