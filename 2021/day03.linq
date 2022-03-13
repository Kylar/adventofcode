<Query Kind="Program" />

#load "..\common\client"

async void Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 3);
	//input.Dump();

//	input = @"00100
//11110
//10110
//10111
//10101
//01111
//00111
//11100
//10000
//11001
//00010
//01010";


	var textLines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries)
		.Select(t => t.Trim());
	var n = textLines.First().Length;
	var bits = Enumerable.Range(0, n)
		.Select(i =>
		{
			var groups = textLines.Select(l => l[i])
				.GroupBy(e => e)
				.Select(g => (g.Key, g.Count()))
				.ToDictionary(g => g.Key, g => g.Item2);
			var r = 1;
			for (var x = n - i - 1; x > 0; x--)
			{
				r *= 2;
			}
			if (groups['0'] > groups['1'])
			{
				return (Gamma: 0, Eps: r);
			}
			else
			{
				return (Gamma: r, Eps: 0);
			}
		})
		//.Dump()
		;



	//.Select(l => int.Parse(l))
	//.Select(e => (A: e[0], B: e[1]))
	;

	// Part 1


	var gamma = bits.Sum(e => e.Gamma);
	var eps = bits.Sum(e => e.Eps);
	var answer = gamma * eps;
	// wrong 229432
	answer.Dump("part1");

	// Part 2


	var generator = BinToDec(SearchGen(textLines.ToArray(), '1', '0')
		//.Dump("gen")
	)
	//.Dump()
	;
	var scrubber = BinToDec(SearchGen(textLines.ToArray(), '0', '1', true)
		//.Dump("scrubber")
	)
	//.Dump()
	;


	// 1492629760 wrong -- wasn't filtering items before computing groups in searchGen
	// 1330294102 wrong -- needed to add invert parameter for scrubber, and fix bug in BinToDec
	// 230 wrong -- ran on input data...
	// 4672151 correct
	answer = generator * scrubber;
	answer.Dump("part2 answer");
	
	//BinToDec("01010").Dump("Dec");
	
}

private int BinToDec(string n)
{
	var x = 1;
	var r = 0;
	for (var i = n.Length - 1; i >= 0; i--)
	{
		if (n[i] != '0')
		{
			r += 1 * x;

		}
		x *= 2;
	}
	return r;
}

private string SearchGen(string[] items, char searchBit, char otherBit, bool invert=false)
{
	var removedSet = new HashSet<int>();
	
	for (var i = 0; i < items.First().Length; i++)
	{
		var groups = items
			.Select((e, index) => (e[i], index))
			.Where(e => !removedSet.Contains(e.index))
			.Select(e => e.Item1)
			.GroupBy(e => e)
			.ToDictionary(e => e.Key, e => e.Count());
		//groups.Dump("groups");
		if (!groups.ContainsKey('1'))
		{
			groups['1'] = 0;
		}
		if (!groups.ContainsKey('0'))
		{
			groups['0'] = 0;
		}
		char mostCommon;
		if (groups[otherBit] > groups[searchBit])
		{
			mostCommon = otherBit; 
		}
		else
		{
			mostCommon = searchBit;
		}
		
		if (invert)
		{
			if (groups[otherBit] < groups[searchBit])
			{
				mostCommon = otherBit;
			}
			else
			{
				mostCommon = searchBit;
			}
		}


		for (var x = 0; x < items.Length; x++)
		{
			if (removedSet.Contains(x))
			{
				continue;
			}
			if (items[x][i] != mostCommon)
			{
				removedSet.Add(x);
				//$"i {i} x {x} item {items[x]}".Dump("removing");
				if (removedSet.Count == items.Length - 1)
				{
					var resultIndex = Enumerable.Range(0, items.Length).Except(removedSet).Single();
					return items[resultIndex];
				}
			}
		}

	}
	throw new Exception("nah");

}
