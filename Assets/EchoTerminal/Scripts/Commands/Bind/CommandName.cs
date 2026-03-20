namespace EchoTerminal
{
public readonly struct CommandName
{
	public readonly string Value;

	public CommandName(string value)
	{
		Value = value;
	}

	public override string ToString()
	{
		return $">{Value}<";
	}
}
}