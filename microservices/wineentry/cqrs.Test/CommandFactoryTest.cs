using AutoMapper;
using cqrs.ApiModels;
using cqrs.Commands;
using Xunit;

namespace cqrs.Test
{
    public class WineEntryCreateRequestConverterTest
    {
        [Fact]
        public void test()
        {
            Mapper.Initialize(_ => _.CreateMap<WineEntryCreateRequest, CommandCollection>()
                .ConvertUsing(new WineEntryCreateRequestConverter()));
            Mapper.AssertConfigurationIsValid();
        }
    }
}