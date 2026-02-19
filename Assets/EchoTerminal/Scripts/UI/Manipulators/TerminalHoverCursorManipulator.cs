using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace EchoTerminal.Components
{
public class TerminalHoverCursorManipulator : PointerManipulator
{
	private readonly Vector2 _hotspot;
	private readonly Texture2D _texture;
	private bool _hovered;
	private bool _pressed;

	public TerminalHoverCursorManipulator(Texture2D texture, Vector2 hotspot)
	{
		_texture = texture;
		_hotspot = hotspot;
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
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}
	}
}
}