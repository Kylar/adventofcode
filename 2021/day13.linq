<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 13);
	var sinput = @"6,10
0,14
9,10
0,3
10,4
4,11
6,0
6,12
4,1
0,13
10,12
3,4
3,0
8,4
1,10
2,14
8,10
9,0

fold along y=7
fold along x=5";
	//input = sinput.Replace("\r\n", "\n");

	var textLines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);
	//textLines.Dump();
	var points = textLines.Where(l => !l.StartsWith("fold"))
		.Select(l => l.Split(",").Select(int.Parse).ToArray())
		.Select(e => (X: e[0], Y: e[1]));

	var folds = textLines.Where(l => l.StartsWith("fold"))
			.Select(l => l.Split(" ")[2])
			.Select(e => {
				var axis = e[0];
				var p = int.Parse(e.Substring(2));
				return (axis, p);
			})
			.ToArray();
			
	points.Take(3).Dump();
	folds.Dump();
	
	foreach (var fold in folds)
	{
		//break;
		if (fold.axis == 'x')
		{
			// a - (a - b) * 2
			// == a - 2a + 2b
			// == 2b - a
			
			points = points
				.Select(p => (p.X < fold.p ? p.X : p.X - (p.X - fold.p) * 2, p.Y))
				.Distinct()
				.ToArray();
		}
		else
		{
			points = points
				.Select(p => (p.X, p.Y < fold.p ? p.Y : p.Y - (p.Y - fold.p) * 2))
				.Distinct()
				.ToArray();
			points.Dump();

		}
		
		//break;
	}
	// 953 wrong
	points.Count().Dump("part1");
	

	for (var i = 0; i <= points.Max(p => p.Y); i++)
	{
		for (var x = 0; x <= points.Max(p => p.X); x++)
		{
			var cc = points.Contains((x, i)) ? '#' : ' ';
			Console.Write(cc);
		}
		Console.WriteLine();
	}
}
