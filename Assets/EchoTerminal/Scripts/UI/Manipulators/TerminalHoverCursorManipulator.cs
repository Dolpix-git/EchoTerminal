using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace EchoTerminal.Components
{
public class TerminalHoverCursorManipulator : PointerManipulator
{
	private readonly Texture2D _texture;
	private readonly Vector2 _hotspot;
	private readonly Texture2D _exitTexture;
	private readonly Vector2 _exitHotspot;
	private bool _hovered;
	private bool _pressed;

	public TerminalHoverCursorManipulator(Texture2D texture, Vector2 hotspot, Texture2D exitTexture = null, Vector2 exitHotspot = default)
	{
		_texture = texture;
		_hotspot = hotspot;
		_exitTexture = exitTexture;
		_exitHotspot = exitHotspot;
	}

	protected override void RegisterCallbacksOnTarget()
	{
		target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
		target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
		target.RegisterCallback<PointerDownEvent>(OnPointerDown);
		target.RegisterCallback<PointerUpEvent>(OnPointerUp);
	}

	protected override void UnregisterCallbacksFromTarget()
	{
		target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
		target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
		target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
		target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
	}

	private void OnPointerEnter(PointerEnterEvent evt)
	{
		_hovered = true;
		Cursor.SetCursor(_texture, _hotspot, CursorMode.Auto);
	}

	private void OnPointerLeave(PointerLeaveEvent evt)
	{
		_hovered = false;
		if (!_pressed)
		{
			Cursor.SetCursor(_exitTexture, _exitHotspot, CursorMode.Auto);
		}
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		if (evt.button == 0)
		{
			_pressed = true;
		}
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		if (evt.button != 0)
		{
			return;
		}

		_pressed = false;

		if (_hovered)
		{
			Cursor.SetCursor(_texture, _hotspot, CursorMode.Auto);
		}
		else
		{
			Cursor.SetCursor(_exitTexture, _exitHotspot, CursorMode.Auto);
		}
	}
}
}