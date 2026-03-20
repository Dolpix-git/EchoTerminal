using System;
using System.Collections.Generic;

namespace EchoTerminal
{
public static class FuzzyMatcher
{
	public static List<string> Filter(IEnumerable<string> candidates, string partial)
	{
		if (string.IsNullOrEmpty(partial))
		{
			return new();
		}

		var scored = new List<(int score, string name)>();

		foreach (var candidate in candidates)
		{
			var s = Score(candidate, partial);
			if (s >= 0)
			{
				scored.Add((s, candidate));
			}
		}

		scored.Sort((a, b) => a.score != b.score
			? a.score.CompareTo(b.score)
			: string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

		var result = new List<string>(scored.Count);
		foreach (var (_, name) in scored)
		{
			result.Add(name);
		}

		return result;
	}

	public static int Score(string candidate, string partial)
	{
		if (string.IsNullOrEmpty(partial))
		{
			return 0;
		}

		if (candidate.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
		{
			return 0;
		}

		if (partial.Length >= 3)
		{
			var len = Math.Min(partial.Length, candidate.Length);
			var dist = EditDistance(
				candidate[..len].ToLowerInvariant(),
				partial[..len].ToLowerInvariant()
			);
			var threshold = Math.Max(1, partial.Length / 3);
			if (dist <= threshold)
			{
				return 1 + dist;
			}
		}

		return -1;
	}

	private static int EditDistance(string a, string b)
	{
		var m = a.Length;
		var n = b.Length;
		var d = new int[m + 1, n + 1];

		for (var i = 0; i <= m; i++)
		{
			d[i, 0] = i;
		}

		for (var j = 0; j <= n; j++)
		{
			d[0, j] = j;
		}

		for (var i = 1; i <= m; i++)
		for (var j = 1; j <= n; j++)
		{
			var cost = a[i - 1] == b[j - 1] ? 0 : 1;
			d[i, j] = Math.Min(d[i - 1, j] + 1, Math.Min(d[i, j - 1] + 1, d[i - 1, j - 1] + cost));
		}

		return d[m, n];
	}
}
}