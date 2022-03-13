<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 22);

//	input = @"on x=-20..26,y=-36..17,z=-47..7
//on x=-20..33,y=-21..23,z=-26..28
//on x=-22..28,y=-29..23,z=-38..16
//on x=-46..7,y=-6..46,z=-50..-1
//on x=-49..1,y=-3..46,z=-24..28
//on x=2..47,y=-22..22,z=-23..27
//on x=-27..23,y=-28..26,z=-21..29
//on x=-39..5,y=-6..47,z=-3..44
//on x=-30..21,y=-8..43,z=-13..34
//on x=-22..26,y=-27..20,z=-29..19
//off x=-48..-32,y=26..41,z=-47..-37
//on x=-12..35,y=6..50,z=-50..-2
//off x=-48..-32,y=-32..-16,z=-15..-5
//on x=-18..26,y=-33..15,z=-7..46
//off x=-40..-22,y=-38..-28,z=23..41
//on x=-16..35,y=-41..10,z=-47..6
//off x=-32..-23,y=11..30,z=-14..3
//on x=-49..-5,y=-3..45,z=-29..18
//off x=18..30,y=-20..-8,z=-3..13
//on x=-41..9,y=-7..43,z=-33..15
//on x=-54112..-39298,y=-85059..-49293,z=-27449..7877
//on x=967..23432,y=45373..81175,z=27513..53682";

	//input = @"on x=3..4,y=1..2,z=48..50";

	var textLines = input.Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);
	textLines.Dump();

	var xPartitionSet = new HashSet<int>();
	var yPartitionSet = new HashSet<int>();
	var zPartitionSet = new HashSet<int>();
	
	var part1 = false;
	
	var entries = textLines.Select(ParseLine);

	if (part1)
	{
		var region = new Range(-50, 51);

		xPartitionSet.Add(region.Min);
		xPartitionSet.Add(region.Max);
		yPartitionSet.Add(region.Min);
		yPartitionSet.Add(region.Max);
		zPartitionSet.Add(region.Min);
		zPartitionSet.Add(region.Max);

		entries = entries.Take(20);
	}

	foreach (var entry in entries)
	{
		xPartitionSet.Add(entry.XRange.Min);
		xPartitionSet.Add(entry.XRange.Max);
		yPartitionSet.Add(entry.YRange.Min);
		yPartitionSet.Add(entry.YRange.Max);
		zPartitionSet.Add(entry.ZRange.Min);
		zPartitionSet.Add(entry.ZRange.Max);
	}

	var xPartitions = xPartitionSet.OrderBy(e => e).ToArray();
	var yPartitions = yPartitionSet.OrderBy(e => e).ToArray();
	var zPartitions = zPartitionSet.OrderBy(e => e).ToArray();
	
	//zPartitions.Dump();
	
	var partitionProcessed = new bool[xPartitionSet.Count, yPartitionSet.Count, zPartitionSet.Count];

	IEnumerable<(int MinIndex, int MaxIndex)> PartitionIndices(int[] partitions, Range range)
	{
		for (var xi = 1; xi < partitions.Length; xi++)
		{
			var minIndex = xi - 1;
			var maxIndex = xi;
			if (partitions[minIndex] < range.Min) continue;
			if (partitions[maxIndex] > range.Max) yield break;
			yield return (minIndex, maxIndex);
		}
	}
	
	long cubesOn = 0L;

	foreach (var entry in entries.Reverse())
	{
		var xThings = PartitionIndices(xPartitions, entry.XRange);
		var yThings = PartitionIndices(yPartitions, entry.YRange);
		var zThings = PartitionIndices(zPartitions, entry.ZRange);
		
		
		
		foreach (var xThing in xThings)
		{
			foreach (var yThing in yThings)
			{
				foreach (var zThing in zThings)
				{
					if (!partitionProcessed[xThing.MinIndex, yThing.MinIndex, zThing.MinIndex])
					{
						partitionProcessed[xThing.MinIndex, yThing.MinIndex, zThing.MinIndex] = true;
						if (entry.On)
						{
							var partitionSizes = (
								X: (xPartitions[xThing.MaxIndex] - xPartitions[xThing.MinIndex]),
								Y: (yPartitions[yThing.MaxIndex] - yPartitions[yThing.MinIndex]),
								Z: (zPartitions[zThing.MaxIndex] - zPartitions[zThing.MinIndex]));
							var numCubes = (long)partitionSizes.X * (long)partitionSizes.Y * (long)partitionSizes.Z;
							cubesOn += numCubes;
						}
					}
				}
			}
		}
	}


	//PartitionRanges(xPartitions).Dump("xPartitions");

	//PartitionCubeSizes().Dump();


	//xPartitionSet.Count.Dump();
	//yPartitionSet.Count.Dump();
	//zPartitionSet.Count.Dump();

	// 364771 part1 wrong
	// 650099 part1 correct
	// 1253517269865253 part2 wrong
	// 1254011191104293 part2 right - had to use long when computing (X*Y*Z) for numCubes in a partition
	cubesOn.Dump();
}

record Range(int Min, int Max);
record Entry(bool On, Range XRange, Range YRange, Range ZRange);

Entry ParseLine(string line)
{
	var parts = line.Split(" ");
	var on = parts[0] == "on" ? true : false;
	var parts2 = parts[1].Split(",");
	var ranges = parts2.Select(ParseRange).ToArray();
	return new Entry(on, ranges[0], ranges[1], ranges[2]);
}

Range ParseRange(string line)
{
	var parts = line.Substring(2).Split("..");
	return new Range(int.Parse(parts[0]), int.Parse(parts[1]) + 1);
}