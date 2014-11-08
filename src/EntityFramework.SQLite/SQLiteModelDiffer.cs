// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteModelDiffer : ModelDiffer
    {
        public SQLiteModelDiffer([NotNull] MigrationOperationFactory operationFactory)
            : base(operationFactory)
        {
        }

        public SQLiteModelDiffer([NotNull] SQLiteDatabaseBuilder databaseBuilder)
            : base(databaseBuilder)
        {
        }

        public new virtual SQLiteTypeMapper TypeMapper
        {
            get { return (SQLiteTypeMapper)base.TypeMapper; }
        }

        public override IReadOnlyList<MigrationOperation> Diff(IModel source, IModel target)
        {
            return new SQLiteMigrationOperationPreProcessor(NameGenerator, TypeMapper).Process(
                base.Diff(source, target), source, target).ToList();
        }
    }
}
