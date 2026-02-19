#if UNITY_EDITOR
using EchoTerminal.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Editor
{
public class EditorTerminalUI : EditorWindow
{
	private TerminalView _view;

	private void OnDestroy()
	{
		_view = null;
	}

	private void CreateGUI()
	{
		var assets = EchoTerminalAssets.Instance;
		if (assets == null || assets.Uxml == null || assets.Config == null)
		{
			Debug.LogError("[EchoTerminal] Create EchoTerminalAssets via Create > Echo Terminal > Assets");
			rootVisualElement.Add(new Label("Error: EchoTerminalAssets not found. See console."));
			return;
		}

		assets.Uxml.CloneTree(rootVisualElement);

		if (assets.StyleSheet != null)
		{
			rootVisualElement.styleSheets.Add(assets.StyleSheet);
		}

		_view = new(rootVisualElement, assets.Config);
	}

	[MenuItem("Window/Echo Terminal")]
	public static EditorTerminalUI Open()
	{
		var window = GetWindow<EditorTerminalUI>("Echo Terminal");
		window.minSize = new(300, 150);
		return window;
	}
}
}
#endif