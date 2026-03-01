using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EchoTerminal
{
public class CommandRegistry
{
	private readonly Dictionary<string, CommandEntry> _commands = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<Type, Component> _instanceCache = new();
	private bool _scanned;

	public IReadOnlyCollection<string> GetCommandNames()
	{
		return _commands.Keys;
	}

	public bool TryGet(string name, out CommandEntry entry)
	{
		return _commands.TryGetValue(name, out entry);
	}

	public Component GetInstance(Type monoType)
	{
		if (_instanceCache.TryGetValue(monoType, out var cached) && cached != null)
		{
			return cached;
		}

		var found = (Component)Object.FindFirstObjectByType(monoType);
		if (found != null)
		{
			_instanceCache[monoType] = found;
		}

		return found;
	}

	public void Scan()
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

				ScanMethods(
					type, 
					BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, 
					monoType: null
				);

				if (typeof(MonoBehaviour).IsAssignableFrom(type) && !type.IsAbstract)
				{
					ScanMethods(
						type,
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
						monoType: type
					);
				}
			}
		}
	}

	private void ScanMethods(Type type, BindingFlags flags, Type monoType)
	{
		foreach (var method in type.GetMethods(flags))
		{
			var attr = method.GetCustomAttribute<TerminalCommandAttribute>();
			if (attr == null)
			{
				continue;
			}

			var name = string.IsNullOrEmpty(attr.Name)
				? method.Name.ToLowerInvariant()
				: attr.Name.ToLowerInvariant();

			_commands[name] = new(method, monoType);
		}
	}
}
}