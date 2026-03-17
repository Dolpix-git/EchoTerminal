using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoTerminal
{
[CreateAssetMenu(fileName = "TerminalHintFormats", menuName = "Echo Terminal/Hint Formats")]
public class TerminalHintFormats : ScriptableObject
{
	[SerializeField] private string _fallbackFormat = "value";
	[SerializeField] private List<TypeFormatEntry> _entries = new();

	private Dictionary<Type, string> _cache;

	public string FallbackFormat => _fallbackFormat;

	private void Reset()
	{
		_entries = new()
		{
			new() { TypeFullName = "System.Int32", Format = "0" },
			new() { TypeFullName = "System.Single", Format = "0.0" },
			new() { TypeFullName = "System.String", Format = "\"text\"" },
			new() { TypeFullName = "System.Boolean", Format = "true" },
			new() { TypeFullName = "UnityEngine.Vector2", Format = "0,0" },
			new() { TypeFullName = "UnityEngine.Vector3", Format = "0,0,0" },
			new() { TypeFullName = "UnityEngine.Color", Format = "red|#RRGGBB" },
			new() { TypeFullName = "UnityEngine.GameObject", Format = "name" }
		};
	}

	private void OnValidate()
	{
		_cache = null;
	}

	public bool TryGetFormat(Type type, out string format)
	{
		_cache ??= BuildCache();
		return _cache.TryGetValue(type, out format);
	}

	private Dictionary<Type, string> BuildCache()
	{
		var dict = new Dictionary<Type, string>();
		foreach (var entry in _entries)
		{
			var type = Type.GetType(entry.TypeFullName);
			if (type != null)
			{
				dict[type] = entry.Format;
			}
		}

		return dict;
	}

	[Serializable]
	public struct TypeFormatEntry
	{
		public string TypeFullName;
		public string Format;
	}
}
}