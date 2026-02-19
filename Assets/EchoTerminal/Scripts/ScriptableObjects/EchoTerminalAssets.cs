#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Editor
{
[CreateAssetMenu(fileName = "EchoTerminalAssets", menuName = "Echo Terminal/Assets")]
public class EchoTerminalAssets : ScriptableObject
{
	private static EchoTerminalAssets _instance;
	[SerializeField] private VisualTreeAsset _uxml;
	[SerializeField] private StyleSheet _styleSheet;
	[SerializeField] private TerminalUIConfig _config;

	public VisualTreeAsset Uxml => _uxml;
	public StyleSheet StyleSheet => _styleSheet;
	public TerminalUIConfig Config => _config;

	public static EchoTerminalAssets Instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}

			var guids = AssetDatabase.FindAssets("t:EchoTerminalAssets");
			if (guids.Length == 0)
			{
				return null;
			}

			var path = AssetDatabase.GUIDToAssetPath(guids[0]);
			_instance = AssetDatabase.LoadAssetAtPath<EchoTerminalAssets>(path);
			return _instance;
		}
	}
}
}
#endif