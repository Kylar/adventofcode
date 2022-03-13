<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 11);
	var sinput = @"5483143223
2745854711
5264556173
6141336146
6357385478
4167524645
2176841721
6882881134
4846848554
5283751526".Replace("\r\n", "\n");
	//input = sinput;

	var textLines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);
	textLines.Dump();
	
	var grid = textLines.Select(line => line.Select(c => c - '0').ToArray()).ToArray();
	var width = grid.First().Length;
	var height = grid.Length;

	var totalFlashes = 0;
	var hasFlashed = new bool[width, height];
	var flashesInDay = 0;
	void TryFlash(int x, int y)
	{
		if (grid[y][x] <= 9)
		{
			return;
		}
		if (hasFlashed[x, y])
		{
			return;
		}
		
//		(x, y).Dump("flash");
		hasFlashed[x, y] = true;
		totalFlashes++;
		flashesInDay++;
		
		for (var dx = -1; dx <= 1; dx++)
		{
			for (var dy = -1; dy <= 1; dy++)
			{
				if (dx != 0 || dy != 0)
				{
					if (dx + x >= 0 && dx+x < width && dy+y >= 0 && dy+y < height)
					{
						grid[y + dy][x + dx]++;
						TryFlash(x + dx, y + dy);
					}
				}
			}
		}
	}
	
	//for (var i = 0; i < 100; i++)
	var i = 0;
	while (true)
	{
		i++;
		flashesInDay = 0;
		DisplayGrid(grid);

		Array.Clear(hasFlashed, 0, hasFlashed.Length);
		foreach (var x in Enumerable.Range(0, width))
		{
			foreach (var y in Enumerable.Range(0, height))
			{
				grid[y][x] += 1;
				TryFlash(x, y);
			}
		}
		
		foreach (var x in Enumerable.Range(0, width))
		{
			foreach (var y in Enumerable.Range(0, height))
			{
				if (grid[y][x] > 9)
				{
					grid[y][x] = 0;
				}
			}
		}
		
		if (flashesInDay == width * height)
		{
			i.Dump("answer2");
			break;
		}
	}
	
	totalFlashes.Dump();
}


void DisplayGrid(int[][] grid)
{
	string.Join("\n", grid.Select(line => string.Join("", line))).Dump("grid");
	
}