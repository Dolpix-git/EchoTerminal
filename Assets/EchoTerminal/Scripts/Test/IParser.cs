namespace EchoTerminal.Scripts.Test
{
    public interface IParser
    {
        bool TryParse(string input, out object result, out int charsConsumed);
    }
}