using System;

namespace EchoTerminal
{
public class CommandNameParser : IRestConsumingParser
{
	public Type TargetType => typeof(CommandName);

	public bool TryParse(string input, out object result, out int charsConsumed)
	{
		result = null;
		charsConsumed = 0;

		if (input.Length == 0 || input[0] != '>')
		{
			return false;
		}

		var close = input.IndexOf('<', 1);

		if (close == -1)
		{
			return false;
		}

		result = new CommandName(input.Substring(1, close - 1));
		charsConsumed = close + 1;
		return true;
	}
}
}