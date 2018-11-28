using System;
using System.Collections.Generic;
using AutoMapper;
using Newtonsoft.Json.Linq;

namespace Tba.WineEntry.Upsert.Application.Commands.Update
{
    public class UpdateRequestConverter : ITypeConverter<UpdateRequest, CommandCollection>
    {
        private readonly IDictionary<EventName, Func<object, JObject>> _dictionary;

        public UpdateRequestConverter()
        {
            _dictionary = new Dictionary<EventName, Func<object, JObject>>
            {
                {EventName.AcquiredOnChanged, _ =>new JObject { ["value"] = (DateTimeOffset)_ } },
                {EventName.BottleSizeChanged, _ =>new JObject { ["value"] = (int)_ } },
                {EventName.BottlesPerCaseChanged, _ =>new JObject { ["value"] = (int)_ } },
                {EventName.QuantityChanged, _ =>new JObject { ["value"] = (int)_ } },
                {EventName.CostPerBottleChanged, _ =>new JObject { ["value"] = (decimal)_ } },
                {EventName.CellarChanged, _ => new JObject(_) },
                {EventName.WineChanged, _ => new JObject(_) }
            };
        }

        public CommandCollection Convert(UpdateRequest source, CommandCollection destination, ResolutionContext context)
        {
            var commands = new CommandCollection(source.Version);
            foreach (var op in source.Operations)
            {
                if (!Enum.TryParse<EventName>(op.Operation, out var cmd))
                {
                    throw new ArgumentException($"Invalid operation {op.Operation}");
                }
                if (_dictionary.ContainsKey(cmd))
                {
                    commands.Add(source.WineEntryId, cmd, op.Value);
                }
            }
            return commands;
        }
    }
}
