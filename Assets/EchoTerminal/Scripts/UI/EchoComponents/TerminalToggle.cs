using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalToggle : IEchoComponent
{
	private readonly InputAction _toggleAction;
	private readonly VisualElement _window;

	public TerminalToggle(VisualElement root, InputActionAsset inputActions)
	{
		_window = root.Q<VisualElement>("game-window");

		if (_window == null || inputActions == null)
		{
			return;
		}

		_window.style.display = DisplayStyle.None;

		_toggleAction = inputActions.FindActionMap("Terminal")?.FindAction("Toggle");

		if (_toggleAction == null)
		{
			Debug.LogWarning("[EchoTerminal] Terminal/Toggle action not found in InputActionAsset.");
			return;
		}

		_toggleAction.performed += OnToggle;
		_toggleAction.Enable();
	}

	private void OnToggle(InputAction.CallbackContext ctx)
	{
		if (_window == null)
		{
			return;
		}

		var isHidden = _window.resolvedStyle.display == DisplayStyle.None;
		_window.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
	}

	~TerminalToggle()
	{
		if (_toggleAction == null)
		{
			return;
		}

		_toggleAction.performed -= OnToggle;
		_toggleAction.Disable();
	}
}
}