using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EchoTerminal
{
public class CommandRegistry
{
	private const BindingFlags _instanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
	private const BindingFlags _staticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private readonly Dictionary<string, List<(MonoBehaviour target, MethodInfo method)>> _commandCache = new(StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<MonoBehaviour> _registered = new();
	private readonly Dictionary<Type, (MethodInfo method, string commandName)[]> _typeCache = new();
	private bool _sceneScanned;
	private bool _staticScanned;
	public static CommandRegistry Instance { get; } = new();

	public void Register(MonoBehaviour behaviour)
	{
		if (behaviour == null || !_registered.Add(behaviour))
		{
			return;
		}

		var commands = GetCommandsForType(behaviour.GetType(), _instanceFlags);

		foreach (var (method, commandName) in commands)
		{
			if (!_commandCache.TryGetValue(commandName, out var list))
			{
				list = new List<(MonoBehaviour, MethodInfo)>();
				_commandCache[commandName] = list;
			}

			list.Add((behaviour, method));
		}
	}

	public void Unregister(MonoBehaviour behaviour)
	{
		if (behaviour == null || !_registered.Remove(behaviour))
		{
			return;
		}

		var commands = GetCommandsForType(behaviour.GetType(), _instanceFlags);

		foreach (var (_, commandName) in commands)
		{
			if (!_commandCache.TryGetValue(commandName, out var list))
			{
				continue;
			}

			for (var i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].target == behaviour)
				{
					list.RemoveAt(i);
				}
			}

			if (list.Count == 0)
			{
				_commandCache.Remove(commandName);
			}
		}
	}

	public bool HasCommand(string name)
	{
		if (_commandCache.ContainsKey(name))
		{
			return true;
		}

		ScanScene();
		return _commandCache.ContainsKey(name);
	}

	public List<(MonoBehaviour target, MethodInfo method)> FindCommands(
		string commandName,
		string targetName)
	{
		if (!_commandCache.TryGetValue(commandName, out var all))
		{
			ScanScene();

			if (!_commandCache.TryGetValue(commandName, out all))
			{
				return new List<(MonoBehaviour, MethodInfo)>();
			}
		}

		if (targetName == null)
		{
			return new List<(MonoBehaviour, MethodInfo)>(all);
		}

		var filtered = new List<(MonoBehaviour, MethodInfo)>();

		foreach (var entry in all)
		{
			if (entry.target == null)
			{
				continue;
			}

			if (string.Equals(entry.target.gameObject.name, targetName, StringComparison.OrdinalIgnoreCase))
			{
				filtered.Add(entry);
			}
		}

		return filtered;
	}

	public IReadOnlyCollection<string> GetCommandNames()
	{
		ScanScene();
		return _commandCache.Keys;
	}

	public List<string> GetTargetNames(string commandName)
	{
		ScanScene();

		if (!_commandCache.TryGetValue(commandName, out var all))
		{
			return new();
		}

		var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var names = new List<string>();

		foreach (var (target, _) in all)
		{
			if (target == null)
			{
				continue;
			}

			var name = target.gameObject.name;

			if (seen.Add(name))
			{
				names.Add(name);
			}
		}

		return names;
	}

	private void ScanScene()
	{
		ScanStaticCommands();

		if (_sceneScanned)
		{
			return;
		}

		_sceneScanned = true;

		foreach (var behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
		{
			if (_registered.Contains(behaviour))
			{
				continue;
			}

			if (GetCommandsForType(behaviour.GetType(), _instanceFlags).Length > 0)
			{
				Register(behaviour);
			}
		}
	}

	private void ScanStaticCommands()
	{
		if (_staticScanned)
		{
			return;
		}

		_staticScanned = true;

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
				if (type == null || !type.IsAbstract || !type.IsSealed)
				{
					continue;
				}

				var commands = GetCommandsForType(type, _staticFlags);

				foreach (var (method, commandName) in commands)
				{
					if (!_commandCache.TryGetValue(commandName, out var list))
					{
						list = new List<(MonoBehaviour, MethodInfo)>();
						_commandCache[commandName] = list;
					}

					list.Add((null, method));
				}
			}
		}
	}

	private (MethodInfo method, string commandName)[] GetCommandsForType(Type type, BindingFlags flags)
	{
		if (_typeCache.TryGetValue(type, out var cached))
		{
			return cached;
		}

		var methods = type.GetMethods(flags);
		var results = new List<(MethodInfo, string)>();

		foreach (var method in methods)
		{
			var attr = method.GetCustomAttribute<TerminalCommandAttribute>();
			if (attr == null)
			{
				continue;
			}

			var name = string.IsNullOrEmpty(attr.Name)
				? method.Name.ToLowerInvariant()
				: attr.Name.ToLowerInvariant();

			results.Add((method, name));
		}

		var array = results.ToArray();
		_typeCache[type] = array;
		return array;
	}
}
}