<Query Kind="Program">
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
</Query>

#load ".\minheapv3"
#load "..\common\measure"

// Port of orlp's solution

// Kyle's note: The heuristic underestimates the cost a bit. For the initial state, C is in the correct room but blocking something else,
// so actually it needs to move at a minimum 5 extra times (up, left/right, right/left, down, down).

byte[] reusableState1;
byte[] reusableState2;
byte[] reusableState3;

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

	Span<bool> fillReady = stackalloc bool[4];

	void try_gen_move(int hallway, int room, bool to_room)
	{
		var pathStart = Math.Min(hallway, 2 + 2 * room);
		var pathEnd = Math.Max(hallway, 2 + 2 * room);

		for (var i = pathStart; i <= pathEnd; i++)
		{
			// if path is obstructed
			if (i != hallway && state[i] != 0) return;
		}

		var path_len = pathEnd - pathStart + 1;

		if (to_room)
		{
			// from hallway to room.
			var vacant_idx = (room_first_occupied[room] ?? room_size) - 1;
			moves.Add((hallway, room_start(room) + vacant_idx, path_len + vacant_idx));
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
				
				// Are we moving to a hallway position that is between the current room and the target room?
				var direct_route = 2 + 2 * Math.Min(room, target_room) < hallway && hallway < 2 + 2 * Math.Max(room, target_room);

				// There are many ways to go out of the room and straight into the target room... this only allows the shortest move out of the room. Tbh this seems like an invalid optimisation...
				if (path_len > 2 && direct_route) return;

				//if (path_len <= 2 || !direct_route)
				{
					moves.Add((room_start(room) + occupied_idx, hallway, path_len + occupied_idx));
				}
			}
		}
	}

	for (var room = 0; room < 4; room++)
	{
		fillReady[room] = true;
		var room_start_index = room_start(room);
		for (var i = 0; i < room_size; i++)
		{
			var b = state[room_start_index + i];
			if (!(b == 0 || b == room + 1))
			{
				fillReady[room] = false;
				break;
			}
		}
	}

	foreach (var hallway in hallwayIndices)
	{
		if (state[hallway] > 0)
		{
			var target_room = state[hallway] - 1;
			if (fillReady[target_room])
			{
				try_gen_move(hallway, target_room, true);
			}
		}
		else
		{
			foreach (var room in roomIndices)
			{
				// This check wasn't here before. It was previously generating moves for amphipods in their final room position, into the hallway.
				if (!fillReady[room])
				{
					try_gen_move(hallway, room, false);
				}
			}
		}
	}
}

int heuristic_fuel_cost(byte[] state)
{
	var N = state.Length;

	// Compute cost as if all amphipods can phase through eachother.
	//var room_size = (N - 11) / 4;
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


// Edited to also add a cost when an amphipod is in the correct room but needs to move out
int heuristic_fuel_cost_v2(byte[] state)
{
	var N = state.Length;
	
	// Compute cost as if all amphipods can phase through eachother.
	//var room_size = (N - 11) / 4;
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

	// added
	for (var room = 0; room < 4; room++)
	{
		var offset = room_size - 1;
		var blocked = false;
		while (offset >= 0)
		{
			var i = 11 + room_size * room + offset;
			var target_room = state[i] - 1;
			if (state[i] == 0)
			{
				offset--;
				continue;
			}

			if (target_room != room)
			{
				// move out of this room, into another room
				num_not_in_room[target_room] += 1;
				var hallway_path_len = 2 * Math.Abs(room - target_room);
				var exit_dist = 1 + offset;
				hcost += (exit_dist + hallway_path_len) * POW10[state[i] - 1];

				blocked = true;
				offset--;
				continue;
			}
			
			if (target_room == room && blocked)
			{
				num_not_in_room[target_room]++;
				// move out of this room, sideways and back to be in front of this room again.
				var dist = offset + 2;
				hcost += POW10[target_room] * dist;
			}
			else
			{
			}
			offset--;
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

void make_goal_state(byte[] state)
{
	for (var i = 0; i < 11; i++) state[i] = 0;
	for (var room = 0; room < 4; room++)
	{
		for (var offset = 0; offset < room_size; offset++)
		{
			state[11 + room * room_size + offset] = (byte)(room + 1);
		}
	}
}

int? astar_fuel_cost(byte[] initial_state)
{
	/* original in gen_moves, moved here to compute only once */
	N = initial_state.Length;
	room_size = (N - 11) / 4;
	/* end gen_moves code */

	// Initialise some arrays that can be used to avoid allocations.
	reusableState1 = new byte[N];
	reusableState2 = new byte[N];
	reusableState3 = new byte[N];

	var to_visit = new MinHeap<HeapEntry>();
	to_visit.Add(new HeapEntry(0, 0, Pack(initial_state)));

	var min_cost = new Dictionary<(long, long), long>()
	{
		{ Pack(initial_state), 0 },
	};
	var moves = new List<(int, int, int)>();
	var iters = 0;
	
	make_goal_state(reusableState1);
	var goal = Pack(reusableState1);

	var _state = reusableState2;
	var new_state = reusableState3;

	while (to_visit.Count > 0)
	{
		iters++;
		var (_hcost, cost, _packedState) = to_visit.RemoveMin();
		
		if (min_cost.TryGetValue(_packedState, out var min_cost_value) && cost > min_cost_value)
		{
			continue; // We got a better estimate in the meantime.
		}
		else if (_packedState == goal)
		{
			//to_visit.list.Capacity.Dump("final heap capacity");
			//min_cost.Count.Dump("final count of min_cost");
			//numStates.Dump("final new states created (not counting initial");
			//System.Runtime.InteropServices.Marshal.SizeOf(typeof(HeapEntry)).Dump("HeapEntry size");
			iters.Dump("num iters");
			return cost;
		}

		UnpackState2(_packedState, _state);
		gen_moves(_state, moves);

		foreach (var tuple in moves)
		{
			var (from, to, dist) = tuple;
			numStates++;
			
			// Comment this block out if uncommenting the block below with SwapPacked()
			Array.Copy(_state, new_state, _state.Length);
			var amphi = new_state[from];
			new_state[to] = amphi;
			new_state[from] = 0;
			var new_packed_state = Pack(new_state);

			// Alternative to swap without unpacking. But both versions of SwapPacked are slower. (also uncomment line within if below when uncommenting this)
			//var new_packed_state = SwapPacked(N, _packedState, from, to);
			//var amphi = GetPackedValue(N, new_packed_state, to);

			// Alternative #2, about same speed as the Array.Copy =\
			//var new_packed_state = SwapPackedV2(N, _packedState, from, to, out var amphi);

			var new_cost = cost + dist * POW10[amphi - 1];
			if (!min_cost.TryGetValue(new_packed_state, out var min_cost_value2) || new_cost < min_cost_value2)
			{
				min_cost[new_packed_state] = new_cost;
				//UnpackState2(new_packed_state, new_state);
				var heuristic_cost = new_cost + heuristic_fuel_cost_v2(new_state);
				to_visit.Add(new HeapEntry(heuristic_cost, new_cost, new_packed_state));
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

	var st = parse_state(string.Join("", part2_input), 11 + 4 * 4);
	//st.Dump();
	var packedState = Pack(st);
	var packedState2 = packedState;
	
	numStates = 0;
	//Measure(() => parse_state(string.Join("", part2_input), 11 + 4 * 4), out var part2input).Dump("parse part2 input measurement");
	// This was allocating 10.5MB
	// The heap capacity ends at 8192 * 16 bytes = 131KB, and it allocates twice as much because it allocates 4k items + 2k items + 1k items etc. as capacity increases = 260KB
	// The number of states created is 23165 x 23 bytes = 533KB, but actually the byte arrays take up 48 bytes, so it's 1112KB
	// The min_cost dictionary allocates about 1MB total (500MB final size and approx 14000 entries = ~36 bytes per entry)
	// So that's 0.25MB + 1MB + 1MB = 2.25MB, wtf is allocating the other 8MB? Answer: Indexing a byte array by a Range object was allocating heaps of memory... now it's at 2.5MB
	// Edit: After more optimisations, this now only allocates ~1.4MB
	//Measure(() => astar_fuel_cost(part2input)).Dump("part2 solve only measurement");

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
	public (long, long) state;

	public HeapEntry(int hcost, int cost, (long, long) state)
	{
		this.hcost = hcost;
		this.cost = cost;
		this.state = state;
	}

	public void Deconstruct(out int hcost, out int cost, out (long, long) state)
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

static (long a, long b) Pack(byte[] state)
{
	var a = 0L;
	for (var i = 0; i < 16; i++)
	{
		a <<= 3;
		a |= state[i];
	}
	var b = 0L;
	for (var i = 0; i < state.Length - 16; i++)
	{
		b <<= 3;
		b |= state[i + 16];
	}

	return (a, b);
}

static void UnpackState2((long a, long b) packedState, byte[] state)
{
	var (a, b) = packedState;

	for (var i = state.Length - 16 - 1; i >= 0; i--)
	{
		state[i + 16] = (byte)(b & 7);
		b >>= 3;
	}
	for (var i = 16 - 1; i >= 0; i--)
	{
		state[i] = (byte)(a & 7);
		a >>= 3;
	}
}

byte GetPackedValue(int N, (long a, long b) packedState, int i)
{
	var (a, b) = packedState;

	if (i < 16)
	{
		return (byte)((a >> (3 * (16 - 1 - i))) & 7);
	}
	return (byte)(b >> (3 * (N - 1 - i)) & 7);
}

(long, long) SetPackedValue(int N, (long a, long b) packedState, int i, byte val)
{
	var (a, b) = packedState;

	if (i < 16)
	{
		ulong clearBits = ~(7UL << (3 * (16 - 1 - i)));
		ulong setBits = (ulong)(val) << (3 * (16 - 1 - i));
		ulong x = (ulong)a;
		x = (x & clearBits) | setBits;
		return ((long)x, b);
	}
	else
	{
		ulong clearBits = ~(7UL << (3 * (N - 1 - i)));
		ulong setBits = (ulong)(val) << (3 * (N - 1 - i));
		ulong x = (ulong)b;
		x = (x & clearBits) | setBits;
		//new { i, val, clearBits, setBits, a, b, x }.Dump();
		return (a, (long)x);
	}
}

(long, long) SwapPacked(int N, (long a, long b) packedState, int p1, int p2)
{	
	byte p1Val = GetPackedValue(N, packedState, p1);
	byte p2Val = GetPackedValue(N, packedState, p2);
	packedState = SetPackedValue(N, packedState, p1, p2Val);
	packedState = SetPackedValue(N, packedState, p2, p1Val);
	return packedState;
}

(long, long) SwapPackedV2(int N, (long a, long b) packedState, int p1, int p2, out byte p1Val)
{
	//byte p1Valx = GetPackedValue(N, packedState, p1);
	//byte p2Valx = GetPackedValue(N, packedState, p2);
	//var altPackedState = SetPackedValue(N, packedState, p1, p2Valx);
	//altPackedState = SetPackedValue(N, altPackedState, p2, p1Valx);
	////return packedState;
	
	ulong a = (ulong)packedState.a;
	ulong b = (ulong)packedState.b;

	var p1Shift = p1 < 16
		? 3 * (16 - 1 - p1)
		: 3 * (N - 1 - p1);
	var p2Shift = p2 < 16
		? 3 * (16 - 1 - p2)
		: 3 * (N - 1 - p2);

	p1Val = p1 < 16
		? (byte)((a >> p1Shift) & 7)
		: (byte)((b >> p1Shift) & 7);

	byte p2Val = p2 < 16
		? (byte)((a >> p2Shift) & 7)
		: (byte)((b >> p2Shift) & 7);

	var p1ClearBits = ~(7UL << p1Shift);
	var p2ClearBits = ~(7UL << p2Shift);

	var p1SetBits = (ulong)(p2Val) << p1Shift;
	var p2SetBits = (ulong)(p1Val) << p2Shift;

	//void DumpInfo()
	//{
	//	new { ab=(a, b), altPackedState, p1, p2, p1Shift, p2Shift, p1Val, p2Val, p1ClearBits, p2ClearBits, p1SetBits, p2SetBits }.Dump();
	//	throw new Exception("oops");
	//}
	//if (p1Val != p1Valx) DumpInfo();
	//if (p2Val != p2Valx) DumpInfo();
	//
	// Set value
	
	//if (p1 < 16)
	//{
	//	if (p2 < 16)
	//	{
	//		// assign a only
	//		a = ((a & p1ClearBits & p2ClearBits) | p1SetBits | p2SetBits);
	//	}
	//	else
	//	{
	//		a = ((a & p1ClearBits) | p1SetBits);
	//		b = ((b & p2ClearBits) | p2SetBits);
	//	}
	//}
	//else
	//{
	//	if (p2 < 16)
	//	{
	//		b = ((b & p1ClearBits) | p1SetBits);
	//		a = ((a & p2ClearBits) | p2SetBits);
	//	}
	//	else
	//	{
	//		// assign b only
	//		b = ((b & p1ClearBits & p2ClearBits) | p1SetBits | p2SetBits);
	//	}
	//}

	if (p1 < 16)
	{
		a = ((a & p1ClearBits) | p1SetBits);
	}
	else
	{
		b = ((b & p1ClearBits) | p1SetBits);
	}

	if (p2 < 16)
	{
		a = ((a & p2ClearBits) | p2SetBits);
	}
	else
	{
		b = ((b & p2ClearBits) | p2SetBits);
	}
	
	return ((long)a, (long)b);
}

