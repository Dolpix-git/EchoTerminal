using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
[Flags]
public enum ResizeDirection
{
	Right = 1,
	Bottom = 2,
	Left = 4
}

public class TerminalResizeManipulator : PointerManipulator
{
	private const float _minWidth = 300f;
	private const float _minHeight = 200f;
	private readonly ResizeDirection _direction;

	private readonly VisualElement _windowElement;
	private bool _resizing;
	private float _startHeight;
	private float _startLeft;
	private Vector2 _startPointer;
	private float _startWidth;

	public TerminalResizeManipulator(VisualElement windowElement, ResizeDirection direction)
	{
		_windowElement = windowElement;
		_direction = direction;
	}

	protected override void RegisterCallbacksOnTarget()
	{
		target.RegisterCallback<PointerDownEvent>(OnPointerDown);
		target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
		target.RegisterCallback<PointerUpEvent>(OnPointerUp);
	}

	protected override void UnregisterCallbacksFromTarget()
	{
		target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
		target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
		target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		if (evt.button != 0)
		{
			return;
		}

		_startPointer = evt.position;
		_startWidth = _windowElement.resolvedStyle.width;
		_startHeight = _windowElement.resolvedStyle.height;
		_startLeft = _windowElement.resolvedStyle.left;
		_resizing = true;
		target.CapturePointer(evt.pointerId);
		evt.StopPropagation();
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		if (!_resizing || !target.HasPointerCapture(evt.pointerId))
		{
			return;
		}

		var delta = (Vector2)evt.position - _startPointer;

		if (_direction.HasFlag(ResizeDirection.Right))
		{
			_windowElement.style.width = Mathf.Max(_minWidth, _startWidth + delta.x);
		}

		if (_direction.HasFlag(ResizeDirection.Bottom))
		{
			_windowElement.style.height = Mathf.Max(_minHeight, _startHeight + delta.y);
		}

		if (!_direction.HasFlag(ResizeDirection.Left))
		{
			return;
		}

		var newWidth = Mathf.Max(_minWidth, _startWidth - delta.x);
		var actualDelta = _startWidth - newWidth;
		_windowElement.style.width = newWidth;
		_windowElement.style.left = _startLeft + actualDelta;
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		if (!_resizing)
		{
			return;
		}

		_resizing = false;
		target.ReleasePointer(evt.pointerId);
	}
}
}