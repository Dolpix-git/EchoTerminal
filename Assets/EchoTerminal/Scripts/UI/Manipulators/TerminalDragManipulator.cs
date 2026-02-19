using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalDragManipulator : PointerManipulator
{
	private readonly TerminalDragConstraints _constraints;
	private readonly VisualElement _windowElement;
	private bool _dragging;
	private Vector2 _startPointer;
	private Vector2 _startPosition;

	public TerminalDragManipulator(VisualElement windowElement, TerminalDragConstraints constraints = default)
	{
		_windowElement = windowElement;
		_constraints = constraints;
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
		var newLeft = _startPosition.x + delta.x;
		var newTop = _startPosition.y + delta.y;

		var parent = _windowElement.parent;
		if (parent != null && parent.layout.width > 0f && parent.layout.height > 0f)
		{
			ApplyConstraints(ref newLeft, ref newTop, parent.layout.width, parent.layout.height);
		}

		_windowElement.style.left = newLeft;
		_windowElement.style.top = newTop;
	}

	private void ApplyConstraints(ref float left, ref float top, float screenW, float screenH)
	{
		var winW = _windowElement.resolvedStyle.width;
		var winH = _windowElement.resolvedStyle.height;

		if (left > 0f && left < _constraints.Left.SnapDistance)
		{
			left = 0f;
		}

		if (top > 0f && top < _constraints.Top.SnapDistance)
		{
			top = 0f;
		}

		var rightGap = screenW - (left + winW);
		if (rightGap > 0f && rightGap < _constraints.Right.SnapDistance)
		{
			left = screenW - winW;
		}

		var bottomGap = screenH - (top + winH);
		if (bottomGap > 0f && bottomGap < _constraints.Bottom.SnapDistance)
		{
			top = screenH - winH;
		}

		top = Mathf.Max(0f, top);
		left = Mathf.Clamp(
			left,
			-(_constraints.Left.Overhang * winW),
			screenW - winW + _constraints.Right.Overhang * winW
		);

		top = Mathf.Min(top, screenH - winH + _constraints.Bottom.Overhang * winH);
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