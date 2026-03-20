using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EchoTerminal
{
public class CommandParser
{
	public CommandParser(CommandRegistry registry)
	{
		Registry = registry;
	}

	public CommandRegistry Registry { get; }

	public CommandParseResult Parse(string input)
	{
		if (!CommandProcessor.TryParseInput(input, out var commandName, out var args, out var leadingSpaces))
		{
			return CommandParseResult.Empty;
		}

		if (!Registry.TryGet(commandName, out var entries))
		{
			return new(commandName, false, args, leadingSpaces, Array.Empty<OverloadResult>());
		}

		var overloads = new List<OverloadResult>();
		foreach (var entry in entries)
		{
			overloads.Add(ParseOverload(entry, args ?? string.Empty));
		}

		return new(commandName, true, args, leadingSpaces, overloads);
	}

	private OverloadResult ParseOverload(CommandEntry entry, string remaining)
	{
		var parsers = CommandProcessor.Parsers;
		var expectedParams = CommandProcessor.GetParams(entry);
		var results = new List<ParamResult>();
		var allValid = true;

		foreach (var param in expectedParams)
		{
			if (remaining.Length == 0)
			{
				results.Add(new(param, null, null, false));
				allValid = false;
				continue;
			}

			if (param.IsTarget)
			{
				if (!remaining.StartsWith("@"))
				{
					results.Add(new(param, null, null, false));
					allValid = false;
					continue;
				}

				if (parsers[typeof(GameObject)].TryParse(remaining, out var goObj, out var goConsumed))
				{
					var token = remaining[..goConsumed];
					remaining = remaining[goConsumed..].TrimStart();
					var valid = goObj != null;
					results.Add(new(param, token, goObj, valid));
					if (!valid)
					{
						allValid = false;
					}
				}
				else
				{
					var (token, rest) = ConsumeToken(remaining);
					remaining = rest;
					results.Add(new(param, token, null, false));
					allValid = false;
				}

				continue;
			}

			if (param.Type.IsGenericType && param.Type.GetGenericTypeDefinition() == typeof(List<>))
			{
				var elementType = param.Type.GetGenericArguments()[0];
				var list = (IList)Activator.CreateInstance(param.Type);
				var valid = true;
				var pos = 0;

				while (pos < remaining.Length)
				{
					string elementSlice;
					if (remaining[pos] == '(')
					{
						var close = remaining.IndexOf(')', pos);
						if (close == -1)
						{
							valid = false;
							break;
						}

						elementSlice = remaining[pos..(close + 1)];
					}
					else
					{
						var comma = remaining.IndexOf(',', pos);
						elementSlice = comma == -1 ? remaining[pos..] : remaining[pos..comma];
					}

					if (!CommandProcessor.TryParseToken(elementSlice.Trim(), elementType, out var el))
					{
						valid = false;
						break;
					}

					list.Add(el);
					pos += elementSlice.Length;

					if (pos < remaining.Length && remaining[pos] == ',')
					{
						pos++;
					}
					else
					{
						break;
					}
				}

				results.Add(new(param, remaining, valid ? list : null, valid));
				if (!valid)
				{
					allValid = false;
				}

				remaining = string.Empty;
				continue;
			}

			if (parsers.TryGetValue(param.Type, out var parser))
			{
				if (parser.TryParse(remaining, out var value, out var consumed))
				{
					var token = remaining[..consumed];
					remaining = remaining[consumed..].TrimStart();
					results.Add(new(param, token, value, true));
				}
				else if (parser is IRestConsumingParser)
				{
					var token = remaining.Length > 0 ? remaining : null;
					results.Add(new(param, token, null, false));
					remaining = string.Empty;
					allValid = false;
				}
				else
				{
					var (token, rest) = ConsumeToken(remaining);
					remaining = rest;
					results.Add(new(param, token, null, false));
					allValid = false;
				}

				continue;
			}

			if (param.Type.IsEnum)
			{
				var (token, rest) = ConsumeToken(remaining);
				remaining = rest;

				if (CommandProcessor.TryParseToken(token, param.Type, out var value))
				{
					results.Add(new(param, token, value, true));
				}
				else
				{
					results.Add(new(param, token, null, false));
					allValid = false;
				}

				continue;
			}

			var (t, r) = ConsumeToken(remaining);
			remaining = r;
			results.Add(new(param, t, null, false));
			allValid = false;
		}

		return new(entry, results, allValid && remaining.Length == 0);
	}

	private static (string token, string rest) ConsumeToken(string input)
	{
		var end = input.IndexOf(' ');
		return end == -1 ? (input, string.Empty) : (input[..end], input[(end + 1)..]);
	}
}
}