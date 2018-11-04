using AutoMapper;
using cqrs.Commands;
using Tba.WineEntry.ApiModels.Update;
using Xunit;

namespace cqrs.Test.Commands
{
    public class UpdateRequestConverterTest
    {
        [Fact]
        public void test()
        {
            Mapper.Initialize(_ => _.CreateMap<UpdateRequest, CommandCollection>()
                .ConvertUsing(new UpdateRequestConverter()));
            Mapper.AssertConfigurationIsValid();
        }
    }
}