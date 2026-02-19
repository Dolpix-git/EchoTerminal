using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalLogDisplay : IEchoComponent
{
	private readonly VisualElement _container;
	private readonly VisualTreeAsset _entryTemplate;
	private readonly Terminal _terminal;
	private int _entryCount;

	public TerminalLogDisplay(Terminal terminal, VisualElement root, TerminalUIConfig config)
	{
		_terminal = terminal;
		_container = root?.Q<VisualElement>("log-container");
		_entryTemplate = config.LogEntryTemplate;

		if (_container == null)
		{
			return;
		}

		_terminal.OnEntryAdded += OnEntryAdded;
		_terminal.OnCleared += OnCleared;
		Refresh();
	}

	~TerminalLogDisplay()
	{
		_terminal.OnEntryAdded -= OnEntryAdded;
		_terminal.OnCleared -= OnCleared;
	}

	private void OnEntryAdded(TerminalEntry entry)
	{
		AddEntryElement(entry);
	}

	private void OnCleared()
	{
		_container?.Clear();
		_entryCount = 0;
	}

	private void Refresh()
	{
		_container?.Clear();
		_entryCount = 0;

		foreach (var entry in _terminal.Entries)
		{
			AddEntryElement(entry);
		}
	}

	private void AddEntryElement(TerminalEntry entry)
	{
		if (_container == null)
		{
			return;
		}

		var clone = _entryTemplate.CloneTree();
		var row = clone.Q<VisualElement>(className: "terminal-entry");
		row.AddToClassList(_entryCount % 2 == 0 ? "terminal-entry--even" : "terminal-entry--odd");

		var timestamp = clone.Q<Label>("timestamp");
		timestamp.text = entry.Timestamp.ToString("HH:mm:ss");

		var message = clone.Q<Label>("message");
		message.enableRichText = true;
		var hex = ColorUtility.ToHtmlStringRGB(entry.Color);
		message.text = $"<color=#{hex}>{entry.Text}</color>";

		_container.Add(clone);
		_entryCount++;
	}
}
}