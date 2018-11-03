using System;
using cqrs.Commands;
using FluentAssertions;
using Xunit;

namespace cqrs.Test
{
    public class SimpleCommandValueTests
    {

        [Fact]
        public void Ctor_Should_Throw_ArgumentNullException_Given_Null_Value()
        {
            // arrange 
            Action act = () => new SimpleCommandValue<string>(null);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Value_Should_Value_Parameter_Given_Valid_Parameters()
        {
            // arrange & act
            var command = new SimpleCommandValue<string>("val");

            // assert
            command.Value.Should().BeEquivalentTo("val");
        }
    }
}