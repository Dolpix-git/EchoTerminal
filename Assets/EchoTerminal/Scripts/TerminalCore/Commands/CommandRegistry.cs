using System;
using System.Collections.Generic;
using System.Reflection;

namespace EchoTerminal
{
public class CommandRegistry
{
	private readonly Dictionary<string, MethodInfo> _commands = new(StringComparer.OrdinalIgnoreCase);
	private bool _scanned;

	public static CommandRegistry Instance { get; } = new();

	public bool TryGet(string name, out MethodInfo method)
	{
		EnsureScanned();
		return _commands.TryGetValue(name, out method);
	}

	public IReadOnlyCollection<string> GetCommandNames()
	{
		EnsureScanned();
		return _commands.Keys;
	}

	private void EnsureScanned()
	{
		if (_scanned)
		{
			return;
		}

		_scanned = true;

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

			foreach (var type in types)
			{
				if (type == null)
				{
					continue;
				}

				foreach (var method in type.GetMethods(BindingFlags.Static |
													   BindingFlags.Public |
													   BindingFlags.NonPublic))
				{
					var attr = method.GetCustomAttribute<TerminalCommandAttribute>();
					if (attr == null)
					{
						continue;
					}

					var commandName = string.IsNullOrEmpty(attr.Name)
						? method.Name.ToLowerInvariant()
						: attr.Name.ToLowerInvariant();

					_commands[commandName] = method;
				}
			}
		}
	}
}
}