using AutoMapper;
using Tba.WineEntry.Application.Commands;
using Tba.WineEntry.Application.Models.Create;
using Xunit;

namespace cqrs.Test.Commands
{
    public class CreateRequestConverterTest
    {
        [Fact]
        public void test()
        {
            Mapper.Initialize(_ => _.CreateMap<CreateWineEntryRequest, CommandCollection>()
                .ConvertUsing(new CreateRequestConverter()));
            Mapper.AssertConfigurationIsValid();
        }
    }
}