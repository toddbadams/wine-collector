using System;
using System.Collections.Generic;
using System.Text;
using cqrs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Tba.WineEntry.Application.Commands;
using Xunit;

namespace Tba.WineEntry.Tests
{
    public class CreateWineEntryTests
    {
        private readonly IMock<IAsyncCollector<Command>> _commandCollectorMock;
        private readonly IMock<ILogger> _loggerMock;

        public CreateWineEntryTests()
        {
            _commandCollectorMock = new Mock<IAsyncCollector<Command>>();
            _loggerMock = new Mock<ILogger>();
        }

            [Fact]
        public void foo()
        {
            // arrange

            // act
            var result = CreateWineEntry.Run(null, _commandCollectorMock.Object, _loggerMock.Object);
        }
    }
}
