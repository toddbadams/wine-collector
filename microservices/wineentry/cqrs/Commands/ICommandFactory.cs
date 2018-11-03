using System;
using System.Collections.Generic;
using cqrs.ApiModels;

namespace cqrs.Commands
{
    public interface ICommandFactory
    {
        IEnumerable<Command> Create(WineEntryCreateRequest request);
    }
}