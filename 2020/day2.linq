<Query Kind="Program" />

#load "..\common\client"

async void Main()
{
	using var c = Configure2020();
	var input = await c.GetInput(2);
	var lines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries)
		.Select(line =>
		{
			var match = Regex.Match(line, @"^(\d+)\-(\d+) (\w): (\w+)$");
			return new Line(
				line,
				int.Parse(match.Groups[1].Value),
				int.Parse(match.Groups[2].Value),
				match.Groups[3].Value[0],
				match.Groups[4].Value
			);
		})
		.ToArray();

	//Part1(lines);
	Part2(lines);
}

private record Line(string Text, int Min, int Max, char C, string Pass);

async void Part1(Line[] lines)
{
	lines
		.Count(e =>
		{
			var count = e.Pass.Count(c => c == e.C);
			return count >= e.Min && count <= e.Max;
		})
		.Dump();
}

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