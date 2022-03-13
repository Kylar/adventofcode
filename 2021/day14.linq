<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 14);

	var textLines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);
	var poly = textLines.First();
	var rules = textLines.Skip(1).Select(c => (L: c[0], R: c[1], New: c.Last()))
		.ToArray();
		
	textLines.Dump();
	poly.Dump();
	rules.Dump();
	
	var ruleDict = rules.ToDictionary(x => (x.L, x.R), x => x.New);

	//
	//
	//for ( var iter = 0 ; iter < 40; iter++)
	//{
	//	iter.Dump();
	//	var s = new StringBuilder();
	//	for (var i = 0; i < poly.Length - 1; i++)
	//	{
	//		var l = poly[i];
	//		var r = poly[i+1];
	//		s.Append(poly[i]);
	//		
	//		//var newc = rules.Single(rule => rule.L == l && rule.R == r).New;
	//		var newc = ruleDict[(l, r)];
	//		s.Append(newc);
	//	}
	//	s.Append(poly[poly.Length - 1]);
	//	poly = s.ToString();
	//}
	//
	//var x = poly.GroupBy(e => e)
	//	.Select(g => (g.Key, g.Count()))
	//	.OrderBy(e => e.Item2)
	//	.ToArray();
	//var min = x.First().Item2;
	//var max = x.Last().Item2;
	//(max - min).Dump("p1");

	var finalAnswer = new Dictionary<char, long>();
	var cache = new Dictionary<(char, char, int), Dictionary<char, long>>();

	for (var i = 0; i < poly.Length - 1; i++)
	{
		var l = poly[i];
		var r = poly[i+1];
		AddToDict(finalAnswer, l, 1);
		foreach (var kvp in ComputeInserted(cache, l, r, 40))
		{
			AddToDict(finalAnswer, kvp.Key, kvp.Value);
		}
		var newc = ruleDict[(l, r)];
	}
	AddToDict(finalAnswer, poly[poly.Length - 1], 1);

	var x = finalAnswer
		.Select(g => (g.Key, g.Value))
		.OrderBy(e => e.Item2)
		.ToArray();
	var min = x.First().Item2;
	var max = x.Last().Item2;
	(max - min).Dump("p2");


	void AddToDict(Dictionary<char, long> d, char c, long l)
	{
		if (d.TryGetValue(c, out var existing))
		{
			d[c] = existing + l;
		}
		else
		{
			d.Add(c, l);
		}
	}

	Dictionary<char, long> ComputeInserted(Dictionary<(char, char, int), Dictionary<char, long>> cache, char l, char r, int iters)
	{
		if (cache.TryGetValue((l, r, iters), out var answer))
		{
			return answer;
		}

		answer = new Dictionary<char, long>();

		if (iters == 0)
		{
			return answer;
		}

		var newC = ruleDict[(l, r)];
		AddToDict(answer, newC, 1);

		foreach (var kvp in ComputeInserted(cache, l, newC, iters - 1))
		{
			AddToDict(answer, kvp.Key, kvp.Value);
		}
		foreach (var kvp in ComputeInserted(cache, newC, r, iters - 1))
		{
			AddToDict(answer, kvp.Key, kvp.Value);
		}
		
		cache.Add((l, r, iters), answer);
		return answer;
	}

}
