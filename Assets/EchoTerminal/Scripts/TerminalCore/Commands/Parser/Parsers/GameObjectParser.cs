using UnityEngine;

namespace EchoTerminal
{
public class GameObjectParser
{
	public Color HighlightColor => new(1f, 0.85f, 0.4f);

	public bool TryParse(string token, out string targetName)
	{
		targetName = null;

		if (token.Length <= 1 || token[0] != '@')
		{
			return false;
		}

		targetName = token.Substring(1);
		return true;
	}
}
}
