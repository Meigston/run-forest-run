namespace Brokers.Common.Interfaces
{
    using System.Collections.Generic;

    public interface IParameterizedEvent
    {
        Dictionary<string, string> GetParameters();
    }
}
