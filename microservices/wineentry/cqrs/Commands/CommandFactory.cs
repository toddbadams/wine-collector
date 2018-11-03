using System;
using System.Collections.Generic;
using cqrs.ApiModels;

namespace cqrs.Commands
{
    public class CommandFactory : ICommandFactory
    {
        public IEnumerable<Command> Create(WineEntryCreateRequest request)
        {
            var wineEntryId = Guid.NewGuid();

            return new CommandBuilder()
                .Add(wineEntryId, CommandName.SetQuantity, new SimpleCommandValue<int>(request.Quantity))
                .Add(wineEntryId, CommandName.SetBottleSize, new SimpleCommandValue<int>(request.BottleSize))
                .Add(wineEntryId, CommandName.SetBottlesPerCase, new SimpleCommandValue<int>(request.BottlesPerCase))
                .Add(wineEntryId, CommandName.SetAcquiredOn, new SimpleCommandValue<DateTimeOffset>(request.AcquiredOn))
                .Add(wineEntryId, CommandName.SetCostPerBottle, new SimpleCommandValue<decimal>(request.CostPerBottle))
                .Add(wineEntryId, CommandName.SetCellar, new SimpleCommandValue<Guid>(request.CellarId))
                .Add(wineEntryId, CommandName.SetWine, new SimpleCommandValue<Guid>(request.WineId));
        }
    }
}