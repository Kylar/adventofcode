<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 12);

	var textLines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);
	//textLines.Dump();
	
	var edges = textLines.Select(e => e.Split("-"))
		.Select(e => (Start: e[0], End: e[1]))
		.ToArray();
		
	var vertices = edges
		.Select(e => e.Start)
		.Concat(edges.Select(e => e.End))
		.Distinct();
		
	vertices.Dump("vertices");

	var vertexToAdjacent = vertices
		.Select(v =>
		{
			var n1 = edges.Where(e => e.Start == v).Select(e => e.End);
			var n2 = edges.Where(e => e.End == v).Select(e => e.Start);
			return (V: v, Adj: n1.Concat(n2).Distinct().ToArray());
		})
		.ToDictionary(e => e.V, e => e.Adj);

	vertexToAdjacent.Dump("vertexToAdjacent");

	edges.Dump();

	int Part1(string[] pathTaken, bool hasVisitedSmallCaveTwice = false)
	{
		var neighbours = vertexToAdjacent[pathTaken.Last()];
		
		var numPaths = 0;
		foreach (var n in neighbours)
		{
			var isBigCave = n != n.ToLower();
			var isSmallCave = n == n.ToLower();
			
			if (n == "end")
			{
				//pathTaken.Dump();
				numPaths++;
			}
			else if (n == "start")
			{
				continue;
			}
			else if (isBigCave)
			{
				var nextPath = pathTaken.Concat(new[] { n }).ToArray();
				numPaths += Part1(nextPath, hasVisitedSmallCaveTwice);
			}
			else if (isSmallCave)
			{
				if (pathTaken.Contains(n) && !hasVisitedSmallCaveTwice)
				{
					var nextHasVisitedSmallCaveTwice = true;
					var nextPath = pathTaken.Concat(new[] { n }).ToArray();
					numPaths += Part1(nextPath, nextHasVisitedSmallCaveTwice);
				}
				else if (!pathTaken.Contains(n))
				{
					var nextPath = pathTaken.Concat(new[] { n }).ToArray();
					numPaths += Part1(nextPath, hasVisitedSmallCaveTwice);
				}
			}
		}
		
		return numPaths;
	}
	
	var path = new[] { "start" };
	
	Part1(path).Dump("partr1");
}


