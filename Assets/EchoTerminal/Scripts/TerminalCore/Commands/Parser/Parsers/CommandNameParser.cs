using UnityEngine;

namespace EchoTerminal
{
public class CommandNameParser
{
	public Color HighlightColor => new(0.5f, 0.9f, 1f);

	public bool TryParse(string token, out string commandName)
	{
		commandName = token.ToLowerInvariant();
		return CommandRegistry.Instance.HasCommand(commandName);
	}
}
}
