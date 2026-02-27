using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalSuggestionPopup
{
	private readonly VisualElement _container;
	private readonly VisualElement _hintContainer;
	private readonly VisualTreeAsset _hintItemTemplate;
	private readonly VisualElement _listContainer;
	private readonly VisualTreeAsset _suggestionItemTemplate;
	private AutocompleteContext _context;
	private int _selectedIndex;

	private List<string> _suggestions;

	public TerminalSuggestionPopup(VisualElement parent, TerminalUIConfig config)
	{
		var clone = config.SuggestionPopupTemplate.CloneTree();
		_container = clone.Q<VisualElement>("suggestion-popup");
		_listContainer = _container.Q<VisualElement>("suggestion-list");
		_hintContainer = _container.Q<VisualElement>("hint-list");
		parent.Add(_container);

		_suggestionItemTemplate = config.SuggestionItemTemplate;
		_hintItemTemplate = config.HintItemTemplate;
	}

	public bool HasSuggestions => _suggestions is { Count: > 0 };

	public void Update(AutocompleteContext context, List<string> hints)
	{
		_context = context;
		var hasSuggestions = context.Suggestions != null && context.Suggestions.Count > 0;
		var hasHints = hints != null && hints.Count > 0;

		if (!hasSuggestions && !hasHints)
		{
			Hide();
			return;
		}

		_listContainer.Clear();

		if (hasSuggestions)
		{
			_suggestions = context.Suggestions;
			_selectedIndex = 0;

			for (var i = 0; i < _suggestions.Count; i++)
			{
				var itemClone = _suggestionItemTemplate.CloneTree();
				var label = itemClone.Q<Label>("suggestion-label");
				label.text = _suggestions[i];

				if (i == 0)
				{
					label.AddToClassList("terminal-suggestion-item--selected");
				}

				_listContainer.Add(itemClone);
			}
		}
		else
		{
			_suggestions = null;
			_selectedIndex = -1;
		}

		_hintContainer.Clear();

		if (hasHints)
		{
			foreach (var hint in hints)
			{
				var hintClone = _hintItemTemplate.CloneTree();
				var label = hintClone.Q<Label>("hint-label");
				label.text = hint;
				_hintContainer.Add(hintClone);
			}
		}

		_container.style.display = DisplayStyle.Flex;
	}

	public void MoveSelection(int delta)
	{
		if (_suggestions == null || _suggestions.Count == 0)
		{
			return;
		}

		var prev = _selectedIndex;
		_selectedIndex += delta;

		if (_selectedIndex < 0)
		{
			_selectedIndex = _suggestions.Count - 1;
		}
		else if (_selectedIndex >= _suggestions.Count)
		{
			_selectedIndex = 0;
		}

		if (prev >= 0 && prev < _listContainer.childCount)
		{
			_listContainer[prev]
				.Q<Label>("suggestion-label")
				?.RemoveFromClassList("terminal-suggestion-item--selected");
		}

		if (_selectedIndex >= 0 && _selectedIndex < _listContainer.childCount)
		{
			_listContainer[_selectedIndex]
				.Q<Label>("suggestion-label")
				?.AddToClassList("terminal-suggestion-item--selected");
		}
	}

	public string AcceptSuggestion(string input)
	{
		if (_suggestions == null || _selectedIndex < 0 || _selectedIndex >= _suggestions.Count)
		{
			return input;
		}

		var selected = _suggestions[_selectedIndex];
		var before = input.Substring(0, _context.ReplaceStart);
		var after = _context.ReplaceEnd < input.Length ? input.Substring(_context.ReplaceEnd) : "";
		var result = before + selected + " " + after;

		Hide();
		return result;
	}

	public void Hide()
	{
		_container.style.display = DisplayStyle.None;
		_listContainer.Clear();
		_hintContainer.Clear();
		_suggestions = null;
		_selectedIndex = -1;
	}
}
}