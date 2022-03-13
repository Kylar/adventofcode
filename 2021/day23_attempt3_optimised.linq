<Query Kind="Program">
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
</Query>

#load ".\minheapv2"
#load "..\common\measure"


// Port of orlp's solution

// Kyle's note: The heuristic underestimates the cost a bit. For the initial state, C is in the correct room but blocking something else,
// so actually it needs to move at a minimum 5 extra times (up, left/right, right/left, down, down).


int[] POW10 = new int[] { 1, 10, 100, 1000 };

/*
State: for logic simplificy we use 0, 1, 3, 5, 7, 9, 10 as the valid hallway positions.
Indices 2, 4, 6, 8 are unused and are always Empty, 11+ are the rooms, with each room contiguous.
Value encoding: 0 = Empty, 1 = A, 2 = B, 3 = C, 4 = D.

    ###################################
	#00 01 02 03 04 05 06 07 08 09 10 #
	###### 11 ## 13 ## 15 ## 17 #######
	     # 12 ## 14 ## 16 ## 18 #
		 ########################
*/

int[] roomIndices = new[] { 0, 1, 2, 3 };
int[] hallwayIndices = new[] { 0, 1, 3, 5, 7, 9, 10 };

static int N;
static int room_size;
static int room_start(int r) => 11 + r * room_size;
static Range[] room_state_ranges = new System.Range[4];
int?[] room_first_occupied = new int?[4];

static int numStates = 0;

void gen_moves(byte[] state, List<(int start, int end, int dist)> moves)
{
	//int? get_room_first_occupied(Span<byte> room_state)
	//{
	//	for (var i = 0; i < room_size; i++)
	//	{
	//		if (room_state[i] > 0)
	//		{
	//			return i;
	//		}
	//	}
	//	return null;
	//}
	//
	//for (var room = 0; room < 4; room++)
	//{
	//	room_first_occupied[room] = get_room_first_occupied(state[room_state_ranges[room]]);
	//}

	for (var room = 0; room < 4; room++)
	{
		room_first_occupied[room] = null;
		for (var i = 0; i < room_size; i++)
		{
			if (state[room_start(room) + i] > 0)
			{
				room_first_occupied[room] = i;
				break;
			}
		}
	}

	void try_gen_move(List<(int, int, int)> moves, int hallway, int room, bool to_room)
	{
		//var path_len = path.Count();
		var path_len = Math.Max(hallway, 2 + 2 * room) - Math.Min(hallway, 2 + 2 * room) + 1;
		//bool unobstructed() => path.All(i => i == hallway || state[i] == 0);
		bool unobstructed()
		{
			var end = Math.Max(hallway, 2  + 2 * room);
			for (var i = Math.Min(hallway, 2 + 2 * room); i <= end; i++)
			{
				if (i != hallway && state[i] != 0) return false;
			}
			return true;
			//return path.All(i => i == hallway || state[i] == 0);
		}

		if (to_room)
		{
			// from hallway to room.

			//var fill_ready = state[room_state_ranges[room]].All(b => b == 0 || b == room + 1);
			bool fill_ready = true;
			var room_start_index = room_start(room);
			for (var i = 0; i < room_size; i++)
			{
				var b = state[room_start_index + i];
				if (!(b == 0 || b == room + 1))
				{
					fill_ready = false;
					break;
				}
			}
			//foreach (var b in state[room_state_ranges[room]])
			//{
			//	if (!(b == 0 || b == room + 1))
			//	{
			//		fill_ready = false;
			//		break;
			//	}
			//}
			
			if (fill_ready && unobstructed())
			{
				var vacant_idx = (room_first_occupied[room] ?? room_size) - 1;
				moves.Add((hallway, room_start(room) + vacant_idx, path_len + vacant_idx));
			}
		}
		else
		{
			// from room to hallway.
			var nullable_occupied_idx = room_first_occupied[room];
			if (nullable_occupied_idx != null)
			{
				var occupied_idx = nullable_occupied_idx.Value;
				//var target_room = state[room_state_ranges[room]][occupied_idx] - 1;
				var target_room = state[room_start(room) + occupied_idx] - 1;

				// To reduce superfluous nodes if we're moving directly to our target
				// room, only ever take 1 step.
				
				//var direct_route = symm_range(2 + 2 * room, 2 + 2 * target_room).Contains(hallway);
				var direct_route = 2 + 2 * Math.Min(room, target_room) <= hallway && hallway <= 2 + 2 * Math.Max(room, target_room);

				if (!(direct_route && path_len > 2) && unobstructed())
				{
					moves.Add((room_start(room) + occupied_idx, hallway, path_len + occupied_idx));
				}
			}
		}
	}

	foreach (var hallway in hallwayIndices)
	{
		if (state[hallway] > 0)
		{
			try_gen_move(moves, hallway, state[hallway] - 1, true);
		}
		else
		{
			foreach (var room in roomIndices)
			{
				try_gen_move(moves, hallway, room, false);
			}
		}
	}
}

int heuristic_fuel_cost(byte[] state)
{
	var N = state.Length;
	
	// Compute cost as if all amphipods can phase through eachother.
	var room_size = (N - 11) / 4;
	var hcost = 0;
	Span<int> num_not_in_room = stackalloc int[4];
	// Cost to move inside the hallway to in front of the target room.
	for (var i = 0; i < 11; i++)
	{
		if (state[i] > 0)
		{
			var target_room = state[i] - 1;
			num_not_in_room[target_room] += 1;
			var dist = Math.Abs((i - (2 + 2 * target_room)));
			hcost += dist * POW10[state[i] - 1];
		}
	}

	// Cost to move from inside current room to hallway in front of target room.
	for (var room = 0; room < 4; room++)
	{
		for (var offset = 0; offset < room_size; offset++)
		{
			var i = 11 + room_size * room + offset;
			if (state[i] > 0)
			{
				var target_room = state[i] - 1;
				if (target_room != room)
				{
					num_not_in_room[target_room] += 1;
					var hallway_path_len = 2 * Math.Abs(room - target_room);
					var exit_dist = 1 + offset;
					hcost += (exit_dist + hallway_path_len) * POW10[state[i] - 1];
				}
			}
		}
	}

	// Total cost for k amphipods in front of room to enter.
	for (var i = 0; i < num_not_in_room.Length; i++)
	{
		var k = num_not_in_room[i];
		hcost += k * (k + 1) / 2 * POW10[i];
	}
	
	return hcost;
}

int? astar_fuel_cost(byte[] initial_state)
{
	/* original in gen_moves, moved here to compute only once */
	N = initial_state.Length;
	room_size = (N - 11) / 4;
	//room_state_ranges = roomIndices
	//	.Select(r => new Range(new Index(room_start(r)), new Index(room_start(r + 1))))
	//	.ToArray();
	//room_state_ranges[0] = new Range(new Index(room_start(0)), new Index(room_start(1)));
	//room_state_ranges[1] = new Range(new Index(room_start(1)), new Index(room_start(2)));
	//room_state_ranges[2] = new Range(new Index(room_start(2)), new Index(room_start(3)));
	//room_state_ranges[3] = new Range(new Index(room_start(3)), new Index(room_start(4)));
	/* end gen_moves code */

	var to_visit = new MinHeap<HeapEntry>();
	to_visit.Add(new HeapEntry(0, 0, initial_state));

	var min_cost = new Dictionary<byte[], long>(new ArrayValueEqualityComparer())
	{
		{ initial_state, 0 },
	};
	var moves = new List<(int, int, int)>();
	//var iters = 0;
	while (to_visit.Count > 0)
	{
		//iters++;
		var (_hcost, cost, _state) = to_visit.RemoveMin();
		
		bool IsIncrementing()
		{
			for (var i = 1; i < _state.Length; i++)
			{
				if (_state[i - 1] > _state[i]) return false;
			}
			return true;
		}

		//if (cost > (min_cost.TryGetValue(_state, out var min_cost_value) ? min_cost_value : long.MaxValue))
		if (min_cost.TryGetValue(_state, out var min_cost_value) && cost > min_cost_value)
		{
			continue; // We got a better estimate in the meantime.
		}
		//else if (_state.Zip(_state.Skip(1)).All(e => e.First <= e.Second))
		else if (IsIncrementing())
		{
			//to_visit.list.Capacity.Dump("final heap capacity");
			//min_cost.Count.Dump("final count of min_cost");
			//numStates.Dump("final new states created (not counting initial");
			//System.Runtime.InteropServices.Marshal.SizeOf(typeof(HeapEntry)).Dump("HeapEntry size");
			//iters.Dump("num iters");
			return cost;
		}

		gen_moves(_state, moves);

		foreach (var tuple in moves)
		{
			var (from, to, dist) = tuple;
			numStates++;
			//Measure(() => new byte[_state.Length], out var new_state).Dump();
			var new_state = new byte[_state.Length];
			Array.Copy(_state, new_state, _state.Length);
			var amphi = new_state[from];
			new_state[to] = amphi;
			new_state[from] = 0;
			var new_cost = cost + dist * POW10[amphi - 1];
			//if (new_cost < (min_cost.TryGetValue(new_state, out var min_cost_value2) ? min_cost_value2 : long.MaxValue))
			if (!min_cost.TryGetValue(new_state, out var min_cost_value2) || new_cost < min_cost_value2)
			{
				min_cost[new_state] = new_cost;
				var heuristic_cost = new_cost + heuristic_fuel_cost(new_state);
				to_visit.Add(new HeapEntry(heuristic_cost, new_cost, new_state));
			}
		}

		moves.Clear();
	}
	
	return null;
}

byte[] parse_state(string s, int state_size)
{
	var N = state_size;
	var state = new byte[state_size];
	var room_size = (N - 11) / 4;
	foreach (var (i, b) in s.Where(b => 'A' <= b && b <= 'D').Select((b, i) => (i, b)))
	{
		state[11 + room_size * (i % 4) + i / 4] = (byte)((b - 'A') + 1);
	}
	return state;
}

void Main()
{
	var input = File.ReadAllText(@"D:\projects\adventofcode\orlp-aoc2021\inputs\day23.txt");
	var start = DateTime.UtcNow;
	var part2_input = input.Split("\n").ToList();
	part2_input = part2_input.Take(3)
		.Concat(new[] { "#D#C#B#A#", "#D#B#A#C#" })
		.Concat(part2_input.Skip(3))
		.ToList();

	var part1 = astar_fuel_cost(parse_state(input, 11 + 2 * 4));
	
	numStates = 0;
	
	var part2start = DateTime.UtcNow;
	var part2 = astar_fuel_cost(parse_state(string.Join("", part2_input), 11 + 4 * 4));
	var part2Time = DateTime.UtcNow - part2start;

	numStates = 0;
	Measure(() => parse_state(string.Join("", part2_input), 11 + 4 * 4), out var part2input).Dump("parse part2 input measurement");
	// This was allocating 10.5MB
	// The heap capacity ends at 8192 * 16 bytes = 131KB, and it allocates twice as much because it allocates 4k items + 2k items + 1k items etc. as capacity increases = 260KB
	// The number of states created is 23165 x 23 bytes = 533KB, but actually the byte arrays take up 48 bytes, so it's 1112KB
	// The min_cost dictionary allocates about 1MB total (500MB final size and approx 14000 entries = ~36 bytes per entry)
	// So that's 0.25MB + 1MB + 1MB = 2.25MB, wtf is allocating the other 8MB? Answer: Indexing a byte array by a Range object was allocating heaps of memory... now it's at 2.5MB
	Measure(() => astar_fuel_cost(part2input)).Dump("part2 solve only measurement");

	Console.WriteLine("time {0}", DateTime.UtcNow - start);
	Console.WriteLine("part1: {0}", part1?.ToString() ?? "no part 1 solution");
	Console.WriteLine("part2: {0} in {1}", part2?.ToString() ?? "no part 2 solution", part2Time);
}

class ArrayValueEqualityComparer : IEqualityComparer<byte[]>
{
	public bool Equals(byte[] x, byte[] y)
	{
		if (x.Length != y.Length) return false;
		for (var i = 0; i < x.Length; i++)
		{
			if (x[i] != y[i]) return false;
		}
		return true;
	}

	public int GetHashCode(byte[] obj)
	{
		var hashCode = 0;
		for (var i = 0; i < obj.Length; i++)
		{
			hashCode = hashCode * 307 + obj[i];
		}
		return hashCode;
	}
}

struct HeapEntry : IComparable<HeapEntry>
{
	public int hcost;
	public int cost;
	public byte[] state;

	public HeapEntry(int hcost, int cost, byte[] state)
	{
		this.hcost = hcost;
		this.cost = cost;
		this.state = state;
	}

	public void Deconstruct(out int hcost, out int cost, out byte[] state)
	{
		hcost = this.hcost;
		cost = this.cost;
		state = this.state;
	}

	public int CompareTo(HeapEntry other)
	{
		return this.hcost - other.hcost;
	}
}
