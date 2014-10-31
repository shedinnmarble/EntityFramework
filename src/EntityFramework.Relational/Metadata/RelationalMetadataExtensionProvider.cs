// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalMetadataExtensionProvider : IRelationalMetadataExtensionProvider
    {
        public virtual IRelationalModelExtensions ModelExtension(IModel model)
        {
            return model.Relational();
        }

        public virtual IRelationalEntityTypeExtensions EntityTypeExtension(IEntityType entityType)
        {
            return entityType.Relational();
        }

        public virtual IRelationalPropertyExtensions PropertyExtension(IProperty property)
        {
            return property.Relational();
        }

        public virtual IRelationalKeyExtensions KeyExtension(IKey key)
        {
            return key.Relational();
        }

        public virtual IRelationalForeignKeyExtensions ForeignKeyExtension(IForeignKey foreignKey)
        {
            return foreignKey.Relational();
        }

        public virtual IRelationalIndexExtensions IndexExtension(IIndex index)
        {
            return index.Relational();
        }
    }
}
