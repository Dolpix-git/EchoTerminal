namespace EchoTerminal.Scripts.Test
{
    public class BoolParser : IParser
    {
        public bool TryParse(string input, out object result, out int charsConsumed)
        {
            int end = input.IndexOf(' ');
            string token = end == -1 ? input : input.Substring(0, end);

            if (bool.TryParse(token, out bool value))
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