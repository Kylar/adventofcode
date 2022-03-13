<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 9);

//	input = @"2199943210
//3987894921
//9856789892
//8767896789
//9899965678".Replace("\r\n", "\n");

	var textLines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);
	textLines.Dump();

	var grid = textLines.Select(e => e.Select(c => c - '0').ToArray()).ToArray()
		//.Dump()
		;

	var width = grid.First().Length;
	var height = grid.Length;
	
	width.Dump();
	height.Dump();
	
	var pointToHeight = grid.SelectMany((l, y) => l.Select((h, x) => (new Point(x, y), h)))
		.ToDictionary(e => e.Item1, e => e.h);
		
	
	Point[] Neighbours(Point p)
	{
		return new[] {
			new Point(p.X - 1, p.Y),
			new Point(p.X + 1, p.Y),
			new Point(p.X, p.Y + 1),
			new Point(p.X, p.Y - 1),
			
		}.Where(p => p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height)
		.ToArray();
	}
	
	var lowPoints = pointToHeight.Keys.Where(p => Neighbours(p).All(n => pointToHeight[n] > pointToHeight[p]))
		.ToList()
		.Dump("low points");
	
	var part1 = lowPoints.Select(p => 1 + pointToHeight[p]).Sum().Dump("part1 answer");
	
	var nextBasin = 0;
	var basinToPoints = new List<HashSet<Point>>();
	var pointToBasin = new Dictionary<Point, int>();
	
	foreach (var p in pointToHeight.Keys)
	{
		if (pointToHeight[p] >= 9)
		{
			continue;
		}
		
		if (pointToBasin.ContainsKey(p))
		{
			continue;
		}
		
		var basinId = nextBasin;
		nextBasin++;
		var numPoints = 0;
		
		var frontier = new HashSet<Point>();
		frontier.Add(p);
		while (frontier.Any())
		{
			var next = frontier.First();
			frontier.Remove(next);
			if (pointToBasin.ContainsKey(next))
			{
				continue;
			}
			
			pointToBasin.Add(next, basinId);
			numPoints++;

			foreach (var neighbour in Neighbours(next))
			{
				if (pointToHeight[neighbour] != 9)
				{
					frontier.Add(neighbour);
				}
			}
		}
	}
	
	pointToBasin
		.GroupBy(kvp => kvp.Value)
		.Select(g =>
		{
			var id = g.Key;
			var points = g.Select(e => e.Key).ToArray();
			return (Id: id, Points: points);
		})
		.OrderByDescending(e => e.Points.Count())
		.Dump()
		.Take(3)
		.Aggregate(1d, (acc, v) => acc * v.Points.Count())
		// 9895 wrong
		// 959136 right (had a typo "!= 0" instead of "!= 9")
		.Dump("answer part2");
	
	//nextBasin.Dump();
	//textLines.Dump();
	
}

record Point(int X, int Y);
