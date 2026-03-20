using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Components
{
public class TerminalCopyButton : IEchoComponent
{
	private static readonly Regex RichTextTags = new(@"<[^>]+>", RegexOptions.Compiled);

	private readonly Button _copyButton;
	private readonly Label _copyToast;
	private readonly VisualElement _logContainer;
	private readonly Terminal _terminal;

	public TerminalCopyButton(Terminal terminal, VisualElement root)
	{
		_terminal = terminal;
		_logContainer = root?.Q<VisualElement>("log-container");
		_copyButton = root?.Q<Button>("copy-button");

		_copyToast = new("Copied to clipboard");
		_copyToast.AddToClassList("terminal-copy-toast");
		_copyToast.pickingMode = PickingMode.Ignore;
		_copyToast.style.display = DisplayStyle.None;
		var toastParent = root?.Q<VisualElement>("terminal-content") ?? root;
		toastParent?.Add(_copyToast);

		_copyButton?.RegisterCallback<ClickEvent>(OnCopyClicked);
	}

	~TerminalCopyButton()
	{
		_copyButton?.UnregisterCallback<ClickEvent>(OnCopyClicked);
	}

	private void OnCopyClicked(ClickEvent evt)
	{
		var hideLog = _logContainer?.ClassListContains("filter-hide-log") ?? false;
		var hideWarning = _logContainer?.ClassListContains("filter-hide-warning") ?? false;
		var hideError = _logContainer?.ClassListContains("filter-hide-error") ?? false;
		var hideCommand = _logContainer?.ClassListContains("filter-hide-command") ?? false;
		var showTimestamp = _logContainer?.ClassListContains("timestamps-visible") ?? false;

		var sb = new StringBuilder();

		foreach (var entry in _terminal.Entries)
		{
			if (hideLog && entry.Kind == LogKind.Log)
			{
				continue;
			}

			if (hideWarning && entry.Kind == LogKind.Warning)
			{
				continue;
			}

			if (hideError && entry.Kind == LogKind.Error)
			{
				continue;
			}

			if (hideCommand && entry.Kind == LogKind.Command)
			{
				continue;
			}

			if (showTimestamp)
			{
				sb.Append($"[{entry.Timestamp:HH:mm:ss}] ");
			}

			sb.AppendLine(RichTextTags.Replace(entry.Text, ""));
		}

		GUIUtility.systemCopyBuffer = sb.ToString().TrimEnd();

		_copyToast.style.display = DisplayStyle.Flex;
		_copyToast.schedule.Execute(() => _copyToast.style.display = DisplayStyle.None).StartingIn(1500);
	}
}
}