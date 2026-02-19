using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalDragManipulator : PointerManipulator
{
	private readonly VisualElement _windowElement;
	private bool _dragging;
	private Vector2 _startPointer;
	private Vector2 _startPosition;

	public TerminalDragManipulator(VisualElement windowElement)
	{
		_windowElement = windowElement;
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
		_startPosition = new(
			_windowElement.resolvedStyle.left,
			_windowElement.resolvedStyle.top
		);
		_dragging = true;
		target.CapturePointer(evt.pointerId);
		_windowElement.BringToFront();
		evt.StopPropagation();
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		if (!_dragging || !target.HasPointerCapture(evt.pointerId))
		{
			return;
		}

		var delta = (Vector2)evt.position - _startPointer;
		_windowElement.style.left = _startPosition.x + delta.x;
		_windowElement.style.top = _startPosition.y + delta.y;
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		if (!_dragging)
		{
			return;
		}

		_dragging = false;
		target.ReleasePointer(evt.pointerId);
	}
}
}