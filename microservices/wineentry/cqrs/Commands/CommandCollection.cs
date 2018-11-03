using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace cqrs.Commands
{
    public class CommandCollection: IEnumerable<Command>
    {
        private int _sequence;
        private readonly List<Command> _commands;
        
        public CommandCollection(int sequence=0)
        {
            _sequence = sequence >= 0
                ? sequence
                : throw new ArgumentException("sequence cannot be negative");
            _commands = new List<Command>();
        }

        public CommandCollection Add(Guid aggregateId, CommandName name, JObject value)
        {
            _commands.Add(new Command(aggregateId, _sequence++, name, value));
            return this;
        }

        public IEnumerator<Command> GetEnumerator()
        {
            return _commands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _commands.GetEnumerator();
        }
    }
}