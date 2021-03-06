﻿using System;
using AutoMapper;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Tba.WineEntry.Upsert.Application.Commands;
using Tba.WineEntry.Upsert.Application.Commands.Update;
using Xunit;

namespace Tba.WineEntry.Upsert.Tests.Commands
{
    public class UpdateRequestConverterTest
    {
        private readonly IMapper _mapper;

        public UpdateRequestConverterTest()
        {
            _mapper = new MapperConfiguration(_ => _.CreateMap<UpdateRequest, CommandCollection>()
                .ConvertUsing(new UpdateRequestConverter()))
                .CreateMapper();
        }

        [Fact]
        public void Map_Should_Throw_ArgumentException_Given_Invalid_Operation()
        {
            // arrange 
            var req = new UpdateRequest
            {
                Operations = new[]
                {
                    new UpdateOperationRequest()
                    {
                        Operation = "garbage",
                        Value = new JObject()
                    }
                }
            };

            // act
            Action act = () => _mapper.Map<MapperConfiguration>(req);

            // assert
            act.Should().Throw<ArgumentException>().WithMessage("Invalid operation garbage");
        }
    }
}