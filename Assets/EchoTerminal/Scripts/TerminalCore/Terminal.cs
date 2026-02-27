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
	private readonly CommandProcessor _processor;

	public Terminal(int maxEntries = 1000)
	{
		_maxEntries = maxEntries;
		_processor = new(this);
	}

	public IReadOnlyList<TerminalEntry> Entries => _entries;

	public void Submit(string input)
	{
		Log(input, new Color(0.6f, 0.8f, 1f));
		_processor.Execute(input);
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