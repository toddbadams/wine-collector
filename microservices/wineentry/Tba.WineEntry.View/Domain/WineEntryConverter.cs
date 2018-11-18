using System;
using System.Collections.Generic;
using AutoMapper;
using Newtonsoft.Json.Linq;
using Tba.WineEntry.View.Application.Events;

namespace Tba.WineEntry.View.Domain
{
    public class WineEntryConverter : ITypeConverter<Event, WineEntry>
    {
        public WineEntry Convert(Event source, WineEntry destination, ResolutionContext context)
        {
            // todo
            throw new NotImplementedException();
        }
    }
}
