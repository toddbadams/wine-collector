using System;
using System.Collections;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Tba.WineEntry.Application.Commands;
using Xunit;

namespace cqrs.Test
{
    public class CommandCollectionTests
    {

        [Fact]
        public void Ctor_Should_Throw_ArgumentException_Given_Negative_Sequence()
        {
            // arrange 
            Action act = () => new CommandCollection(-1);

            // assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("sequence cannot be negative");
        }

        [Fact]
        public void Commands_Should_Be_Empty_Given_None_Added()
        {
            // arrange 
            var commands = new CommandCollection(4);

            // act

            // assert
            commands.Should().HaveCount(0);
        }

        [Fact]
        public void Sequence_Should_Be_Parameter_Given_Zero_Or_Positive_Sequence()
        {
            // arrange 
            var commands = new CommandCollection(4);

            // act
            commands.Add(Guid.NewGuid(), CommandName.SetAcquiredOn, new JObject());

            // assert
            commands.First().Sequence.Should().Be(4);
        }

        [Fact]
        public void Commands_Should_Enumerate_Given_Added_Commands()
        {
            // arrange 
            var commands = new CommandCollection(4).Add(Guid.NewGuid(), CommandName.SetAcquiredOn, new JObject());

            // act & assert
            ((IEnumerable)commands)
                .GetEnumerator()
                .MoveNext()
                .Should().BeTrue();
        }
    }
}