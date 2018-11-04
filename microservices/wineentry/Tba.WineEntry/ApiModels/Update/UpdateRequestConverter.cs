using System;
using System.Collections.Generic;
using AutoMapper;
using cqrs.Commands;
using Newtonsoft.Json.Linq;

namespace Tba.WineEntry.ApiModels.Update
{
    public class UpdateRequestConverter : ITypeConverter<UpdateRequest, CommandCollection>
    {
        private readonly IDictionary<CommandName, Func<object, JObject>> _dictionary;

        public UpdateRequestConverter()
        {
            _dictionary = new Dictionary<CommandName, Func<object, JObject>>
            {
                {CommandName.SetAcquiredOn, _ =>new JObject { ["value"] = (DateTimeOffset)_ } },
                {CommandName.SetBottleSize, _ =>new JObject { ["value"] = (int)_ } },
                {CommandName.SetBottlesPerCase, _ =>new JObject { ["value"] = (int)_ } },
                {CommandName.SetQuantity, _ =>new JObject { ["value"] = (int)_ } },
                {CommandName.SetCostPerBottle, _ =>new JObject { ["value"] = (decimal)_ } },
                {CommandName.SetCellar, _ => new JObject(_) },
                {CommandName.SetWine, _ => new JObject(_) }
            };
        }

        public CommandCollection Convert(UpdateRequest source, CommandCollection destination, ResolutionContext context)
        {
            var commands = new CommandCollection(source.Version);
            foreach (var op in source.Operations)
            {
                var cmd = Enum.Parse<CommandName>(op.Operation);
                if (_dictionary.ContainsKey(cmd))
                {
                    commands.Add(source.WineEntryId, cmd, op.Value);
                }
            }
            return commands;
        }
    }
}
