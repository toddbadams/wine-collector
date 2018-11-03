using System;
using System.Collections.Generic;
using AutoMapper;
using cqrs.Commands;
using Newtonsoft.Json.Linq;

namespace cqrs.ApiModels
{
    public class WineEntryCreateRequestConverter : ITypeConverter<WineEntryCreateRequest, CommandCollection>
    {
        public CommandCollection Convert(WineEntryCreateRequest source, CommandCollection destination, ResolutionContext context)
        {
            var wineEntryId = Guid.NewGuid();
            return new CommandCollection()
                .Add(wineEntryId, CommandName.SetAcquiredOn, ToJObject(source.AcquiredOn))
                .Add(wineEntryId, CommandName.SetBottleSize, ToJObject((int)source.BottleSize))
                .Add(wineEntryId, CommandName.SetBottlesPerCase, ToJObject((int)source.BottlesPerCase))
                .Add(wineEntryId, CommandName.SetCostPerBottle, ToJObject(source.CostPerBottle))
                .Add(wineEntryId, CommandName.SetQuantity, ToJObject(source.Quantity))
                .Add(wineEntryId, CommandName.SetCellar, ToJObject(source.Cellar))
                .Add(wineEntryId, CommandName.SetWine, ToJObject(source.Cellar));
        }


        private static JObject ToJObject(DateTimeOffset value) => new JObject { ["value"] = value };

        private static JObject ToJObject(int value) => new JObject { ["value"] = value };

        private static JObject ToJObject(decimal value) => new JObject { ["value"] = value };

        private static JObject ToJObject(KeyValuePair<Guid, string> _) =>
            new JObject { ["key"] = _.Key, ["value"] = _.Value };
    }
}