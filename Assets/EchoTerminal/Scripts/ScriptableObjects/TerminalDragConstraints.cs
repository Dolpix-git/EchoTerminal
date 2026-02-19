using System;

namespace EchoTerminal
{
[Serializable]
public struct TerminalDragConstraints
{
	public TerminalEdgeConstraint Top;
	public TerminalEdgeConstraint Left;
	public TerminalEdgeConstraint Right;
	public TerminalEdgeConstraint Bottom;
}
}
