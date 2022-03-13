<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(1);
	
	var lines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries)
		.Select(int.Parse)
		.ToArray()
		//.Dump()
		;
		
		var x  = lines.Zip(lines.Skip(1)).Count(e => e.First < e.Second).Dump();

	//lines = lines.Take(5).ToArray();
//	lines = new[]
//	{
//		199, 200, 208, 210, 200, 207, 240, 269, 260, 263
//};
	//lines.Dump();
	var result = new List<int>();
	for (var i = 0; i < lines.Length + 0; i++)
	{	
		
		
		var n = 0;
		for (var j = i; j < i + 3; j++)
		{
			if (j >= 0 && j < lines.Length)
			{
				n += lines[j];
			}
		}
		result.Add(n);
	}
	//1105 wrong
	
	result.Dump();

	var y = result.Zip(result.Skip(1)).Count(e => e.First < e.Second).Dump();

	//1105 wrong, 1103 correct
	//Part1(lines);
	//Part2(lines);
	
	// Lost a few seconds setting up LINQPad for 2021 because I saved and it put it in a different dir and I didn't click the right option to change #load path
	// part 1 (var x) Time 1:45
	// part 2 (var y) Time 8:56 -- Got confused about sliding window - I thought it was (1), (1 + 2), (1 + 2 + 3), (2 + 3 + 4), but it's (1 + 2 + 3), (2 + 3 + 4) :(
}

private record Line(string Text, int Min, int Max, char C, string Pass);

//async void Part1(string[] lines)
//{
//	lines
//		.Count(e =>
//		{
//			var count = e.Pass.Count(c => c == e.C);
//			return count >= e.Min && count <= e.Max;
//		})
//		.Dump();
//}

async void Part2(Line[] lines)
{
	var answer = lines
		.Count(e =>
		{
			bool Match(int i) => i >= 0 && i < e.Pass.Length && e.Pass[i] == e.C;
			var match1 = Match(e.Min - 1);
			var match2 = Match(e.Max - 1);
			return match1 && !match2 || !match1 && match2;
		})
		.Dump();
		
	if (answer == 422)
	{
		// Forgot to convert 1 based indices from input into 0 based indices for string indexing
		throw new Exception("422 is a wrong answer");
	}
}

// You can define other methods, fields, classes and namespaces here