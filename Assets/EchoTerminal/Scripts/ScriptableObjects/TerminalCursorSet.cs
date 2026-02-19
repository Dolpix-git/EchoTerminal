using UnityEngine;

namespace EchoTerminal
{
[CreateAssetMenu(fileName = "TerminalCursorSet", menuName = "Echo Terminal/Cursor Set")]
public class TerminalCursorSet : ScriptableObject
{
	[Header("Cursors")]
	[SerializeField] private Texture2D _move;
	[SerializeField] private Texture2D _resizeEW;
	[SerializeField] private Texture2D _resizeNS;
	[SerializeField] private Texture2D _resizeNWSE;
	[SerializeField] private Texture2D _resizeNESW;

	[Header("Hotspot")]
	[Tooltip("Pixel offset from the top-left of the cursor texture to its active point. For a centered 64x64 cursor use (32, 32).")]
	[SerializeField] private Vector2 _hotspot = new(32f, 32f);

	public Texture2D Move       => _move;
	public Texture2D ResizeEW   => _resizeEW;
	public Texture2D ResizeNS   => _resizeNS;
	public Texture2D ResizeNWSE => _resizeNWSE;
	public Texture2D ResizeNESW => _resizeNESW;
	public Vector2   Hotspot    => _hotspot;
}
}
