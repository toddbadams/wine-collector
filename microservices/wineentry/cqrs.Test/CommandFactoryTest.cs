using AutoMapper;
using cqrs.ApiModels;
using cqrs.Commands;
using Xunit;

namespace cqrs.Test
{
    public class CommandFactoryTest
    {
        //private readonly ICommandFactory _commandFactory = new CommandFactory();

        //[Fact]
        //public void Create_Should_Create_Sequenced_Commands_Given_Valid_Request()
        //{
        //    // arrange & act
        //    var commands = _commandFactory.Create(ValidWineEntryCreateRequest()).ToArray();

        //    // assert
        //    for (var i = 0; i < commands.Length; i++)
        //    {
        //        commands.ElementAt(i).Sequence.Should().Be(i++);
        //    }
        //}

        //[Theory]
        //[InlineData("SetAcquiredOn")]
        //[InlineData("SetBottleSize")]
        //[InlineData("SetBottlesPerCase")]
        //[InlineData("SetCostPerBottle")]
        //[InlineData("SetQuantity")]
        //[InlineData("SetCellar")]
        //[InlineData("SetWine")]
        //public void Create_Should_Create_Valid_Command_Given_Valid_Request(string expectedName)
        //{
        //    // arrange & act
        //    var commands = _commandFactory.Create(ValidWineEntryCreateRequest()).ToArray();

        //    // assert
        //    commands.Where(_ => _.Name == expectedName).Should().HaveCount(1);
        //}

        //[Fact]
        //public void Create_Should_Create_Commands_With_Same_AggregateId_Given_Valid_Request()
        //{
        //    // arrange & act
        //    var commands = _commandFactory.Create(ValidWineEntryCreateRequest()).ToArray();

        //    // assert
        //    var expected = commands.First().AggregateId;
        //    foreach (var command in commands)
        //    {
        //        command.AggregateId.Should().Be(expected);
        //    }
        //}

        //private static WineEntryCreateRequest ValidWineEntryCreateRequest()
        //{
        //    return new WineEntryCreateRequest
        //    {
        //        AcquiredOn = DateTimeOffset.Now,
        //        BottleSize = BottleSize.DoubleMagnum,
        //        BottlesPerCase = BottlesPerCase.One,
        //        Cellar = new KeyValuePair<Guid, string>(Guid.NewGuid(), "home"),
        //        CostPerBottle = (decimal)40.50,
        //        Quantity = 1,
        //        Wine = new KeyValuePair<Guid, string>(Guid.NewGuid(), "2011 Sine Qua Non The Moment")
        //    };
        //}

        [Fact]
        public void test()
        {
            Mapper.Initialize(_ => _.CreateMap<WineEntryCreateRequest, CommandCollection>()
                .ConvertUsing(new WineEntryCreateRequestConverter()));
            Mapper.AssertConfigurationIsValid();
        }
    }
}