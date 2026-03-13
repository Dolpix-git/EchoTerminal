using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoTerminal
{
[CreateAssetMenu(fileName = "TerminalHighlightColors", menuName = "Echo Terminal/Highlight Colors")]
public class TerminalHighlightColors : ScriptableObject
{
	[Header("Command Colors")] [SerializeField]
	private Color _commandColor = new(0.67f, 0.87f, 1f);

	[SerializeField] private Color _unknownColor = new(1f, 0.27f, 0.27f);
	[SerializeField] private Color _fallbackParamColor = new(1f, 0.87f, 0.53f);

	[Header("Type Colors")] [SerializeField]
	private Color _numberColor = new(1f, 0.72f, 0.42f);

	[SerializeField] private Color _stringColor = new(0.95f, 0.98f, 0.55f);
	[SerializeField] private Color _boolColor = new(1f, 0.47f, 0.78f);
	[SerializeField] private Color _vectorColor = new(0.55f, 0.91f, 0.99f);
	[SerializeField] private Color _gameObjectColor = new(0.31f, 0.98f, 0.48f);

	public Color CommandColor => _commandColor;
	public Color UnknownColor => _unknownColor;
	public Color FallbackParamColor => _fallbackParamColor;

	private Dictionary<Type, Color> _typeColors;

	public IReadOnlyDictionary<Type, Color> TypeColors =>
		_typeColors ??= new()
		{
			{ typeof(GameObject), _gameObjectColor },
			{ typeof(bool), _boolColor },
			{ typeof(Vector3), _vectorColor },
			{ typeof(float), _numberColor },
			{ typeof(string), _stringColor },
		};

	private void OnValidate()
	{
		_typeColors = null;
	}
}
}