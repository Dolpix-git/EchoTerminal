using UnityEngine;

namespace EchoTerminal
{
public class CommandExecutor
{
	private readonly CommandParser _parser;
	private readonly Terminal _terminal;

	public CommandExecutor(Terminal terminal, CommandParser parser)
	{
		_terminal = terminal;
		_parser = parser;
	}

	public void Execute(string input)
	{
		var result = _parser.Parse(input);

		if (string.IsNullOrEmpty(result.CommandName))
		{
			return;
		}

		if (!result.IsKnownCommand)
		{
			_terminal.Log($"Unknown command: '{result.CommandName}'");
			return;
		}

		foreach (var overload in result.Overloads)
		{
			if (!overload.IsComplete)
			{
				continue;
			}

			Invoke(overload);
			return;
		}

		foreach (var overload in result.Overloads)
		{
			if (overload.Params.Count == 0 || !overload.Params[0].Expected.IsTarget)
			{
				continue;
			}

			var targetParam = overload.Params[0];
			if (targetParam is { Token: not null, IsValid: false })
			{
				_terminal.Log($"No GameObject named '{targetParam.Token[1..]}' found in scene.");
				return;
			}
		}

		foreach (var overload in result.Overloads)
		{
			if (overload.Params.Count == 0 || !overload.Params[0].Expected.IsTarget)
			{
				continue;
			}

			if (overload.Params[0].Token != null)
			{
				continue;
			}

			var allNonTargetValid = true;
			for (var i = 1; i < overload.Params.Count; i++)
			{
				if (!overload.Params[i].IsValid)
				{
					allNonTargetValid = false;
					break;
				}
			}

			if (!allNonTargetValid)
			{
				continue;
			}

			Invoke(overload);
			return;
		}

		foreach (var overload in result.Overloads)
		{
			foreach (var param in overload.Params)
			{
				if (param.Expected.IsTarget)
				{
					continue;
				}

				var type = param.Expected.Type;
				var checkType = type.IsGenericType &&
								type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>)
					? type.GetGenericArguments()[0]
					: type;
				if (!CommandProcessor.Parsers.ContainsKey(checkType) && !checkType.IsEnum)
				{
					Debug.LogWarning(
						$"[EchoTerminal] No parser registered for type '{checkType.Name}' (parameter '{param.Expected.Name}' on command '{result.CommandName}'). Register an IParser<{checkType.Name}> to support it.");
				}
			}
		}

		_terminal.Log($"Invalid arguments for '{result.CommandName}'");
	}

	private void Invoke(OverloadResult overload)
	{
		var entry = overload.Entry;
		var methodParams = entry.Method.GetParameters();
		var args = new object[methodParams.Length];
		var hasTarget = overload.Params.Count > 0 && overload.Params[0].Expected.IsTarget;
		var paramOffset = hasTarget ? 1 : 0;
		var userParamIdx = 0;

		for (var i = 0; i < methodParams.Length; i++)
		{
			if (methodParams[i].ParameterType == typeof(Terminal))
			{
				args[i] = _terminal;
			}
			else
			{
				args[i] = overload.Params[paramOffset + userParamIdx].Value;
				userParamIdx++;
			}
		}

		if (entry.IsStatic)
		{
			var invokeResult = entry.Method.Invoke(null, args);
			if (invokeResult is string message)
			{
				_terminal.Log(message);
			}

			return;
		}

		var singleTarget = hasTarget ? overload.Params[0].Value as GameObject : null;
		var targets = _parser.Registry.GetInstances(entry.MonoType);
		var invoked = false;

		foreach (var target in targets)
		{
			if (singleTarget != null && target.gameObject != singleTarget)
			{
				continue;
			}

			invoked = true;
			var invokeResult = entry.Method.Invoke(target, args);
			if (invokeResult is string message)
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
	}
}
}