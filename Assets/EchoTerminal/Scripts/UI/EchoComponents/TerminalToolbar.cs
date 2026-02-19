using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalToolbar : IEchoComponent
{
	private const string _timestampsVisibleClass = "timestamps-visible";
	private readonly Button _clearButton;
	private readonly VisualElement _logContainer;

	private readonly Terminal _terminal;
	private readonly Button _timestampsToggle;
	private bool _timestampsEnabled;

	public TerminalToolbar(Terminal terminal, VisualElement root)
	{
		_terminal = terminal;

		_logContainer = root?.Q<VisualElement>("log-container");
		_clearButton = root?.Q<Button>("clear-button");
		_timestampsToggle = root?.Q<Button>("timestamps-toggle");

		_clearButton?.RegisterCallback<ClickEvent>(OnClearClicked);
		_timestampsToggle?.RegisterCallback<ClickEvent>(OnTimestampsClicked);
		OnTimestampsClicked(null);
	}

	~TerminalToolbar()
	{
		_clearButton?.UnregisterCallback<ClickEvent>(OnClearClicked);
		_timestampsToggle?.UnregisterCallback<ClickEvent>(OnTimestampsClicked);
	}

	private void OnClearClicked(ClickEvent evt)
	{
		_terminal.Clear();
	}

	private void OnTimestampsClicked(ClickEvent evt)
	{
		_timestampsEnabled = !_timestampsEnabled;

		if (_timestampsEnabled)
		{
			_timestampsToggle?.AddToClassList("terminal-toolbar-toggle--active");
			_logContainer?.AddToClassList(_timestampsVisibleClass);
		}
		else
		{
			_timestampsToggle?.RemoveFromClassList("terminal-toolbar-toggle--active");
			_logContainer?.RemoveFromClassList(_timestampsVisibleClass);
		}
	}
}
}