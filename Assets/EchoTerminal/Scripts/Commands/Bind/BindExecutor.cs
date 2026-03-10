using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoTerminal
{
public class BindExecutor : MonoBehaviour
{
	private Dictionary<Key, string> _binds;
	private GameTerminalUI _terminalUI;

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

	private void OnDestroy()
	{
		BindStore.Changed -= RebuildCache;
	}

	public void Init(GameTerminalUI terminalUI)
	{
		_terminalUI = terminalUI;
		RebuildCache();
		BindStore.Changed += RebuildCache;
	}

	private void RebuildCache()
	{
		_binds = BindStore.GetAll();
	}
}
}