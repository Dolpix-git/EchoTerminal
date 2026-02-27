namespace EchoTerminal.Scripts.Test
{
    public class IntParser : IParser
    {
        public bool TryParse(string input, out object result, out int charsConsumed)
        {
            int end = input.IndexOf(' ');
            string token = end == -1 ? input : input.Substring(0, end);

            if (int.TryParse(token, out int value))
            {
                result = value;
                charsConsumed = token.Length;
                return true;
            }

            result = null;
            charsConsumed = 0;
            return false;
        }
    }
}