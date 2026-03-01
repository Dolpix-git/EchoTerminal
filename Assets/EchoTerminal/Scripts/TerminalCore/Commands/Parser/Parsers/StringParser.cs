namespace EchoTerminal.Scripts.Test
{
    public class StringParser : IParser
    {
        public bool TryParse(string input, out object result, out int charsConsumed)
        {
            if (input.Length == 0)
            {
                result = null;
                charsConsumed = 0;
                return false;
            }

            if (input[0] == '"')
            {
                int closing = input.IndexOf('"', 1);
                if (closing != -1)
                {
                    result = input.Substring(1, closing - 1);
                    charsConsumed = closing + 1;
                    return true;
                }
            }

            int end = input.IndexOf(' ');
            string token = end == -1 ? input : input.Substring(0, end);
            result = token;
            charsConsumed = token.Length;
            return true;
        }
    }
}