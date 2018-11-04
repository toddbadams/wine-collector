using AutoMapper;
using cqrs.Commands;
using Tba.WineEntry.ApiModels.Create;
using Xunit;

namespace cqrs.Test.Commands
{
    public class CreateRequestConverterTest
    {
        [Fact]
        public void test()
        {
            Mapper.Initialize(_ => _.CreateMap<CreateRequest, CommandCollection>()
                .ConvertUsing(new CreateRequestConverter()));
            Mapper.AssertConfigurationIsValid();
        }
    }
}