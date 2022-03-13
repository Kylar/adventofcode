<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 19);
	//input = TestInput();

	var textLines = input.Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);
	//textLines.Dump();
	
	var scanners = new List<List<Point>>();
	List<Point> l = null;
	foreach (var line in textLines)
	{
		if (line.Contains("scanner"))
		{
			l = new List<Point>();
			scanners.Add(l);
		}
		else
		{
			l.Add(ParseLine(line));
		}
	}

	var orientations = AllRotations();

	Func<Point, Point> InverseTransform(Func<Point, Point> f)
	{
		var p = new Point(1, 2, 3);
		var p2 = f(p);
		return orientations.First(o => o(p2) == p);
	}

	//scanners.Dump();


	var knownBeaconSets = new[] { scanners.First() }.ToList();
	
	var remainingScanners = scanners.Skip(1).ToList();

	var scannerPoses = new List<Point> { new Point(0, 0, 0) };


	bool TryMerge()
	{
		var n1 = 0;
		foreach (var knownBeacons in knownBeaconSets)
		{
			n1++;
			var n2 = 0;
			foreach (var beacons in remainingScanners)
			{
				n2++;
				foreach (var orientation in orientations)
				{
					var rotatedBeacons = beacons
						.Select(b => orientation(b))
						.ToList();

					var dists = new double[knownBeacons.Count, rotatedBeacons.Count];
					var dists2 = new List<(Point, Point)>();

					foreach (var i in Enumerable.Range(0, knownBeacons.Count))
					{
						foreach (var j in Enumerable.Range(0, rotatedBeacons.Count))
						{
							dists[i, j] = (rotatedBeacons[j] - knownBeacons[i]).Length();
							dists2.Add((rotatedBeacons[j], knownBeacons[i]));
						}
					}
					//dists.Dump();

					//dists2.Dump();

					var matches = dists2
						.GroupBy(e => (e.Item2 - e.Item1).ToString())
						.Where(g => g.Count() >= 12)
						.Select(g => g.First())
						.Select(e => e.Item1 - e.Item2);
					if (matches.Any())
					{
						var offset = matches.First();
						$"matching n1={n1} n2={n2} dist={offset}".Dump();
						knownBeaconSets.Add(rotatedBeacons.Select(r => r - offset).ToList());
						scannerPoses.Add(offset);
						remainingScanners.Remove(beacons);
						//var inverseOrientation = InverseTransform(orientation);
						// combine beacons
						return true;
					}

				}
			}
		}

		return false;
	}

	while (TryMerge())
	{
		
	}
	// 694 wrong part1
	// 355 correct (I forgot to actually translate the beacons after finding 12 matches)
	//knownBeacons.Count.Dump();
	knownBeaconSets.Dump("knownBeaconSets");
	knownBeaconSets.SelectMany(b => b).Distinct().Count().Dump();

	var maxDist = 0L;
	for (var i = 0; i < scannerPoses.Count; i++)
	{
		for (var j = 0; j < scannerPoses.Count; j++)
		{
			if (i == j) continue;
			var d = (scannerPoses[j] - scannerPoses[i]).ManhattanLength();
			if (d > maxDist) maxDist = d;
		}


	}
	
	// 8443 part2 wrong 
	// 10842 part2 right (forgot to abs each coordinate to compute manhattan distance)
	maxDist.Dump("part2");
}



Point ParseLine(string line)
{
	var parts = line.Split(",").Select(int.Parse).ToArray();
	var entry = new Point(parts[0], parts[1], parts[2]);
	return entry;
}

record Point(int X, int Y, int Z)
{
	public override string ToString()
	{
		return $"({X}, {Y}, {Z})";
	}
	
	public static Point operator -(Point p, Point p2)
	{
		return new Point(p.X - p2.X, p.Y - p2.Y, p.Z - p2.Z);	
	}

	public long ManhattanLength()
	{
		return( X > 0 ? X : -X)
			+ (Y > 0 ? Y : -Y)
			+ (Z > 0?  Z : -Z);
	}
	public double Length()
	{
		return Math.Sqrt(X * X + Y * Y + Z * Z);
	}
}


Func<Point, Point>[] AllRotations()
{
	//var p = new Point(1, 1, 1);
	//var result = new List<Func<Point, Point>>();
	//
		//var combs = from x in new[] { -1, 1 }
		//from y in new[] { -1, 1 }
		//from z in new[] { -1, 1 }
		//from i in new[] { 0, 1, 2 }
		//select (new Point(x, y, z), i);

	//return combs.Select(x =>
	//{
	//	var mul = x.Item1;
	//	var transform = x.i;
	//	
	//	
	//	if (transform == 0)
	//	{
	//		return point => new Point(p.X * mul.X, p.Y * mul.Y, p.Z * mul.Z);
	//	}
	//	else if (transform == 1)
	//	{
	//		return p => new Point(p.X * mul.Y, mul.Z, mul.X);
	//	}
	//	else{
	//		return point => new Point(mul.Z, mul.X, mul.Y);
	//	}
	//}
	//	
	var faces = new Func<Point, Point>[]
	{
		p => p,
		p => RotateAroundY(p),
		p => RotateAroundY(RotateAroundX(p)),
		p => RotateAroundY(RotateAroundX(RotateAroundX(p))),
		p => RotateAroundY(RotateAroundX(RotateAroundX(RotateAroundX(p)))),
		p => RotateAroundY(RotateAroundX(RotateAroundX(RotateAroundX(p)))),
		p => RotateAroundY(RotateAroundY(p)),
	};

	var allOrientations = faces.SelectMany(f => new Func<Point, Point>[]
	{
		p => f(p),
		p => RotateAroundZ(f(p)),
		p => RotateAroundX(f(p)),
		p => RotateAroundY(f(p)),
		p => RotateAroundZ(RotateAroundZ(f(p))),
		p => RotateAroundX(RotateAroundX(f(p))),
		p => RotateAroundY(RotateAroundY(f(p))),
		p => RotateAroundZ(RotateAroundZ(RotateAroundZ(f(p)))),
		p => RotateAroundX(RotateAroundX(RotateAroundX(f(p)))),
		p => RotateAroundY(RotateAroundY(RotateAroundY(f(p)))),
	});
	
	var p = new Point(1, 2, 3);
	return allOrientations
		.GroupBy(f => f(p).ToString())
		.Select(g => g.First())
		.ToArray();
	
	//return allOrientations.ToArray();
	
}

Point RotateAroundX(Point p)
{
	return new Point(p.X, -p.Z, p.Y);
}
Point RotateAroundY(Point p)
{
	return new Point(p.Z, p.Y, -p.X);
}
Point RotateAroundZ(Point p)
{
	return new Point(-p.Y, p.X, p.Z);
}

string TestInput()
{

	return @"--- scanner 0 ---
404,-588,-901
528,-643,409
-838,591,734
390,-675,-793
-537,-823,-458
-485,-357,347
-345,-311,381
-661,-816,-575
-876,649,763
-618,-824,-621
553,345,-567
474,580,667
-447,-329,318
-584,868,-557
544,-627,-890
564,392,-477
455,729,728
-892,524,684
-689,845,-530
423,-701,434
7,-33,-71
630,319,-379
443,580,662
-789,900,-551
459,-707,401

--- scanner 1 ---
686,422,578
605,423,415
515,917,-361
-336,658,858
95,138,22
-476,619,847
-340,-569,-846
567,-361,727
-460,603,-452
669,-402,600
729,430,532
-500,-761,534
-322,571,750
-466,-666,-811
-429,-592,574
-355,545,-477
703,-491,-529
-328,-685,520
413,935,-424
-391,539,-444
586,-435,557
-364,-763,-893
807,-499,-711
755,-354,-619
553,889,-390

--- scanner 2 ---
649,640,665
682,-795,504
-784,533,-524
-644,584,-595
-588,-843,648
-30,6,44
-674,560,763
500,723,-460
609,671,-379
-555,-800,653
-675,-892,-343
697,-426,-610
578,704,681
493,664,-388
-671,-858,530
-667,343,800
571,-461,-707
-138,-166,112
-889,563,-600
646,-828,498
640,759,510
-630,509,768
-681,-892,-333
673,-379,-804
-742,-814,-386
577,-820,562

--- scanner 3 ---
-589,542,597
605,-692,669
-500,565,-823
-660,373,557
-458,-679,-417
-488,449,543
-626,468,-788
338,-750,-386
528,-832,-391
562,-778,733
-938,-730,414
543,643,-506
-524,371,-870
407,773,750
-104,29,83
378,-903,-323
-778,-728,485
426,699,580
-438,-605,-362
-469,-447,-387
509,732,623
647,635,-688
-868,-804,481
614,-800,639
595,780,-596

--- scanner 4 ---
727,592,562
-293,-554,779
441,611,-461
-714,465,-776
-743,427,-804
-660,-479,-426
832,-632,460
927,-485,-438
408,393,-506
466,436,-512
110,16,151
-258,-428,682
-393,719,612
-211,-452,876
808,-476,-593
-575,615,604
-485,667,467
-680,325,-822
-627,-443,-432
872,-547,-609
833,512,582
807,604,487
839,-516,451
891,-625,532
-652,-548,-490
30,-46,-14";
}