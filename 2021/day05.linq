<Query Kind="Program" />

#load "..\common\client"

record Entry((int X, int Y) Start, (int X, int Y) End);
async void Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 5);

//	input = @"0,9 -> 5,9
//8,0 -> 0,8
//9,4 -> 3,4
//2,2 -> 2,1
//7,0 -> 7,4
//6,4 -> 2,0
//0,9 -> 2,9
//3,4 -> 1,4
//0,0 -> 8,8
//5,5 -> 8,2".Replace("\r\n", "\n");

	//input = "5,5 -> 2,2";

	var lines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);
	//lines.Dump();

	var entries = lines
		.Select(line =>
		{
			var parts = line.Split(" -> ");
			var part1 = parts[0].Split(",").Select(int.Parse).ToArray();
			var part2 = parts[1].Split(",").Select(int.Parse).ToArray();
			return new Entry((part1[0], part1[1]), (part2[0], part2[1]));
		});

	var width = entries.SelectMany(e => new[] { e.Start.X, e.End.X }).Max();
	var height = entries.SelectMany(e => new[] { e.Start.Y, e.End.Y }).Max();

	width = 1000;
	height = 1000;
	
	//width = 10;
	//height = 10;

	//width.Dump("widht");
	//height.Dump("height");
	//return;
	int[,] grid = new int[width, height];
	
	foreach (var entry in entries)
	{
		if (entry.Start.X == entry.End.X)
		{
			var ordered = new[] { entry.Start, entry.End }.OrderBy(e => e.Y).ToArray();
			
			for (var y = ordered[0].Y; y <= ordered[1].Y; y++)
			{
				grid[entry.Start.X, y] += 1;
			}
		}
		else if (entry.Start.Y == entry.End.Y)
		{
			var ordered = new[] { entry.Start, entry.End }.OrderBy(e => e.X).ToArray();

			for (var x = ordered[0].X; x <= ordered[1].X; x++)
			{
				grid[x, entry.Start.Y] += 1;
			}
		}
		else
		{
			var xDiff = entry.End.X - entry.Start.X;
			var yDiff = entry.End.Y - entry.Start.Y;

			// Diagonal +x +y or -x -y
			if (xDiff == yDiff)
			{
				var ordered = new[] { entry.Start, entry.End }.OrderBy(e => e.X).ToArray();

				var minX = ordered[0].X;
				var minY = ordered[0].Y;

				for (var i = 0; i <= ordered[1].X - ordered[0].X; i++)
				{
					grid[i + ordered[0].X, i + ordered[0].Y] += 1;
				}

			}
			// diagonal +x -y or -x +y
			else if (xDiff == -yDiff)
			{
				//new { xDiff, yDiff }.Dump();
				(int X, int Y)[] ordered;
				if (xDiff > 0)
				{
					ordered = new[] { entry.Start, entry.End }.OrderBy(e => e.X).ToArray();
				}
				else
				{
					ordered = new[] { entry.Start, entry.End }.OrderBy(e => e.X).ToArray();
				}
				
				// x increasing, y decreasing
				var length = ordered[1].X - ordered[0].X;

				for (var i = 0; i <= ordered[1].X - ordered[0].X; i++)
				{
					var x = i + ordered[0].X;
					var y = -i + ordered[0].Y;
					//new[] { x, y }.Dump();
					//if (x < 0 || x >= width)
					//{
					//	"invalid x".Dump();
					//}
					//if (y < 0 || y >= height)
					//{
					//	"invalid x".Dump();
					//}
					grid[x, y] += 1;
				}
			}
			else{ throw new Exception("oops"); }

		}
	}
	
	var sum = 0;
	foreach (var x in Enumerable.Range(0, width))
	{
		foreach (var y in Enumerable.Range(0, height))
		{
			if (grid[x, y] >= 2)
			{
				sum++;
			}
		}
	}

	sum.Dump();

	var grid2 = new int[height, width];
	foreach (var x in Enumerable.Range(0, width))
	{
		foreach (var y in Enumerable.Range(0, height))
		{
			grid2[y, x] = grid[x, y];
		}
	}
	grid2.Dump();

	//grid.Dump();
// 19053 wrong part2
// 22336 wqrong part2
// 22364 correct ( fucked up length by 1, changed to < instead of <= earlier when I shouldn't have)
	//entries.Dump();
	
}
