using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoTerminal
{
public class Terminal
{
	public CommandRegistry Registry { get; }
	public CommandHighlight Highlighter { get; }
	public CommandSuggest Suggester { get; }
	public CommandHints Hints { get; }
	public CommandParser Parser { get; }
	public CommandExecutor Executor { get; }

	public IReadOnlyList<TerminalEntry> Entries => _entries;

	public event Action OnCleared;
	public event Action<TerminalEntry> OnEntryAdded;

	private readonly List<TerminalEntry> _entries = new();
	private readonly int _maxEntries;

	public Terminal(TerminalHighlightColors highlightColors, int maxEntries = 1000)
	{
		_maxEntries = maxEntries;
		Registry = new();
		Registry.Scan();
		Parser = new(Registry);
		Executor = new(this, Parser);
		Highlighter = new(Parser, highlightColors);
		Suggester = new(Parser);
		Hints = new(Parser, highlightColors);
	}

	public void Submit(string input)
	{
		Log(Highlighter.Highlight(input));
		Executor.Execute(input);
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