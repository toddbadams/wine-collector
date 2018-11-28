using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Tba.WineEntry.Upsert.Application.Commands;
using Tba.WineEntry.Upsert.Presentation;
using Xunit;

namespace Tba.WineEntry.Upsert.Tests
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
