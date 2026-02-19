using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalGameWindow : IEchoComponent
{
	public TerminalGameWindow(VisualElement root)
	{
		var window = root.Q<VisualElement>("game-window");

		if (window == null)
		{
			return;
		}

		var titleBar = window.Q<VisualElement>("title-bar");

		if (titleBar != null)
		{
			titleBar.AddManipulator(new TerminalDragManipulator(window));
		}

		var closeBtn = window.Q<Button>("close-button");
		closeBtn?.RegisterCallback<ClickEvent>(_ => window.style.display = DisplayStyle.None);

		WireResize(window, "resize-right", ResizeDirection.Right);
		WireResize(window, "resize-bottom", ResizeDirection.Bottom);
		WireResize(window, "resize-left", ResizeDirection.Left);
		WireResize(window, "resize-corner-br", ResizeDirection.Right | ResizeDirection.Bottom);
		WireResize(window, "resize-corner-bl", ResizeDirection.Left | ResizeDirection.Bottom);
	}

	private static void WireResize(VisualElement window, string handleName, ResizeDirection direction)
	{
		var handle = window.Q<VisualElement>(handleName);
		handle?.AddManipulator(new TerminalResizeManipulator(window, direction));
	}
}
}
