using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalAutoScroll : IEchoComponent
{
	private readonly ScrollView _scrollView;
	private readonly Terminal _terminal;

	public TerminalAutoScroll(Terminal terminal, VisualElement root)
	{
		_terminal = terminal;
		_scrollView = root?.Q<ScrollView>("log-scroll");

		if (_scrollView == null)
		{
			return;
		}

		_terminal.OnEntryAdded += OnEntryAdded;
	}

	~TerminalAutoScroll()
	{
		_terminal.OnEntryAdded -= OnEntryAdded;
	}

	private void OnEntryAdded(TerminalEntry entry)
	{
		_scrollView?.schedule.Execute(() => _scrollView.scrollOffset = new(0, float.MaxValue));
	}
}
}
