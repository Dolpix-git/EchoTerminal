using System;

namespace EchoTerminal.Scripts.Test
{
    public interface IParser
    {
        Type TargetType { get; }
        bool TryParse(string input, out object result, out int charsConsumed);
    }
}