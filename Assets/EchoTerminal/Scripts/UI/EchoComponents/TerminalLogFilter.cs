using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalLogFilter : IEchoComponent
{
	private const string _activeClass = "terminal-toolbar-toggle--active";
	private readonly Button _filterCommand;
	private readonly Button _filterError;

	private readonly Button _filterLog;
	private readonly Button _filterWarning;
	private readonly VisualElement _logContainer;
	private readonly Terminal _terminal;
	private int _countCommand;
	private int _countError;

	private int _countLog;
	private int _countWarning;
	private bool _showCommand = true;
	private bool _showError = true;

	private bool _showLog = true;
	private bool _showWarning = true;

	public TerminalLogFilter(Terminal terminal, VisualElement root)
	{
		_terminal = terminal;
		_logContainer = root?.Q<VisualElement>("log-container");
		_filterLog = root?.Q<Button>("filter-log");
		_filterWarning = root?.Q<Button>("filter-warning");
		_filterError = root?.Q<Button>("filter-error");
		_filterCommand = root?.Q<Button>("filter-command");

		_filterLog?.RegisterCallback<ClickEvent>(_ => Toggle(LogKind.Log));
		_filterWarning?.RegisterCallback<ClickEvent>(_ => Toggle(LogKind.Warning));
		_filterError?.RegisterCallback<ClickEvent>(_ => Toggle(LogKind.Error));
		_filterCommand?.RegisterCallback<ClickEvent>(_ => Toggle(LogKind.Command));

		_terminal.OnEntryAdded += OnEntryAdded;
		_terminal.OnCleared += OnCleared;

		RefreshAll();
	}

	~TerminalLogFilter()
	{
		_terminal.OnEntryAdded -= OnEntryAdded;
		_terminal.OnCleared -= OnCleared;
	}

	private void Toggle(LogKind kind)
	{
		switch (kind)
		{
			case LogKind.Log:
				_showLog = !_showLog;
				break;
			case LogKind.Warning:
				_showWarning = !_showWarning;
				break;
			case LogKind.Error:
				_showError = !_showError;
				break;
			case LogKind.Command:
				_showCommand = !_showCommand;
				break;
		}

		_logContainer?.EnableInClassList($"filter-hide-{kind.ToString().ToLower()}", !IsVisible(kind));
		UpdateButton(kind);
	}

	private bool IsVisible(LogKind kind)
	{
		return kind switch
		{
			LogKind.Log     => _showLog,
			LogKind.Warning => _showWarning,
			LogKind.Error   => _showError,
			LogKind.Command => _showCommand,
			_               => true
		};
	}

	private void UpdateButton(LogKind kind)
	{
		var (button, label, count) = kind switch
		{
			LogKind.Log     => (_filterLog, "Log", _countLog),
			LogKind.Warning => (_filterWarning, "Warn", _countWarning),
			LogKind.Error   => (_filterError, "Err", _countError),
			LogKind.Command => (_filterCommand, "Cmd", _countCommand),
			_               => (null, "", 0)
		};

		if (button == null)
		{
			return;
		}

		button.text = $"{label} {count}";
		button.EnableInClassList(_activeClass, IsVisible(kind));
	}

	private void RefreshAll()
	{
		UpdateButton(LogKind.Log);
		UpdateButton(LogKind.Warning);
		UpdateButton(LogKind.Error);
		UpdateButton(LogKind.Command);
	}

	private void OnEntryAdded(TerminalEntry entry)
	{
		switch (entry.Kind)
		{
			case LogKind.Log:
				_countLog++;
				break;
			case LogKind.Warning:
				_countWarning++;
				break;
			case LogKind.Error:
				_countError++;
				break;
			case LogKind.Command:
				_countCommand++;
				break;
		}

		UpdateButton(entry.Kind);
	}

	private void OnCleared()
	{
		_countLog = _countWarning = _countError = _countCommand = 0;
		RefreshAll();
	}
}
}