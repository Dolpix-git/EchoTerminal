using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EchoTerminal.Scripts.Test;
using UnityEngine;

namespace EchoTerminal
{
public class CommandProcessor
{
	private static Dictionary<Type, IParser> _parsers;

	public static IReadOnlyDictionary<Type, IParser> Parsers => GetParsers();

	private readonly CommandRegistry _registry;
	private readonly Terminal _terminal;

	public CommandProcessor(Terminal terminal, CommandRegistry registry)
	{
		_terminal = terminal;
		_registry = registry;
	}

	private static Dictionary<Type, IParser> GetParsers()
	{
		if (_parsers != null)
		{
			return _parsers;
		}

		_parsers = new();

		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (assembly.IsDynamic)
			{
				continue;
			}

			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				types = e.Types;
			}

			if (types == null)
			{
				continue;
			}

			foreach (var type in types)
			{
				if (type == null || type.IsAbstract || type.IsInterface)
				{
					continue;
				}

				if (!typeof(IParser).IsAssignableFrom(type))
				{
					continue;
				}

				if (type.GetConstructor(Type.EmptyTypes) == null)
				{
					continue;
				}

				var parser = (IParser)Activator.CreateInstance(type);
				_parsers[parser.TargetType] = parser;
			}
		}

		return _parsers;
	}

	public void Execute(string input)
	{
		var remaining = input.TrimStart();
		if (remaining.Length == 0)
		{
			return;
		}

		var space = remaining.IndexOf(' ');
		var commandName = space == -1 ? remaining : remaining[..space];
		remaining = space == -1 ? string.Empty : remaining[space..].TrimStart();

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
		var parsers = GetParsers();

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
				if (!Enum.TryParse(paramType, token, true, out var enumVal))
				{
					return false;
				}

				args[i] = enumVal;
				commandString = commandString[token.Length..].TrimStart();
				continue;
			}

			if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(List<>))
			{
				var elementType = paramType.GetGenericArguments()[0];
				var list = (IList)Activator.CreateInstance(paramType);
				var tokens = commandString.Split(',');

				foreach (var raw in tokens)
				{
					var t = raw.Trim();
					if (parsers.TryGetValue(elementType, out var ep))
					{
						if (!ep.TryParse(t, out var el, out _))
						{
							return false;
						}

						list.Add(el);
					}
					else if (elementType.IsEnum)
					{
						if (!Enum.TryParse(elementType, t, true, out var enumEl))
						{
							return false;
						}

						list.Add(enumEl);
					}
					else
					{
						return false;
					}
				}

				args[i] = list;
				commandString = string.Empty;
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