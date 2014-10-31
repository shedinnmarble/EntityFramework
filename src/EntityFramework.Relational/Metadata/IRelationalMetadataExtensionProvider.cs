// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public interface IRelationalMetadataExtensionProvider
    {
        IRelationalModelExtensions ModelExtension([NotNull] IModel model);
        IRelationalEntityTypeExtensions EntityTypeExtension([NotNull] IEntityType entityType);
        IRelationalPropertyExtensions PropertyExtension([NotNull] IProperty property);
        IRelationalKeyExtensions KeyExtension([NotNull] IKey key);
        IRelationalForeignKeyExtensions ForeignKeyExtension([NotNull] IForeignKey foreignKey);
        IRelationalIndexExtensions IndexExtension([NotNull] IIndex index);
    }
}
