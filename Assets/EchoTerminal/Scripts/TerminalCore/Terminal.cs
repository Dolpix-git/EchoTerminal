using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoTerminal
{
public class Terminal
{
	public event Action OnCleared;
	public event Action<TerminalEntry> OnEntryAdded;
	private readonly List<TerminalEntry> _entries = new();
	private readonly int _maxEntries;

	public Terminal(int maxEntries = 1000)
	{
		_maxEntries = maxEntries;
		var parser = new CommandParser();
		CommandProcessor = new(this, parser);
		Highlighter = new(parser);
		SuggestionAnalyzer = new(parser);
		HintAnalyzer = new(parser);
	}

	public IReadOnlyList<TerminalEntry> Entries => _entries;

	private CommandProcessor CommandProcessor { get; }
	private CommandHighlighter Highlighter { get; }
	private SuggestionAnalyzer SuggestionAnalyzer { get; }
	private HintAnalyzer HintAnalyzer { get; }

	public void Submit(string input)
	{
		Log(input, new Color(0.6f, 0.8f, 1f));
		CommandProcessor.Execute(input);
	}

	public string Highlight(string input)
	{
		return Highlighter.Highlight(input);
	}

	public AutocompleteContext GetSuggestions(string input)
	{
		return SuggestionAnalyzer.GetSuggestions(input);
	}

	public List<string> GetHints(string input)
	{
		return HintAnalyzer.GetHints(input);
	}

	public void Log(string text, Color? color = null)
	{
		var entry = new TerminalEntry(text, color ?? Color.white);

		if (_entries.Count >= _maxEntries)
		{
			_entries.RemoveAt(0);
		}

		_entries.Add(entry);
		OnEntryAdded?.Invoke(entry);
	}

	public void Clear()
	{
		_entries.Clear();
		OnCleared?.Invoke();
	}
}
}