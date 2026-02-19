using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalView
{
	public Terminal Terminal { get; }

	private readonly List<IEchoComponent> _components = new();

	public TerminalView(VisualElement root, TerminalUIConfig config)
	{
		Terminal = new();

		_components.Add(new TerminalLogDisplay(Terminal, root, config));
		_components.Add(new TerminalAutoScroll(Terminal, root));
		_components.Add(new TerminalInput(Terminal, root, config));
		_components.Add(new TerminalToolbar(Terminal, root));
		_components.Add(new TerminalUnityLog(Terminal));

		if (root.Q<VisualElement>("game-window") != null)
		{
			_components.Add(new TerminalGameWindow(root));
		}
	}

	public void AddComponent(IEchoComponent component)
	{
		_components.Add(component);
	}

	~TerminalView()
	{
		_components.Clear();
	}
}
}
