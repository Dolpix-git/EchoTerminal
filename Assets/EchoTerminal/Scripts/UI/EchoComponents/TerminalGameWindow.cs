using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalGameWindow : IEchoComponent
{
	public TerminalGameWindow(VisualElement root, TerminalCursorSet cursorSet = null, TerminalDragConstraints dragConstraints = default)
	{
		var window = root.Q<VisualElement>("game-window");

		if (window == null)
		{
			return;
		}

		var titleBar = window.Q<VisualElement>("title-bar");

		if (titleBar != null)
		{
			if (cursorSet?.Move != null)
			{
				titleBar.AddManipulator(new TerminalHoverCursorManipulator(cursorSet.Move, cursorSet.Hotspot));
			}

			titleBar.AddManipulator(new TerminalDragManipulator(window, dragConstraints));
		}

		var closeBtn = window.Q<Button>("close-button");
		closeBtn?.RegisterCallback<ClickEvent>(_ => window.style.display = DisplayStyle.None);

		if (cursorSet?.Move != null)
		{
			closeBtn?.AddManipulator(new TerminalHoverCursorManipulator(null, Vector2.zero, cursorSet.Move, cursorSet.Hotspot));
		}

		WireResize(
			window,
			"resize-right",
			ResizeDirection.Right,
			cursorSet?.ResizeEW,
			cursorSet
		);
		WireResize(
			window,
			"resize-bottom",
			ResizeDirection.Bottom,
			cursorSet?.ResizeNS,
			cursorSet
		);
		WireResize(
			window,
			"resize-left",
			ResizeDirection.Left,
			cursorSet?.ResizeEW,
			cursorSet
		);
		WireResize(
			window,
			"resize-corner-br",
			ResizeDirection.Right | ResizeDirection.Bottom,
			cursorSet?.ResizeNWSE,
			cursorSet
		);
		WireResize(
			window,
			"resize-corner-bl",
			ResizeDirection.Left | ResizeDirection.Bottom,
			cursorSet?.ResizeNESW,
			cursorSet
		);
	}

	private static void WireResize(
		VisualElement window,
		string handleName,
		ResizeDirection direction,
		Texture2D cursor,
		TerminalCursorSet cursorSet)
	{
		var handle = window.Q<VisualElement>(handleName);

		if (handle == null)
		{
			return;
		}

		if (cursor != null)
		{
			handle.AddManipulator(new TerminalHoverCursorManipulator(cursor, cursorSet.Hotspot));
		}

		handle.AddManipulator(new TerminalResizeManipulator(window, direction));
	}
}
}