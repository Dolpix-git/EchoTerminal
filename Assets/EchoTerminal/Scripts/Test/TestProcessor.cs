using System.Collections.Generic;
using UnityEngine;

namespace EchoTerminal.Scripts.Test
{
public class TestProcessor : MonoBehaviour
{
	private readonly List<IParser> _parsers = new()
	{
		new IntParser(),
		new BoolParser(),
		new StringParser()
	};

	[TerminalCommand]
	public void Process()
	{
		const string input = "fuck \"the skinny guys\" 1010011   false yo";
		var remaining = input.TrimStart();

		while (remaining.Length > 0)
		{
			var parsed = false;

			foreach (var parser in _parsers)
			{
				if (!parser.TryParse(remaining, out var result, out var consumed))
				{
					continue;
				}

				Debug.Log($"[{result.GetType().Name}] {result}");
				remaining = remaining.Substring(consumed).TrimStart();
				parsed = true;
				break;
			}

			if (parsed)
			{
				continue;
			}

			Debug.LogWarning($"Could not parse: '{remaining}'");
			break;
		}
	}
}
}