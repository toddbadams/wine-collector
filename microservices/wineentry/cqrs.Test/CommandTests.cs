using System;
using cqrs.Commands;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace cqrs.Test
{
    public class CommandTests
    {
        [Fact]
        public void Ctor_Should_Throw_ArgumentException_Given_Empty_AggregateId()
        {
            // arrange 
            Action act = () => new Command(Guid.Empty, 0, CommandName.SetQuantity, new JObject());

            // assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("aggregateId cannot be empty");
        }

        [Fact]
        public void Ctor_Should_Throw_ArgumentException_Given_Negative_Sequence()
        {
            // arrange 
            Action act = () => new Command(Guid.NewGuid(), -1, CommandName.SetQuantity, new JObject());

            // assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("sequence cannot be negative");
        }

        [Fact]
        public void Ctor_Should_Throw_ArgumentNullException_Given_Null_Value()
        {
            // arrange 
            Action act = () => new Command(Guid.NewGuid(), 0, CommandName.SetQuantity, null);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Id_Should_Be_AggregateId_And_Sequence_Given_Valid_Parameters()
        {
            // arrange 
            var aggregateId = Guid.NewGuid();

            // arrange
            var command = new Command(aggregateId, 456, CommandName.SetQuantity, new JObject());

            // assert
            command.Id.Should().Be(aggregateId.ToString() + "-456");
        }

        [Fact]
        public void Value_Should_Value_Parameter_Given_Valid_Parameters()
        {
            // arrange 

            // arrange
            var command = new Command(Guid.NewGuid(), 456, CommandName.SetQuantity, JObject.Parse("{ 'prop': 'val' }"));

            // assert
            command.Value.GetValue("prop").Value<string>().Should().BeEquivalentTo("val");
        }

        [Fact]
        public void Command_Should_Serialize_Given_Valid_Parameters()
        {
            // arrange 
            var aggregateId = Guid.NewGuid();
            var expected = $"{{\"aggregateId\":\"{aggregateId}\"," +
                           $"\"id\":\"{aggregateId}-456\"," +
                           "\"name\":\"SetQuantity\",\"sequence\":456,\"value\":{\"prop\":\"val\"}}";

            var command = new Command(aggregateId, 456, CommandName.SetQuantity, JObject.Parse("{ 'prop': 'val' }"));

            // arrange
            var serializedCommand = JsonConvert.SerializeObject(command);

            // assert
            serializedCommand.Should().Be(expected);
        }
    }
}