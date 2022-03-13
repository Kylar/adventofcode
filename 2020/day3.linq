<Query Kind="Program" />

#load "..\common\client"

async void Main()
{
	using var c = Configure2020();
	var input = await c.GetInput(3);
	var grid = input.Split("\n", StringSplitOptions.RemoveEmptyEntries)
		.ToArray();
	grid.Dump();

	//Part1(grid);
	Part2(grid);
}

async void Part1(string[] lines)
{
	var x = 0;
	var y = 0;
	var hits = 0;
	for (var i = 0; i < lines.Length; i++)
	{
		var hit = lines[y][x % lines[y].Length] == '#';
		if (hit) hits++;
		x += 3;
		y += 1;
	}
	hits.Dump();
}

async void Part2(string[] lines)
{
	long result = 1;
	var slopes = new[]
	{
		(1, 1),
		(3, 1),
		(5, 1),
		(7, 1),
		(1, 2),
	};
	foreach (var slope in slopes)
	{
		var x = 0;
		var hits = 0;
		for (var y = 0; y < lines.Length;)
		{
			var hit = lines[y][x % lines[y].Length] == '#';
			if (hit) hits++;
			x += slope.Item1;
			y += slope.Item2;
		}
		hits.Dump();
		result *= hits;
	}
	result.Dump();
}

// You can define other methods, fields, classes and namespaces here