using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;

namespace Tba.EventStore
{
    public interface ISupportNotifier
    {
        Task Notify(string message);
    }
}
