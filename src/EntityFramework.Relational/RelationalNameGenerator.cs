// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalNameGenerator
    {
        private readonly IRelationalMetadataExtensionProvider _extensionProvider;

        public RelationalNameGenerator([NotNull] IRelationalMetadataExtensionProvider extensionProvider)
        {
            Check.NotNull(extensionProvider, "extensionProvider");

            _extensionProvider = extensionProvider;
        }

        public virtual SchemaQualifiedName SchemaQualifiedSequenceName([NotNull] ISequence sequence)
        {
            Check.NotNull(sequence, "sequence");

            return new SchemaQualifiedName(sequence.Name, sequence.Schema);
        }

        public virtual SchemaQualifiedName SchemaQualifiedTableName([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            var extensions = _extensionProvider.Extensions(entityType);

            return new SchemaQualifiedName(extensions.Table, extensions.Schema);
        }

        public virtual string KeyName([NotNull] IKey key)
        {
            Check.NotNull(key, "key");

            return
                _extensionProvider.Extensions(key).Name
                ?? (key.EntityType.GetPrimaryKey() == key
                    ? string.Format("PK_{0}",
                        SchemaQualifiedTableName(key.EntityType))
                    : string.Format("UC_{0}_{1}",
                        SchemaQualifiedTableName(key.EntityType),
                        ColumnNames(key.Properties)));
        }

        public virtual string ForeignKeyName([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return
                _extensionProvider.Extensions(foreignKey).Name
                ?? string.Format(
                    "FK_{0}_{1}_{2}",
                    SchemaQualifiedTableName(foreignKey.EntityType),
                    SchemaQualifiedTableName(foreignKey.ReferencedEntityType),
                    ColumnNames(foreignKey.Properties));
        }

        public virtual string IndexName([NotNull] IIndex index)
        {
            Check.NotNull(index, "index");

            return
                _extensionProvider.Extensions(index).Name
                ?? string.Format(
                    "IX_{0}_{1}",
                    SchemaQualifiedTableName(index.EntityType),
                    ColumnNames(index.Properties));
        }

        private string ColumnNames(IEnumerable<IProperty> properties)
        {
            return string.Join("_",
                properties.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(p => _extensionProvider.Extensions(p).Column));
        }
    }
}
