// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalObjectNameFactory
    {
        private readonly IRelationalMetadataExtensionProvider _extensions;

        public RelationalObjectNameFactory([NotNull] IRelationalMetadataExtensionProvider extensions)
        {
            Check.NotNull(extensions, "extensions");

            _extensions = extensions;
        }

        public IRelationalMetadataExtensionProvider Extensions
        {
            get { return _extensions; }
        }

        public virtual SchemaQualifiedName FullTableName(IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            var extension = Extensions.EntityTypeExtension(entityType);
            var tableName = extension.Table ?? entityType.Name;

            return new SchemaQualifiedName(tableName, extension.Schema);
        }

        public virtual string ColumnName([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return Extensions.PropertyExtension(property).Column ?? property.Name;
        }

        public virtual string PrimaryKeyName([NotNull] IKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            return
                Extensions.KeyExtension(primaryKey).Name
                ?? string.Format(
                    "PK_{0}",
                    FullTableName(primaryKey.EntityType));
        }

        public virtual string UniqueConstraintName([NotNull] IKey key)
        {
            Check.NotNull(key, "key");

            return
                Extensions.KeyExtension(key).Name
                ?? string.Format(
                    "UC_{0}_{1}",
                    FullTableName(key.EntityType),
                    string.Join("_", key.Properties.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).Select(ColumnName)));
        }

        public virtual string ForeignKeyName([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return
                Extensions.KeyExtension(foreignKey).Name
                ?? string.Format(
                    "FK_{0}_{1}_{2}",
                    FullTableName(foreignKey.EntityType),
                    FullTableName(foreignKey.ReferencedEntityType),
                    string.Join("_", foreignKey.Properties.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).Select(ColumnName)));
        }

        public virtual string IndexName([NotNull] IIndex index)
        {
            Check.NotNull(index, "index");

            return Extensions.IndexExtension(index).Name 
                ?? string.Format(
                    "IX_{0}_{1}",
                    FullTableName(index.EntityType),
                    string.Join("_", index.Properties.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).Select(ColumnName)));
        }
    }
}
