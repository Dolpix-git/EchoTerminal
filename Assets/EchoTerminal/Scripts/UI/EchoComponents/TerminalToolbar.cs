using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalToolbar : IEchoComponent
{
	private const string _timestampsVisibleClass = "timestamps-visible";
	private const string _activeClass = "terminal-toolbar-toggle--active";

	private readonly Button _clearButton;
	private readonly VisualElement _logContainer;
	private readonly Button _scrollBottomButton;
	private readonly ScrollView _scrollView;
	private readonly Terminal _terminal;
	private readonly Button _timestampsToggle;

	private bool _timestampsEnabled;

	public TerminalToolbar(Terminal terminal, VisualElement root)
	{
		_terminal = terminal;
		_logContainer = root?.Q<VisualElement>("log-container");
		_scrollView = root?.Q<ScrollView>("log-scroll");
		_clearButton = root?.Q<Button>("clear-button");
		_timestampsToggle = root?.Q<Button>("timestamps-toggle");
		_scrollBottomButton = root?.Q<Button>("scroll-bottom");

		_clearButton?.RegisterCallback<ClickEvent>(OnClearClicked);
		_timestampsToggle?.RegisterCallback<ClickEvent>(OnTimestampsClicked);
		_scrollBottomButton?.RegisterCallback<ClickEvent>(OnScrollBottomClicked);

		OnTimestampsClicked(null);
	}

	~TerminalToolbar()
	{
		_clearButton?.UnregisterCallback<ClickEvent>(OnClearClicked);
		_timestampsToggle?.UnregisterCallback<ClickEvent>(OnTimestampsClicked);
		_scrollBottomButton?.UnregisterCallback<ClickEvent>(OnScrollBottomClicked);
	}

	private void OnClearClicked(ClickEvent evt)
	{
		_terminal.Clear();
	}

	private void OnTimestampsClicked(ClickEvent evt)
	{
		_timestampsEnabled = !_timestampsEnabled;
		_timestampsToggle?.EnableInClassList(_activeClass, _timestampsEnabled);
		_logContainer?.EnableInClassList(_timestampsVisibleClass, _timestampsEnabled);
	}

	private void OnScrollBottomClicked(ClickEvent evt)
	{
		_scrollView?.schedule.Execute(() => _scrollView.scrollOffset = new(0, float.MaxValue));
	}
}
}