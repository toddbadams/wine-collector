using System;
using System.Collections.Generic;
using AutoMapper;
using cqrs.Commands;
using Newtonsoft.Json.Linq;

namespace Tba.WineEntry.ApiModels.Update
{
    public class UpdateRequestConverter : ITypeConverter<UpdateRequest, CommandCollection>
    {
        private IDictionary<CommandName, Func<object, JObject>> _dictionary;

        public UpdateRequestConverter()
        {
            _dictionary = new Dictionary<CommandName, Func<object, JObject>>
            {
                {CommandName.SetAcquiredOn, _ =>new JObject { ["value"] = (DateTimeOffset)_ } }
            };
        }
        public CommandCollection Convert(UpdateRequest source, CommandCollection destination, ResolutionContext context)
        {
            var commands = new CommandCollection(source.Version);
            foreach (var op in source.Operations)
            {
                commands.Add(source.WineEntryId, Enum.Parse<CommandName>(op.Operation), op.Value);
            }

            return commands;
        }


        private static JObject ToJObjectFromDateTimeOffset(object value) => new JObject { ["value"] = (DateTimeOffset)value };

        private static JObject ToJObject(int value) => new JObject { ["value"] = value };

        private static JObject ToJObject(decimal value) => new JObject { ["value"] = value };

        private static JObject ToJObject(KeyValuePair<Guid, string> _) =>
            new JObject { ["key"] = _.Key, ["value"] = _.Value };
    }
}
