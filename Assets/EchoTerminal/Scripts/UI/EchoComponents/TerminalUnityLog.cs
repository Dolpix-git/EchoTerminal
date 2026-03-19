using UnityEngine;

namespace EchoTerminal.Components
{
public class TerminalUnityLog : IEchoComponent
{
	private readonly Terminal _terminal;

	public TerminalUnityLog(Terminal terminal)
	{
		_terminal = terminal;
		Application.logMessageReceived += OnLogMessageReceived;
	}

	~TerminalUnityLog()
	{
		Application.logMessageReceived -= OnLogMessageReceived;
	}

	private void OnLogMessageReceived(string message, string stackTrace, LogType type)
	{
		var (color, kind) = type switch
		{
			LogType.Error     => (new(1f, 0.3f, 0.3f), LogKind.Error),
			LogType.Exception => (new(1f, 0.3f, 0.3f), LogKind.Error),
			LogType.Warning   => (new(1f, 0.9f, 0.3f), LogKind.Warning),
			LogType.Assert    => (new Color(1f, 0.5f, 0.2f), LogKind.Warning),
			_                 => (new Color(0.7f, 0.7f, 0.7f), LogKind.Log)
		};

		_terminal.Log(message, color, kind);
	}
}
}