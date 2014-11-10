// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Update
{
    public class BatchExecutorTest
    {
        [Fact]
        public async Task ExecuteAsync_calls_Commit_if_no_transaction()
        {
            var mockModificationCommandBatch = new Mock<ModificationCommandBatch>();

            var transactionMock = new Mock<RelationalTransaction>();
            var mockRelationalConnection = new Mock<RelationalConnection>();

            RelationalTransaction currentTransaction = null;
            mockRelationalConnection.Setup(m => m.BeginTransaction()).Returns(() => currentTransaction = transactionMock.Object);
            mockRelationalConnection.Setup(m => m.Transaction).Returns(() => currentTransaction);

            var cancellationToken = new CancellationTokenSource().Token;

            var relationalTypeMapper = new RelationalTypeMapper();
            var batchExecutor = new BatchExecutorForTest(relationalTypeMapper);

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.OpenAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close());
            transactionMock.Verify(t => t.Commit());
            mockModificationCommandBatch.Verify(mcb => mcb.ExecuteAsync(
                It.IsAny<RelationalTransaction>(),
                relationalTypeMapper,
                It.IsAny<DbContext>(),
                null,
                cancellationToken));
        }

        [Fact]
        public async Task ExecuteAsync_does_not_call_Commit_if_existing_transaction()
        {
            var mockModificationCommandBatch = new Mock<ModificationCommandBatch>();

            var transactionMock = new Mock<RelationalTransaction>();
            var mockRelationalConnection = new Mock<RelationalConnection>();
            mockRelationalConnection.Setup(m => m.Transaction).Returns(transactionMock.Object);

            var cancellationToken = new CancellationTokenSource().Token;

            var relationalTypeMapper = new RelationalTypeMapper();
            var batchExecutor = new BatchExecutorForTest(relationalTypeMapper);

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.OpenAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close());
            mockRelationalConnection.Verify(rc => rc.BeginTransaction(), Times.Never);
            transactionMock.Verify(t => t.Commit(), Times.Never);
            mockModificationCommandBatch.Verify(mcb => mcb.ExecuteAsync(
                It.IsAny<RelationalTransaction>(),
                relationalTypeMapper,
                It.IsAny<DbContext>(),
                null,
                cancellationToken));
        }

        private class BatchExecutorForTest : BatchExecutor
        {
            public BatchExecutorForTest(RelationalTypeMapper typeMapper)
                : base(typeMapper, new LazyRef<DbContext>(() => null), new LoggerFactory())
            {
            }

            protected override ILogger Logger
            {
                get { return null; }
            }
        }
    }
}
