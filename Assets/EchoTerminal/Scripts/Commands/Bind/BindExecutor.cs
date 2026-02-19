using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoTerminal
{
public class BindExecutor : MonoBehaviour
{
	private GameTerminalUI _terminalUI;
	private Dictionary<Key, string> _binds;

	public void Init(GameTerminalUI terminalUI)
	{
		_terminalUI = terminalUI;
		RebuildCache();
		BindStore.Changed += RebuildCache;
	}

	private void OnDestroy()
	{
		BindStore.Changed -= RebuildCache;
	}

	private void RebuildCache()
	{
		_binds = BindStore.GetAll();
	}

	private void Update()
	{
		if (_terminalUI == null || _terminalUI.Terminal == null)
		{
		 	return;
		}

		if (GameTerminalUI.IsFocused)
		{
			return;
		}

		var keyboard = Keyboard.current;

		if (keyboard == null)
		{
			return;
		}

		foreach (var (key, command) in _binds)
		{
			if (keyboard[key].wasPressedThisFrame)
			{
				_terminalUI.Terminal.Submit(command);
			}
		}
	}
}
}
