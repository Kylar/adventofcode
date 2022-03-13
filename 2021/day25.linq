<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 25);
//	input = @"v...>>.vv>
//.vv>>.vv..
//>>.>v>...v
//>>v>>.>.v.
//v>v.vv.v..
//>.>>..v...
//.vv..>.>v.
//v.v..>>v.v
//....v..v.>";

	var textLines = input.Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);
	textLines.Dump();
		
	var width = textLines[0].Length;
	var height = textLines.Length;
	
	var grid = new char[height, width];
	var nextGrid = new char[height, width];
	
	for (var y = 0; y < height; y++)
	{
		for (var x = 0; x < width; x++)
		{
			grid[y, x] = textLines[y][x];
		}
	}
	
	(width, height).Dump("size");
	
	grid.Dump("initial grid");
	int steps = 0;
	do
	{
		steps++;
	} while (Step(grid, nextGrid));
	
	grid.Dump("final grid");
	
	steps.Dump("part1 answer");
}

bool Step(char[,] grid, char[,] nextGrid)
{
	bool moved = false;
	var h = grid.GetLength(0);
	var w = grid.GetLength(1);
	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			if (grid[y, x] == '>')
			{
				var canMove = grid[y,(x+1) % w] == '.';
				if (canMove)
				{
					moved = true;
				}
				nextGrid[y, x] = canMove ? '.' : '>';
			}
			else if (grid[y, x] == '.')
			{
				if (grid[y, (x+w-1) % w] == '>')
				{
					nextGrid[y, x] = '>';
				}
				else
				{
					nextGrid[y, x] = '.';
				}
			}
			else
			{
				nextGrid[y, x] = grid[y, x];
			}
		}
	}
	
	var temp = grid;
	grid = nextGrid;
	nextGrid = temp;

	for (var y = 0; y < h; y++)
	{
		for (var x = 0; x < w; x++)
		{
			if (grid[y, x] == 'v')
			{
				var canMove = grid[(y + 1) % h, x] == '.';
				if (canMove)
				{
					moved = true;
				}
				nextGrid[y, x] = canMove ? '.' : 'v';
			}
			else if (grid[y, x] == '.')
			{
				if (grid[(y + h - 1) %h , x] == 'v')
				{
					nextGrid[y, x] = 'v';
				}
				else
				{
					nextGrid[y, x] = '.';
				}
			}
			else
			{
				nextGrid[y, x] = grid[y, x];
			}
		}
	}
	
	return moved;
}