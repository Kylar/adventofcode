<Query Kind="Program">
  <NuGetReference>OptimizedPriorityQueue</NuGetReference>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Priority_Queue</Namespace>
</Query>

#load "..\common\client"
#load "common\measure"


async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 23);
	input.Dump();
	
	
	
	input = new string(input.ToLower().Where(c => ".abcd".Contains(c)).ToArray());
	input.Dump();

	bool isPart2 = false;

	var input2 = "...........cdcabbda";
	
	if (isPart2)
	{
		input2 = "...........cdddccbabbabdaca";
	}

	var initialState = input2;
	
	var numRooms = 4;
	var roomSize = isPart2 ? 4 : 2;
	var map = new Map(roomSize);

	Dictionary<int, (int Start, int End, int Moves, int[] Cells)[]> ComputePaths()
	{
		var hallwayToAboveRoom = new[]
		{
			new[] { 0, 1, 2 },
			new[] { 0, 1, 2, 3, 4 },
			new[] { 0, 1, 2, 3, 4, 5, 6 },
			new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
			new[] { 1, 2 },
			new[] { 1, 2, 3, 4 },
			new[] { 1, 2, 3, 4, 5, 6 },
			new[] { 1, 2, 3, 4, 5, 6, 7, 8 },
			new[] { 3, 2 },
			new[] { 3, 4 },
			new[] { 3, 4, 5, 6 },
			new[] { 3, 4, 5, 6, 7, 8 },
			new[] { 5, 4, 3, 2 },
			new[] { 5, 4 },
			new[] { 5, 6 },
			new[] { 5, 6, 7, 8 },
			new[] { 7, 6, 5, 4, 3, 2 },
			new[] { 7, 6, 5, 4 },
			new[] { 7, 6 },
			new[] { 7, 8 },
			new[] { 9, 8 },
			new[] { 9, 8, 7, 6 },
			new[] { 9, 8, 7, 6, 5, 4 },
			new[] { 9, 8, 7, 6, 5, 4, 3, 2 },
			new[] { 10, 9, 8 },
			new[] { 10, 9, 8, 7, 6 },
			new[] { 10, 9, 8, 7, 6, 5, 4 },
			new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 },
		};

		var aboveRoomToRoom = Enumerable.Range(0, numRooms)
			.SelectMany(i => new[]
			{
				new[] { 2 * (i+1) }
					.Concat(Enumerable.Range(0, roomSize)
						.Select(x => x + 11)
						.Select(e => e + i * roomSize))
					.ToArray(),
			})
			.SelectMany(p => Enumerable.Range(0, roomSize)
				.Select(i => p.Reverse().Skip(roomSize - i - 1).Reverse().ToArray()))
			.ToArray()
			;

		var allPathsToRoom = hallwayToAboveRoom.SelectMany(hallwayToAboveRoomPath =>
		{
			return aboveRoomToRoom
				.Where(p => p.First() == hallwayToAboveRoomPath.Last())
				.Select(p => hallwayToAboveRoomPath.Concat(p.Skip(1)).ToArray())
				.ToArray();
		});
		
		var allPathInfo = allPathsToRoom
			.Select(p => (Start: p.First(), End: p.Last(), Moves: p.Count() - 1, Cells: p))
			.ToArray();

		var allHallwayToRoomPathInfo = allPathInfo
			.Select(p => (Start: p.Start, End: p.End, Moves: p.Moves, Cells: p.Cells.Skip(1).ToArray()))
			.ToArray();

		var allRoomToHallwayPathInfo = allPathInfo
			.Select(p => (Start: p.End, End: p.Start, Moves: p.Moves, Cells: p.Cells.Reverse().Skip(1).ToArray()))
			.ToArray();
		
		var cellToPaths2 = allHallwayToRoomPathInfo.Concat(allRoomToHallwayPathInfo)
			.GroupBy(e => e.Start)
			.ToDictionary(e => e.Key, e => e.ToArray());
			
		return cellToPaths2;
	}

	var cellToPaths = ComputePaths();

	int[] RoomsForOccupant(int i)
	{
		return Enumerable.Range(0, roomSize)
			.Select(r => r + i * roomSize + 11)
			.ToArray();
	}

	var occupantToTargetRooms = new Dictionary<char, int[]>()
	{
		{ 'a', RoomsForOccupant(0) },
		{ 'b', RoomsForOccupant(1) },
		{ 'c', RoomsForOccupant(2) },
		{ 'd', RoomsForOccupant(3) },
	};
	
	
	//occupantToTargetRooms.Dump("occupantToTargetRooms");
	var occupantToMoveCost = new Dictionary<char, int>()
	{
		{ 'a', 1 },
		{ 'b', 10 },
		{ 'c', 100 },
		{ 'd', 1000 },
	};

	bool IsHallway(int id) => id <= 10;

	long ComputeAnswerSlow(Func<string, IEnumerable<(string NextState, int ExtraCost)>> NextStates)
	{
		var nextStates = new Queue<PathState>();
		nextStates.Enqueue(new PathState(initialState, 0));

		var stateToMinEnergy = new Dictionary<string, long>();

		long? minEnergy = null;
		while (nextStates.Count > 0)
		{
			var pathState = nextStates.Dequeue();

			var state = pathState.State;
			if (stateToMinEnergy.TryGetValue(state, out var existingMinEnergy) && pathState.EnergySpent >= existingMinEnergy)
			{
				continue;
			}
			else
			{
				stateToMinEnergy[state] = pathState.EnergySpent;
			}
			if (IsGoal(pathState.State))
			{
				//pathState.Dump("goal!");
				if (minEnergy == null || pathState.EnergySpent < minEnergy)
				{
					minEnergy = pathState.EnergySpent;
				}
			}
			else
			{
				foreach (var (newState, cost) in NextStates(state))
				{
					if (!stateToMinEnergy.ContainsKey(newState))
					{
						nextStates.Enqueue(new PathState(newState, pathState.EnergySpent + cost));
					}
				}
			}
		}
		
		return minEnergy.Value;
	}

	TimeSpan Time(Action a)
	{
		var sw = new Stopwatch();
		sw.Start();
		a();
		sw.Stop();
		return sw.Elapsed.Dump();
	}


	string SwapCells(string cells, int from, int to)
	{
		var newChars = cells
			.Select((c, id) =>
			{
				if (id == from) return cells[to];
				else if (id == to) return cells[from];
				else return c;
			});
		return new string(newChars.ToArray());
	}

	IEnumerable<(string NewState, int ExtraCost)> NextStates(string s)
	{
		var hallwayToRoomAnswers = new List<(string NewState, int ExtraCost)>();
		var roomToHallwayAnswers = new List<(string NewState, int ExtraCost)>();
		
		foreach (var (cell, id) in s.Select((c, i) => (c, i)))
		{
			if (cell == '.')
			{
				//$"{cell} skipping occupant null".Dump();
				continue;
			}
			var occupant = cell;

			if (IsHallway(id))
			{
//				var start = id;
//				var target = map.OccupantToPositionList[occupant];
//				
//				var i = target.Count - 1;
//				while (i >= 0 && s[map.PointToStateIndex(target[i])] == cell)
//				{
//					i--;
//				}
//
//				if (i >= 0)
//				{
//					var end = target[i];
//					var path = map.StartToEndToPaths[map.StatePointList[start]][target[0]];
//					var canMove = true;
//					foreach (var p in path.Skip(1))
//					{
//						if (s[map.StatePointList.IndexOf(p)] != '.')
//						{
//							canMove = false;
//							break;
//						}
//					}
//					
//					if (canMove)
//					{
//						var startP = map.StatePointList[start];
//						var cost = Math.Abs(end.Y - startP.Y) + Math.Abs(startP.X - end.X) * occupantToMoveCost[occupant];
//						hallwayToRoomAnswers.Add((SwapCells(s, start,map.PointToStateIndex(end)), cost));
//					}
//				}
				
				// Find valid moves to rooms.
				var targetCells = occupantToTargetRooms[occupant];
				var validPaths = cellToPaths[id]
					.Where(p => p.Cells.All(c => s[c] == '.'))
					.Where(p => map.OccupantToPositionList[occupant].Contains(map.StateIndexToPoint(p.End)))
					// Take the longest path (i.e. move deepest into the room possible)
					.OrderByDescending(p => p.Moves)
					.Take(1)
					.ToArray();
				foreach (var path in validPaths)
				{
					var cost = path.Moves * occupantToMoveCost[occupant];
					hallwayToRoomAnswers.Add((SwapCells(s, path.Start, path.End), cost));
				}
			}
			else
			{
				if (occupantToTargetRooms[occupant].Contains(id))
				{
					// occupant is in target room
					var targetRoomIds = occupantToTargetRooms[occupant];
					
					bool skipOccupant = true;
					
					var deeperRooms = targetRoomIds.SkipWhile(r => r != id).Skip(1);
					
					if (deeperRooms.All(r => s[r] == occupant))
					{
						continue;
					}
				}

				// Move from room to hallway
				var validPaths = cellToPaths[id]
					.Where(p => p.Cells.All(c => s[c] == '.'));
				foreach (var path in validPaths)
				{
					var cost = path.Moves * occupantToMoveCost[occupant];
					roomToHallwayAnswers.Add((SwapCells(s, path.Start, path.End), cost));
				}
			}
		}
		
		var states = hallwayToRoomAnswers
			.Concat(roomToHallwayAnswers)
			.Where(e => !IsStateUnsolvable(e.NewState))
			.ToArray();
		return states;
	}

	bool IsGoal(string s)
	{
		return s[11] == 'a'
			&& s[12] == 'a'
			&& s[13] == 'b'
			&& s[14] == 'b'
			&& s[15] == 'c'
			&& s[16] == 'c'
			&& s[17] == 'd'
			&& s[18] == 'd';
	}

	int HeuristicCostToEnd(string s)
	{
		var cost = 0;
		foreach (var (cell, id) in s.Select((c, i) => (c, i)))
		{
			if (cell == '.') continue;
			
			var targetRooms = occupantToTargetRooms[cell];
			if (!targetRooms.Contains(id))
			{
				var startPoint = map.StateIndexToPoint(id);
				var endPoint = map.StateIndexToPoint(targetRooms[0]);
				
				// Find shortest path to room disregarding other occupants
				var minDistanceTravelled = map.StartToEndToPaths[startPoint][endPoint].Length;
				cost += minDistanceTravelled * occupantToMoveCost[cell];
			}
		}
		return cost;
	}
	
	bool IsStateUnsolvable(string s)
	{
		// if an occupant is in the hallway, and its target room has N occupants that need to leave, but there are M spots in the hallway for the N occupants with M < N, then
		// it is impossible for the state to be solved.
		foreach (var (cell, id) in s.Select((c, i) => (c, i)))
		{
			// Occupant in hallway
			if (IsHallway(id) && cell != '.' && (id == 3 || id == 5 || id == 7))
			{
				//cell.Dump("cell");
				var filledSpaces = occupantToTargetRooms[cell]
					.Reverse()
					.TakeWhile(c => s[c] == cell)
					//.Dump("filledSpaces")
					.Count();
				//filledSpaces.Dump("filledSpaces");
				var occupantsToLeaveRoom = occupantToTargetRooms[cell]
					.Take(occupantToTargetRooms[cell].Count() - filledSpaces)
					.Select(c => s[c])
					.Where(o => o != '.');
				//occupantsToLeaveRoom.Dump("occupantsToLeaveRoom");
				
				// TODO: Find occupants (e.g. D) which are blocked by cell (A) due to it's position in the hallway blocking passage
				var occupantsBlockedByCell = 0;
				char[] sep1 = null;
				char[] sep2 = null;
				if (id == 3)
				{
					sep1 = "a".ToArray();
					sep2 = "bcd".ToArray();
					// separates A from BCD
					
				}
				else if (id == 5)
				{
					sep1 = "ab".ToArray();
					sep2 = "cd".ToArray();
					// separates AB from CD
				}
				else if (id == 7)
				{
					sep1 = "abc".ToArray();
					sep2 = "d".ToArray();
					// separates ABC from D
				}

				var otherSep = sep1.Contains(cell) ? sep2 : sep1;
				occupantsToLeaveRoom = occupantsToLeaveRoom
					.Where(e => otherSep.Any(x => x == e));
				//occupantsToLeaveRoom.Dump("occupantsToLeaveRoom");
				var numOccupantsToLeave = occupantsToLeaveRoom.Count();
				//numOccupantsToLeave.Dump("numOccupantsToLeave");
				// Other occupants are in the target room
				if (numOccupantsToLeave != 0)
				{
					//"in if".Dump();
					// Find path from the occupent `cell` to its target room
					var roomStart = map.OccupantToPositionList[cell].First();
					var cellPos = map.StatePointList[id];
					var pathToTargetRoom = map.StartToEndToPaths[cellPos][roomStart];
					var aboveRoom = new Point(roomStart.X, roomStart.Y - 1);
					//  Then find all reachable hallway points that don't go through cell or stop on its path
					var frontier = new Queue<Point>();
					frontier.Enqueue(aboveRoom);
					var set = new HashSet<Point>();
					while (frontier.Any())
					{
						var p = frontier.Dequeue();
						if (!set.Add(p)) continue;
						
						foreach (var n in map.PointToNeighbours[p])
						{
							if (n != cellPos && map.HallwayPointList.Contains(n))
							{
								frontier.Enqueue(n);
							}
						}
					}

					//new { set, pathToTargetRoom, room = map.OccupantToPositionList[cell], noStopPoints = map.HallwayNoStopPoints }.Dump("info");
					// Then
					var hallwayHideyPoints = set//.Dump("set")
						.Except(pathToTargetRoom)
						//.Except(map.OccupantToPositionList[cell])
						.Except(map.HallwayNoStopPoints)
						//.Dump("hallwayHideyPoints")
						.Count()
						//.Dump("set except stuff")
						;
					
					if (hallwayHideyPoints < numOccupantsToLeave)
					{
						//map.VisualString(s).Dump($"unsolvable. {hallwayHideyPoints} hallwayHideyPoints < {numOccupantsToLeave} numOccupantsToLeave, cell {cell}, id {id}, state \"{s}\"");
						return true;
					}
					
					//var reachableHallwaySpaces = allRoomToHallwayPathInfo.Where(p => p.Start
					// if (reachableHallwaySpaces.Count() < occupantsToLeaveRoom)
				}
			}
		}

		return false;
	}

	//var target = 2000000;
	//var minCostPerStep = 7;
	//Time(() => DijkstrasAlgo(5, i => i == target, x => new[] { (x - 1, 1), (x + 1, 10), (x + 2, 15) }).Dump("Dijk 1"));
	//Time(() => AStarAlgo(5, i => i == target, x => new[] { (x - 1, 1), (x + 1, 10), (x + 2, 15) }, n => (target - n) * minCostPerStep).Dump("A* 1"));

	//minCostPerStep = 2;
	//Time(() => DijkstrasAlgo(5, i => i == target, x => new[] { (x - 1, 1), (x + 1, 2), (x + 2, 15) }).Dump("DIJK 2"));
	//Time(() => AStarAlgo(5, i => i == target, x => new[] { (x - 1, 1), (x + 1, 2), (x + 2, 15) }, n => (target- n)*minCostPerStep).Dump("A* 2"));

	//return;

	void DoStuff()
	{
		for (var i = 0; i < 10000; i++)
		{
			foreach (var _ in NextStates(initialState))
			{
				
			}
		}
	}
	
	Measure(DoStuff).Dump("Measure DoStuff");

	if (isPart2 == false)
	{
		// 14.1 seconds
		// 4.3 after optimising hallway nextstate
		var answer = Time(() => ComputeAnswerSlow(NextStates).Dump());
		answer.Dump("Part 1 custom");

		// 9.5 seconds when using HeuristicCostToEnd
		var answer_withh = Time(() => ComputeAnswerSlow(s => NextStates(s).OrderBy(s => HeuristicCostToEnd(s.NewState))));
		answer_withh.Dump("Part 1 custom with heuristic");
		
		var answer_withhandp = Time(() => ComputeAnswerSlow(s => NextStates(s).Where(s => !IsStateUnsolvable(s.NewState)).OrderBy(s => HeuristicCostToEnd(s.NewState))));
		answer_withhandp.Dump("Part 1 custom with heuristic and pruning");

		// 79.2 seconds
		var answer_2 = Time(() => DijkstrasAlgo(initialState, IsGoal, NextStates));
		answer_2.Dump("Part 1 dijkstras");

		// 19.8 seconds
		var answer_3 = Time(() => AStarAlgo(initialState, IsGoal, NextStates, HeuristicCostToEnd));
		answer_3.Dump("Part 1 A*");
	}

	//var answer2 = DijkstrasAlgo(initialState, IsGoal, NextStates);
	//answer2.Dump("answer 2 dijkstras");

	//var junk = "bb.a......." + ".daa" + "..bb" + "cccc" + "dadd";
	//var junk = ".......d...cdddccbabbab.aca";
	//IsStateUnsolvable(junk).Dump("isStateUnsolvable(junk)");
	//IsGoal(junk).Dump();

	//var answer3 = AStarAlgo(initialState, IsGoal, NextStates, HeuristicCostToEnd);
	//answer3.Dump("answer 3 A*");

	if (isPart2)
	{
		// Recompute answer from manual solution
		var s = 0L;
		var states = new[]
		{
			".. ....... .. cddd ccba bbab daca".Replace(" ", null),
			"a. .....b. bb cddd ccba .... daca".Replace(" ", null),
			"a. .....b. bb .ddd ..ba .ccc daca".Replace(" ", null),
			"aa ...b.b. bb .ddd .... .ccc daca".Replace(" ", null),
			"aa ....... .. .ddd bbbb .ccc daca".Replace(" ", null),
			"aa ...d... aa .ddd bbbb cccc ....".Replace(" ", null),
			"aa ....... aa .... bbbb cccc dddd".Replace(" ", null),
			".. ....... .. aaaa bbbb cccc dddd".Replace(" ", null),
		};

		for (var i = 0; i < states.Length - 1; i++)
		{
			s += DijkstrasAlgo(states[i], s => s == states[i + 1], NextStates).Dump("part 1 xxx");
			s.Dump("cumulative sum");
		}
	}

	//var minEnergy = ComputeAnswerSlow().Dump("part1");
	
	
}

int DijkstrasAlgo<TNode>(TNode source, Func<TNode, bool> IsGoal, Func<TNode, IEnumerable<(TNode, int Cost)>> SuccessorNodes)
{
	var dist = new Dictionary<TNode, int>();
	var prev = new Dictionary<TNode, TNode>();
	dist.Add(source, 0);

	var vertexQueue = new SimplePriorityQueue<TNode, int>();
	
	vertexQueue.EnqueueWithoutDuplicates(source, default);

	while (vertexQueue.Count > 0)
	{
		
		//vertexQueue.Select(e => new { Node = e, Priority = vertexQueue.GetPriority(e), Dist = dist[e] }).Dump("queue");
		//new { dist, prev }.Dump("dist and prev");
		var u = vertexQueue.Dequeue();
		//u.Dump("u");

		if (IsGoal(u))
		{
			return dist[u];
		}

		foreach (var (v, cost) in SuccessorNodes(u))
		{
			//new { u, v, cost }.Dump("u v cost");
			var alt = dist[u] + cost;
			if (!dist.TryGetValue(v, out var currentDist) || alt < currentDist)
			{
				dist[v] = alt;
				prev[v] = u;
				
				if (!vertexQueue.EnqueueWithoutDuplicates(v, alt))
				{
					if (alt < vertexQueue.GetPriority(v))
					{
						vertexQueue.UpdatePriority(v, alt);
					}
				}
			}
		}
	}
	
	throw new Exception("No path found");
}

int AStarAlgo<TNode>(TNode source, Func<TNode, bool> IsGoal, Func<TNode, IEnumerable<(TNode, int Cost)>> SuccessorNodes, Func<TNode, int> HeuristicCostToEnd)
{
	var dist = new Dictionary<TNode, int>();
	var distPlusH = new Dictionary<TNode, int>();
	var prev = new Dictionary<TNode, TNode>();
	dist.Add(source, 0);
	distPlusH.Add(source, dist[source] + HeuristicCostToEnd(source));

	var vertexQueue = new SimplePriorityQueue<TNode, int>();

	vertexQueue.EnqueueWithoutDuplicates(source, default);

	long iters = 0;

	while (vertexQueue.Count > 0)
	{
		var u = vertexQueue.Dequeue();
		iters++;
		if (iters % 10000 == 0)
		{
			new { iters, vertexQueue.Count, distU = dist[u], distPlusHU = distPlusH[u] }.Dump();
		}

		if (IsGoal(u))
		{
			return dist[u];
		}

		foreach (var (v, cost) in SuccessorNodes(u))
		{
			var altDist = dist[u] + cost;
			if (!dist.TryGetValue(v, out var currentDist) || altDist < currentDist)
			{
				dist[v] = altDist;
				distPlusH[v] = altDist + HeuristicCostToEnd(v);
				prev[v] = u;

				if (!vertexQueue.EnqueueWithoutDuplicates(v, distPlusH[v]))
				{
					vertexQueue.UpdatePriority(v, distPlusH[v]);
				}
			}
		}
	}
	
	throw new Exception("no goal found");
}


record Cell(int Id, char Occupant);

record PathState(string State, long EnergySpent);

record Point(int X, int Y);

class Map
{
	/*
	Sample map:
		
	#############
	#ooxoxoxoxoo#
	###C#C#B#D###
	  #D#C#B#A#
	  #D#B#A#C#
	  #D#A#B#A#
	  #########
	*/
	public int RoomSize;
	public List<Point> AllPointList;
	public List<Point> HallwayPointList;
	public Dictionary<char, List<Point>> OccupantToPositionList;
	public HashSet<Point> HallwayNoStopPoints;
	public List<Point> StatePointList;
	public Dictionary<Point, int> StatePointToIndex;
	public Dictionary<Point, Dictionary<Point, Point[]>> StartToEndToPaths;

	public Dictionary<Point, Point[]> PointToNeighbours;

	public int PointToStateIndex(Point p)
	{
		return StatePointToIndex[p];
	}
	
	public Point StateIndexToPoint(int stateIndex)
	{
		return StatePointList[stateIndex];
	}

	public Map(int roomSize)
	{
		RoomSize = roomSize;
		HallwayPointList = new List<Point>();
		HallwayNoStopPoints = new HashSet<Point>();
		OccupantToPositionList = new Dictionary<char, List<Point>>();

		for (var i = 0; i < 11; i++)
		{
			HallwayPointList.Add(new Point(i, 0));
			if (i == 2 || i == 4 || i == 6 || i == 8)
			{
				HallwayNoStopPoints.Add(new Point(i, 0));
			}
		}
		
		for (var roomI = 0; roomI < 4; roomI++)
		{
			var occupant = (char)('a' + roomI);
			var occupantPositions = new List<Point>();
			for (var y = 0; y < roomSize; y++)
			{
				occupantPositions.Add(new Point(roomI * 2 + 2, 1 + y));
			}
			OccupantToPositionList.Add(occupant, occupantPositions);
		}

		AllPointList = HallwayPointList
			.Concat(OccupantToPositionList['a'])
			.Concat(OccupantToPositionList['b'])
			.Concat(OccupantToPositionList['c'])
			.Concat(OccupantToPositionList['d'])
			.ToList();
			
		StatePointList = AllPointList
			//.Except(HallwayNoStopPoints)
			.ToList();
		StatePointToIndex = StatePointList
			.Select((p, i) => (p, i))
			.ToDictionary(e => e.p, e => e.i);

		PointToNeighbours = new Dictionary<Point, Point[]>();
		foreach (var point in AllPointList)
		{
			var neighbours = new[]
			{
				new Point(point.X, point.Y - 1),
				new Point(point.X, point.Y + 1),
				new Point(point.X + 1, point.Y),
				new Point(point.X - 1, point.Y),
			}.Where(p => AllPointList.Contains(p));
			PointToNeighbours.Add(point, neighbours.ToArray());
		}
		
		StartToEndToPaths = new Dictionary<Point, Dictionary<Point, Point[]>>();
		foreach (var point in AllPointList)
		{
			var allPathsFromPoint = FindPaths(point)
				.ToArray();
			StartToEndToPaths[point] = allPathsFromPoint
				.ToDictionary(e => e.Last(), e => e);
		}
	}

	public IEnumerable<Point[]> FindPaths(Point start, Point target)
	{
		return FindPaths(new Point[] { start }, target);
	}

	public IEnumerable<Point[]> FindPaths(Point[] path, Point target)
	{
		var p = path[path.Length - 1];
		if (p == target)
		{
			yield return path;
		}
		
		foreach (var neighbour in PointToNeighbours[p])
		{
			if (!path.Contains(neighbour))
			{
				var newPath = path
					.Concat(new[] { neighbour })
					.ToArray();
				foreach (var result in FindPaths(newPath, target))
				{
					yield return result;
				}
			}
		}
	}

	public IEnumerable<Point[]> FindPaths(Point start)
	{
		return FindPaths(new Point[] { start });
	}

	public IEnumerable<Point[]> FindPaths(Point[] path)
	{
		var p = path[path.Length - 1];
		yield return path;

		foreach (var neighbour in PointToNeighbours[p])
		{
			if (!path.Contains(neighbour))
			{
				var newPath = path
					.Concat(new[] { neighbour })
					.ToArray();
				foreach (var result in FindPaths(newPath))
				{
					yield return result;
				}
			}
		}
	}

	public char[,] VisualString(string s)
	{
		var grid = new char[RoomSize + 1, 12];
		for (var y = 0; y < RoomSize + 1; y++)
		{
			for (var x = 0; x < 12; x++)
			{
				var p = new Point(x, y);
				if (!AllPointList.Contains(p))
				{
					grid[y, x] = ' ';
				}
				else if (HallwayNoStopPoints.Contains(p))
				{
					grid[y, x] = '.';
				}
				else
				{
					var index = StatePointList.IndexOf(p);
					var occupant = s[index];
					grid[y, x] = occupant == '.' ? '.' : occupant;
				}
			}
		}
		return grid;
	}
}
