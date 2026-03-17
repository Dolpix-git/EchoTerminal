using System;
using System.Collections.Generic;
using System.Reflection;
using EchoTerminal.Scripts.Test;
using UnityEngine;

namespace EchoTerminal
{
public class CommandHints
{
	private static Dictionary<Type, IHintFormatter> _formatters;

	private readonly TerminalHighlightColors _colors;
	private readonly CommandParser _parser;

	public CommandHints(CommandParser parser, TerminalHighlightColors colors)
	{
		_parser = parser;
		_colors = colors;
	}

	private static IReadOnlyDictionary<Type, IHintFormatter> Formatters => GetFormatters();

	public List<string> GetHints(string input)
	{
		var result = _parser.Parse(input);

		if (!result.IsKnownCommand || result.Args == null)
		{
			return null;
		}

		var hints = new List<string>();
		foreach (var overload in result.Overloads)
		{
			var parts = new List<string>();
			var activeFound = false;
			var irrelevant = false;

			foreach (var param in overload.Params)
			{
				if (param.Token != null && !param.IsValid)
				{
					irrelevant = true;
					break;
				}

				var str = FormatParam(param.Expected);
				if (!activeFound && !param.IsValid)
				{
					str = $"<b>{Colorize(str, param.Expected.Type)}</b>";
					activeFound = true;
				}

				parts.Add(str);
			}

			if (!irrelevant && parts.Count > 0)
			{
				hints.Add(string.Join(" ", parts));
			}
		}

		return hints.Count > 0 ? hints : null;
	}

	private static string FormatParam(CommandParam param)
	{
		return param.IsTarget ? "<@gameObject>" : $"<{param.Name}:{GetFormatExample(param.Type)}>";
	}

	private static string GetFormatExample(Type type)
	{
		if (Formatters.TryGetValue(type, out var formatter))
		{
			return formatter.Format;
		}

		if (type.IsEnum)
		{
			var names = Enum.GetNames(type);
			return names.Length > 0 ? names[0] : type.Name;
		}

		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
		{
			var elementExample = GetFormatExample(type.GetGenericArguments()[0]);
			return $"{elementExample},{elementExample},...";
		}

		return type.Name;
	}

	private string Colorize(string text, Type type)
	{
		if (_colors == null)
		{
			return text;
		}

		var color = _colors.TypeColors.TryGetValue(type, out var c) ? c : _colors.FallbackParamColor;
		return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
	}

	private static Dictionary<Type, IHintFormatter> GetFormatters()
	{
		if (_formatters != null)
		{
			return _formatters;
		}

		_formatters = new();

		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (assembly.IsDynamic)
			{
				continue;
			}

			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				types = e.Types;
			}

			if (types == null)
			{
				continue;
			}

			foreach (var type in types)
			{
				if (type == null || type.IsAbstract || type.IsInterface)
				{
					continue;
				}

				if (!typeof(IHintFormatter).IsAssignableFrom(type))
				{
					continue;
				}

				if (type.GetConstructor(Type.EmptyTypes) == null)
				{
					continue;
				}

				var formatter = (IHintFormatter)Activator.CreateInstance(type);
				_formatters[formatter.TargetType] = formatter;
			}
		}

		return _formatters;
	}
}
}