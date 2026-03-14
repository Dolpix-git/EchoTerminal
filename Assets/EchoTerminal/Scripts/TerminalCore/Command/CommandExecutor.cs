using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EchoTerminal
{
public class CommandExecutor
{
	private readonly CommandRegistry _registry;
	private readonly Terminal _terminal;

	public CommandExecutor(Terminal terminal, CommandRegistry registry)
	{
		_terminal = terminal;
		_registry = registry;
	}

	public void Execute(string input)
	{
		if (!CommandProcessor.TryParseInput(input, out var commandName, out var remaining, out _))
		{
			return;
		}

		remaining ??= string.Empty;

		if (!_registry.TryGet(commandName, out var entries))
		{
			_terminal.Log($"Unknown command: '{commandName}'");
			return;
		}

		foreach (var entry in entries)
		{
			if (TryInvoke(entry, remaining))
			{
				return;
			}
		}

		_terminal.Log($"Invalid arguments for '{commandName}'");
	}

	private bool TryInvoke(CommandEntry entry, string commandString)
	{
		var parsers = CommandProcessor.Parsers;

		GameObject singleTarget = null;
		if (!entry.IsStatic && parsers[typeof(GameObject)].TryParse(commandString, out var goObj, out var goConsumed))
		{
			var targetName = commandString[1..goConsumed];
			commandString = commandString[goConsumed..].TrimStart();
			singleTarget = goObj as GameObject;
			if (singleTarget == null)
			{
				_terminal.Log($"No GameObject named '{targetName}' found in scene.");
				return true;
			}
		}

		var parameters = entry.Method.GetParameters();
		var args = new object[parameters.Length];

		for (var i = 0; i < parameters.Length; i++)
		{
			var paramType = parameters[i].ParameterType;

			if (paramType == typeof(Terminal))
			{
				args[i] = _terminal;
				continue;
			}

			if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(List<>))
			{
				var elementType = paramType.GetGenericArguments()[0];
				var list = (IList)Activator.CreateInstance(paramType);
				foreach (var raw in commandString.Split(','))
				{
					if (!CommandProcessor.TryParseToken(raw.Trim(), elementType, out var el))
					{
						return false;
					}

					list.Add(el);
				}

				args[i] = list;
				commandString = string.Empty;
				continue;
			}

			if (parsers.TryGetValue(paramType, out var parser))
			{
				if (!parser.TryParse(commandString, out args[i], out var consumed))
				{
					return false;
				}

				commandString = commandString[consumed..].TrimStart();
				continue;
			}

			if (paramType.IsEnum)
			{
				var end = commandString.IndexOf(' ');
				var token = end == -1 ? commandString : commandString[..end];
				if (!CommandProcessor.TryParseToken(token, paramType, out args[i]))
				{
					return false;
				}

				commandString = commandString[token.Length..].TrimStart();
				continue;
			}

			return false;
		}

		if (entry.IsStatic)
		{
			var result = entry.Method.Invoke(null, args);
			if (result is string message)
			{
				_terminal.Log(message);
			}

			return true;
		}

		var targets = _registry.GetInstances(entry.MonoType);
		var invoked = false;

		foreach (var target in targets)
		{
			if (singleTarget != null && target.gameObject != singleTarget)
			{
				continue;
			}

			invoked = true;
			var result = entry.Method.Invoke(target, args);
			if (result is string message)
			{
				_terminal.Log(message);
			}
		}

		if (!invoked)
		{
			_terminal.Log(singleTarget != null
				? $"No '{entry.MonoType.Name}' on '{singleTarget.name}' found in scene."
				: $"No active '{entry.MonoType.Name}' found in scene.");
		}

		return true;
	}
}
}