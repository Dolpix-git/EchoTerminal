using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoTerminal
{
public static class BindStore
{
	private const string _prefsKey = "EchoTerminal.Binds";
	private static Dictionary<Key, string> _cache;

	public static event Action Changed;

	[Serializable]
	private class BindData
	{
		public List<Key> Keys = new();
		public List<string> Commands = new();
	}

	public static Dictionary<Key, string> GetAll()
	{
		if (_cache != null)
		{
			return _cache;
		}

		var data = Load();
		_cache = new Dictionary<Key, string>();

		for (var i = 0; i < data.Keys.Count; i++)
		{
			_cache[data.Keys[i]] = data.Commands[i];
		}

		return _cache;
	}

	public static void Set(Key key, string command)
	{
		var data = Load();
		var index = data.Keys.FindIndex(k => k == key);

		if (index >= 0)
		{
			data.Commands[index] = command;
		}
		else
		{
			data.Keys.Add(key);
			data.Commands.Add(command);
		}

		Save(data);
		_cache = null;
		Changed?.Invoke();
	}

	public static bool Remove(Key key)
	{
		var data = Load();
		var index = data.Keys.FindIndex(k => k == key);

		if (index < 0)
		{
			return false;
		}

		data.Keys.RemoveAt(index);
		data.Commands.RemoveAt(index);
		Save(data);
		_cache = null;
		Changed?.Invoke();
		return true;
	}

	public static void Clear()
	{
		PlayerPrefs.DeleteKey(_prefsKey);
		PlayerPrefs.Save();
		_cache = null;
		Changed?.Invoke();
	}

	private static BindData Load()
	{
		var json = PlayerPrefs.GetString(_prefsKey, "");

		if (string.IsNullOrEmpty(json))
		{
			return new();
		}

		try
		{
			return JsonUtility.FromJson<BindData>(json);
		}
		catch
		{
			return new();
		}
	}

	private static void Save(BindData data)
	{
		PlayerPrefs.SetString(_prefsKey, JsonUtility.ToJson(data));
		PlayerPrefs.Save();
	}
}
}
