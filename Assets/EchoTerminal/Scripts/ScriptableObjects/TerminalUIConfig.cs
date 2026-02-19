using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal
{
[CreateAssetMenu(fileName = "TerminalUIConfig", menuName = "Echo Terminal/UI Config")]
public class TerminalUIConfig : ScriptableObject
{
	[Header("Templates")]
	[SerializeField] private VisualTreeAsset _logEntryTemplate;
	[SerializeField] private VisualTreeAsset _suggestionPopupTemplate;
	[SerializeField] private VisualTreeAsset _suggestionItemTemplate;
	[SerializeField] private VisualTreeAsset _hintItemTemplate;

	[Header("Cursors")]
	[SerializeField] private TerminalCursorSet _cursorSet;
	[SerializeField] private TerminalDragConstraints _dragConstraints;

	public VisualTreeAsset LogEntryTemplate => _logEntryTemplate;
	public VisualTreeAsset SuggestionPopupTemplate => _suggestionPopupTemplate;
	public VisualTreeAsset SuggestionItemTemplate => _suggestionItemTemplate;
	public VisualTreeAsset HintItemTemplate => _hintItemTemplate;
	public TerminalCursorSet CursorSet => _cursorSet;
	public TerminalDragConstraints DragConstraints => _dragConstraints;
}
}
