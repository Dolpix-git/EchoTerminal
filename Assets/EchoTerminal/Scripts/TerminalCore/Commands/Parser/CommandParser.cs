using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace EchoTerminal
{
public class CommandParser
{
	public ValueParserRegistry Values { get; }
	public Color CommandNameColor => _commandNameParser.HighlightColor;
	public Color TargetColor => _gameObjectParser.HighlightColor;

	private readonly CommandNameParser _commandNameParser;
	private readonly GameObjectParser _gameObjectParser;
	private readonly Dictionary<char, char> _delimiterPairs;

	public CommandParser()
	{
		Values = new();
		_commandNameParser = new();
		_gameObjectParser = new();
		_delimiterPairs = Values.GetDelimiterPairs();
	}

	public List<string> Tokenize(string input)
	{
		return TokenizeWithSpans(input).ConvertAll(s => s.token);
	}

	public List<(string token, int start, int end)> TokenizeWithSpans(string input)
	{
		var spans = new List<(string, int, int)>();
		var current = new StringBuilder();
		var tokenStart = -1;

		for (var i = 0; i < input.Length; i++)
		{
			var character = input[i];

			if (character == ' ')
			{
				if (current.Length > 0)
				{
					spans.Add((current.ToString(), tokenStart, i));
					current.Clear();
					tokenStart = -1;
				}

				continue;
			}

			if (tokenStart == -1)
			{
				tokenStart = i;
			}

			if (_delimiterPairs != null && _delimiterPairs.TryGetValue(character, out var closeDelim))
			{
				if (current.Length > 0)
				{
					spans.Add((current.ToString(), tokenStart, i));
					current.Clear();
				}

				tokenStart = i;
				current.Append(character);
				i++;

				if (character == closeDelim)
				{
					while (i < input.Length && input[i] != closeDelim)
					{
						current.Append(input[i]);
						i++;
					}

					if (i < input.Length)
					{
						current.Append(input[i]);
						i++;
					}
				}
				else
				{
					var depth = 1;

					while (i < input.Length && depth > 0)
					{
						var ch = input[i];
						current.Append(ch);

						if (ch == character)
						{
							depth++;
						}
						else if (ch == closeDelim)
						{
							depth--;
						}

						i++;
					}
				}

				spans.Add((current.ToString(), tokenStart, i));
				current.Clear();
				tokenStart = -1;
				i--;
				continue;
			}

			current.Append(character);
		}

		if (current.Length > 0)
		{
			spans.Add((current.ToString(), tokenStart, input.Length));
		}

		return spans;
	}

	public bool TryParseCommandName(List<string> tokens, out string commandName)
	{
		commandName = null;

		if (tokens.Count == 0)
		{
			return false;
		}

		return _commandNameParser.TryParse(tokens[0], out commandName);
	}

	public bool TryParseTarget(List<string> tokens, out string targetName)
	{
		targetName = null;

		if (tokens.Count < 2)
		{
			return false;
		}

		return _gameObjectParser.TryParse(tokens[1], out targetName);
	}

	public string[] ExtractArguments(List<string> tokens, bool hasTarget)
	{
		var start = hasTarget ? 2 : 1;
		var count = Math.Max(0, tokens.Count - start);
		var args = new string[count];

		for (var i = 0; i < count; i++)
		{
			args[i] = tokens[start + i];
		}

		return args;
	}

	public bool TryParseArguments(
		string[] args,
		ParameterInfo[] parameters,
		out object[] results,
		out int failedIndex)
	{
		results = null;
		failedIndex = -1;

		if (args.Length != parameters.Length)
		{
			return false;
		}

		results = new object[parameters.Length];

		for (var i = 0; i < parameters.Length; i++)
		{
			if (!Values.TryConvertSingle(args[i], parameters[i].ParameterType, out results[i]))
			{
				failedIndex = i;
				return false;
			}
		}

		return true;
	}

	public InputAnalysis Analyze(string input)
	{
		var result = new InputAnalysis { SubCommandOffset = -1 };

		if (string.IsNullOrEmpty(input))
		{
			result.Spans = new List<(string, int, int)>();
			return result;
		}

		result.Spans = TokenizeWithSpans(input);
		result.TrailingSpace = input[^1] == ' ';

		if (result.Spans.Count == 0)
		{
			return result;
		}

		if (result.TrailingSpace)
		{
			result.EditingIndex = result.Spans.Count;
			result.Partial = "";
			result.ReplaceStart = input.Length;
			result.ReplaceEnd = input.Length;
		}
		else
		{
			result.EditingIndex = result.Spans.Count - 1;
			var last = result.Spans[result.EditingIndex];
			result.Partial = last.token;
			result.ReplaceStart = last.start;
			result.ReplaceEnd = last.end;
		}

		result.CommandName = result.Spans[0].token.ToLowerInvariant();
		result.CommandValid = CommandRegistry.Instance.HasCommand(result.CommandName);

		result.HasTarget = result.Spans.Count > 1 &&
						   result.Spans[1].token.Length > 0 &&
						   result.Spans[1].token[0] == '@';
		result.ArgStart = result.HasTarget ? 2 : 1;

		if (result.CommandValid)
		{
			result.Parameters = ResolveParameters(
				result.CommandName,
				result.Spans.Count,
				result.HasTarget);
		}

		if (result.Parameters != null &&
			result.Parameters.Length >= 2 &&
			result.Parameters[^1].ParameterType == typeof(string))
		{
			result.SubCommandOffset = result.ArgStart + result.Parameters.Length - 1;
		}

		return result;
	}

	public ParameterInfo[] ResolveParameters(string commandName, int spanCount, bool hasTarget)
	{
		var matches = CommandRegistry.Instance.FindCommands(commandName, null);

		if (matches.Count == 0)
		{
			return null;
		}

		var argCount = Math.Max(0, spanCount - 1 - (hasTarget ? 1 : 0));

		foreach (var (_, method) in matches)
		{
			if (method.GetParameters().Length == argCount)
			{
				return method.GetParameters();
			}
		}

		return matches[0].method.GetParameters();
	}
}
}