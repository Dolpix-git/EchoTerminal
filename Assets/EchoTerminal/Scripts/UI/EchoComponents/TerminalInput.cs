using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalInput : IEchoComponent
{
	private readonly Label _highlight;
	private readonly TextField _inputField;
	private readonly TerminalSuggestionPopup _popup;
	private readonly Terminal _terminal;

	public TerminalInput(Terminal terminal, VisualElement root, TerminalUIConfig config)
	{
		_terminal = terminal;
		_inputField = root?.Q<TextField>("input-field");

		if (_inputField == null)
		{
			return;
		}

		_inputField.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

		_highlight = root.Q<Label>("highlight-label");
		_highlight.enableRichText = true;

		var textElement = _inputField.Q<TextElement>();
		textElement.style.color = Color.clear;

		_popup = new(_inputField.parent, config);

		_inputField.RegisterValueChangedCallback(OnValueChanged);
	}

	~TerminalInput()
	{
		_inputField?.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
		_inputField?.UnregisterValueChangedCallback(OnValueChanged);
	}

	private void OnValueChanged(ChangeEvent<string> evt)
	{
		if (_highlight == null || evt.target != _inputField)
		{
			return;
		}

		// ToDo Reintroduce when we have static highlight, suggestion, or hints class.
		_highlight.text = evt.newValue;
		// _highlight.text = _terminal.Highlight(evt.newValue);
		// _popup.Update(
		// 	_terminal.GetSuggestions(evt.newValue),
		// 	_terminal.GetHints(evt.newValue)
		// );
	}

	private void OnKeyDown(KeyDownEvent evt)
	{
		if (_popup is { HasSuggestions: true })
		{
			switch (evt.keyCode)
			{
				case KeyCode.Tab or KeyCode.Return or KeyCode.KeypadEnter:
				{
					var result = _popup.AcceptSuggestion(_inputField.value);
					_inputField.value = result;
					_inputField.schedule.Execute(() =>
					{
						_inputField.Focus();
						_inputField.SelectRange(result.Length, result.Length);
					});

					evt.StopImmediatePropagation();
					evt.StopPropagation();
					return;
				}
				case KeyCode.UpArrow:
					_popup.MoveSelection(-1);
					evt.StopImmediatePropagation();
					evt.StopPropagation();
					return;
				case KeyCode.DownArrow:
					_popup.MoveSelection(1);
					evt.StopImmediatePropagation();
					evt.StopPropagation();
					return;
				case KeyCode.Escape:
					_popup.Hide();
					evt.StopImmediatePropagation();
					evt.StopPropagation();
					return;
			}
		}

		if (evt.keyCode == KeyCode.Tab)
		{
			// ToDo Reintroduce when we have static highlight, suggestion, or hints class.
			// _popup.Update(
			// 	_terminal.GetSuggestions(_inputField.value),
			// 	_terminal.GetHints(_inputField.value)
			// );
			evt.StopImmediatePropagation();
			evt.StopPropagation();
			return;
		}

		if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
		{
			return;
		}

		var text = _inputField.value;
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}

		_terminal.Submit(text);
		_inputField.value = "";

		evt.StopImmediatePropagation();
		evt.StopPropagation();

		_inputField.schedule.Execute(() => _inputField.Focus());
	}
}
}