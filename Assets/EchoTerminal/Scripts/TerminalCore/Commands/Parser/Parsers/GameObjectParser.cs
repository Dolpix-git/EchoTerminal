using UnityEngine;

namespace EchoTerminal.Scripts.Test
{
public class GameObjectParser : IParser
{
	public bool TryParse(string input, out object result, out int charsConsumed)
	{
		result = null;
		charsConsumed = 0;

		if (string.IsNullOrEmpty(input) || input[0] != '@')
		{
			return false;
		}

		var space = input.IndexOf(' ', 1);
		var name = space == -1 ? input[1..] : input[1..space];
		charsConsumed = name.Length + 1;
		result = GameObject.Find(name);
		return true;
	}
}
}