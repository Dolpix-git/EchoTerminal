using System;
using UnityEngine;

namespace EchoTerminal
{
public class CommandProcessor : IEchoComponent
{
	private static readonly Color ErrorColor = new(1f, 0.3f, 0.3f);
	private static readonly Color ResponseColor = new(0.5f, 1f, 0.5f);
	private readonly CommandParser _parser;

	private readonly Terminal _terminal;

	public CommandProcessor(Terminal terminal, CommandParser parser)
	{
		_terminal = terminal;
		_parser = parser;
	}

	public void Execute(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return;
		}

		var tokens = _parser.Tokenize(input);
		if (tokens.Count == 0)
		{
			return;
		}

		if (!_parser.TryParseCommandName(tokens, out var commandName))
		{
			_terminal.Log($"Unknown command: '{tokens[0].ToLowerInvariant()}'", ErrorColor);
			return;
		}

		var hasTarget = _parser.TryParseTarget(tokens, out var targetName);
		var matches = CommandRegistry.Instance.FindCommands(commandName, targetName);
		if (matches.Count == 0 && hasTarget)
		{
			_terminal.Log(
				$"No GameObject named '{targetName}' has command '{commandName}'",
				ErrorColor);
			return;
		}

		var args = _parser.ExtractArguments(tokens, hasTarget);
		var invoked = false;
		string lastError = null;

		foreach (var (target, method) in matches)
		{
			var parameters = method.GetParameters();

			if (args.Length > parameters.Length && parameters.Length > 0
				&& parameters[^1].ParameterType == typeof(string))
			{
				var collapsed = new string[parameters.Length];
				Array.Copy(args, collapsed, parameters.Length - 1);
				collapsed[^1] = string.Join(" ", args, parameters.Length - 1,
					args.Length - parameters.Length + 1);
				args = collapsed;
			}

			if (args.Length != parameters.Length)
			{
				lastError = $"'{commandName}' expects {parameters.Length} argument(s), got {args.Length}";
				continue;
			}

			if (!_parser.TryParseArguments(args, parameters, out var convertedArgs, out var failedIndex))
			{
				var param = parameters[failedIndex];
				lastError = $"Argument '{args[failedIndex]}' cannot be parsed as " +
							$"{param.ParameterType.Name} for parameter '{param.Name}'";
				continue;
			}

			try
			{
				var result = method.Invoke(target, convertedArgs);

				if (method.ReturnType == typeof(string) && result is string str)
				{
					_terminal.Log(str, ResponseColor);
				}

				invoked = true;
			}
			catch (Exception ex)
			{
				var inner = ex.InnerException ?? ex;
				_terminal.Log($"Error in {commandName}: {inner.Message}", ErrorColor);
			}
		}

		if (!invoked && lastError != null)
		{
			_terminal.Log(lastError, ErrorColor);
		}
	}
}
}
