<Query Kind="Program">
  <NuGetReference>OptimizedPriorityQueue</NuGetReference>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Priority_Queue</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
</Query>

#load "..\common\client"
#load "..\common\measure"

const byte A = 0;
const byte B = 1;
const byte C = 2;
const byte D = 3;
const byte E = 5;

byte[] reusableState;
byte[] reusableState2;
byte[] reusableState3;

async Task Main()
{
	// TODO: Compare to https://github.com/orlp/aoc2021/blob/master/src/bin/day23.rs which apparently runs in about 7ms? Mine with his input and optimisation on takes 22ms allocating ~13MB
	// from https://www.reddit.com/r/adventofcode/comments/rmnozs/2021_day_23_solutions/

	// Even though I have a part2 variable, parts of this file assume part2 is always true.

	var part2 = true;
	reusableState = part2 ? new byte[23] : new byte[19];
	reusableState2 = part2 ? new byte[23] : new byte[19];
	reusableState3 = part2 ? new byte[23] : new byte[19];

	using var c = Configure2021();
	var myInput = await c.GetInput(dayNumber: 23);

	var orlpInput = @"#############
#...........#
###D#A#C#A###
  #D#C#B#B#
  #########";

	//var a = new byte[2]
	//{
	//	3, 4
	//};
	//
	//var b = new byte[2]
	//{
	//	3, 4
	//};

	//(a == b).Dump();
	//var obj1 = (object)PackState3(a);
	//var obj2 = (object)PackState3(b);
	//(obj1 == obj2).Dump();
	//
	//IEqualityComparer x = EqualityComparer<ByteArrayWithEquality>.Default;
	//x.Equals(obj1, obj2).Dump();

	//return;

	//var input = myInput;
	var input = orlpInput;

	if (input == myInput)
	{
		// This assertion is for my input.
		// <room> <room> <room> <room> <hallway>
		Debug.Assert("CDDD CCBA BBAB DACA ......." == FormatState(ParseInput(input, part2: true)));
	}

	var goalState = new byte[23];
	MakeGoalState(goalState);

	Measure(() => ParseInput(input, part2: true), out var state).Dump("ParseInput");
	FormatState(state).Dump("initial state");
	
	var distances = GetDistances();

	//AStarAlgo(state, n => IsGoal(n), n => NextStates(n, distances), n => Heuristic(n, distances));

	//if (FormatState(UnpackState2(PackState2(MakeInput("AAAA .BBB CCCC ..DD D..DB..")))) != "AAAA .BBB CCCC ..DD D..DB..") throw new Exception("oops");

	Func<TPacked, bool> MakeIsGoal<TPacked>(
		Func<byte[], bool> isGoal,
		Func<TPacked, byte[]> unpack)
	{
		return n => IsGoal(unpack(n));
	}
	
	Func<byte[], int> MakeAStarAlgo<TPacked>(
		Func<byte[], TPacked> pack,
		Func<TPacked, byte[]> unpack)
	{
		return state => AStarAlgo(
			pack(state),
			n => IsGoal(unpack(n)),
			n => NextStates(unpack(n), distances)
				.Select(e => (pack(e.State), e.MoveCost)),
			n => Heuristic(unpack(n), distances));
	}

	Func<byte[], int> MakeAStarAlgoV2<TPacked>(
		Func<byte[], TPacked> pack,
		Func<TPacked, byte[]> unpack)
	{
		return state => AStarAlgoV2(
			pack(state),
			n => IsGoal(unpack(n)),
			n => NextStates(unpack(n), distances)
				.Select(e => (pack(e.State), e.MoveCost)),
			n => Heuristic(unpack(n), distances));
	}

	Func<byte[], int> MakeAStarAlgoV2PackedFuncs<TPacked>(
		Func<byte[], TPacked> pack,
		Action<TPacked, byte[]> unpack)
		where TPacked : IEquatable<TPacked>
	{
		var goal = pack(goalState);
		var moves = new List<(TPacked, int)>();
		return state => AStarAlgoV2(
			pack(state),
			n => n.Equals(goal),
			//n => IsGoalPacked(n, unpack),
			n =>
			{
				moves.Clear();
				NextStatesPacked(n, distances, pack, unpack, moves);
				return moves;
			},
			n => HeuristicPacked(n, distances, unpack));
	}


	Func<byte[], int> MakeDijkstrasAlgo<TPacked>(
		Func<byte[], TPacked> pack,
		Func<TPacked, byte[]> unpack)
	{
		return state => DijkstrasAlgo(
			pack(state),
			n => IsGoal(unpack(n)),
			n => NextStates(unpack(n), distances)
				.Select(e => (pack(e.State), e.MoveCost)));
	}
	var astarPack = MakeAStarAlgo(PackState, UnpackState);

	int expectedResult = 0;
	if (input == orlpInput)
	{
		expectedResult = 47665;
	}
	else
	{
		expectedResult = 47193;
	}

	var algos = new Func<byte[], int>[]
	{
		// No packing doesn't work without equality comparer
		//Measure(() => AStarAlgo(state, n => IsGoal(n), n => NextStates(n, distances).Select(s => (s.State, s.MoveCost)), n => Heuristic(n, distances))).Item2.Dump("A* with heuristic (no packing)");
		// No packing works with equality comparer
		s => AStarAlgoV2WithEqualityComparer(s, n => IsGoal(n), n => NextStates(n, distances).Select(s => (s.State, s.MoveCost)), n => Heuristic(n, distances)),
		MakeAStarAlgo(PackState, UnpackState),
		MakeAStarAlgo(PackState2, UnpackState2),
		MakeAStarAlgo(PackState3, UnpackState3),
		MakeAStarAlgoV2(PackState, UnpackState),
		MakeAStarAlgoV2(PackState2, UnpackState2),
		MakeAStarAlgoV2(PackState3, UnpackState3),
		MakeAStarAlgoV2(PackState4, UnpackState4),
		MakeDijkstrasAlgo(PackState, UnpackState),
		MakeDijkstrasAlgo(PackState2, UnpackState2),
		MakeDijkstrasAlgo(PackState3, UnpackState3),
		MakeAStarAlgoV2PackedFuncs(PackState2, UnpackState2),
	};

	algos
		.Select(algo =>
		{
			GC.Collect();
			return Measure(() => algo(state));
		})
		.Select((result, i) => new { i, result.Result, result.Item2.ExecutionTime, result.Item2.TotalBytesAllocated })
		.Dump();
}

byte[] ParseInput(IEnumerable<char> input, bool part2)
{
	var roomChars = input.Where(e => "ABCD".Contains(e)).ToList();

	if (part2)
	{
		var extraLines = @"DCBA DBAC"
			.Replace(" ", "")
			.ToArray();

		roomChars = roomChars
			.Take(4)
			.Concat(extraLines)
			.Concat(roomChars.Skip(4))
			.ToList();
	}

	input.Dump("input");


	var rooms = Enumerable.Range(0, 4)
		.Select(e => new List<char>())
		.ToList();
	var i = 0;
	foreach (var roomChar in roomChars)
	{
		rooms[i].Add(roomChar);
		i = (i + 1) % 4;
	}

	string JoinRoom(IEnumerable<char> chars) => new string(chars.ToArray());
	string JoinRooms(IEnumerable<string> rooms) => string.Join(" ", rooms);

	var rooms2 = rooms.Select(r => JoinRoom(r));
	var result = JoinRooms(rooms2.Concat(new[] { "......." }));
	return MakeInput(result);
}

static string PackState(byte[] state)
{
	var chars = new char[state.Length];
	for (var i = 0; i < state.Length; i++)
	{
		chars[i] = (char)state[i];
	}
	return new string(chars);
}

static byte[] UnpackState(string packedState)
{
	var state = new byte[packedState.Length];
	for (var i = 0; i < state.Length; i++)
	{
		state[i] = (byte)packedState[i];
	}
	return state;
}

static (long a, long b) PackState2(byte[] state)
{
	var a = 0L;
	for (var i = 0; i < 16; i++)
	{
		a <<= 3;
		a |= state[i];
	}
	var b = 0L;
	for (var i = 0; i < 7; i++)
	{
		b <<= 3;
		b |= state[i+16];
	}

	return (a, b);
}

static byte[] UnpackState2((long a, long b) packedState)
{
	var (a, b) = packedState;

	var state = new byte[16+7];
	for (var i = 7 - 1; i >= 0; i--)
	{
		state[i+16] = (byte)(b & 7);
		b >>= 3;
	}
	for (var i = 16 - 1; i >= 0; i--)
	{
		state[i] = (byte)(a & 7);
		a >>= 3;
	}
	return state;
}

static void UnpackState2((long a, long b) packedState, byte[] state)
{
	var (a, b) = packedState;

	for (var i = 7 - 1; i >= 0; i--)
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

static (ulong a, byte b) PackState4(byte[] state)
{
	ulong a = 0;
	for (var i = 0; i < 21; i++)
	{
		a <<= 3;
		a |= state[i];
	}
	byte b = 0;
	for (var i = 0; i < 2; i++)
	{
		b <<= 3;
		b |= state[i + 21];
	}

	return (a, b);
}

static byte[] UnpackState4((ulong a, byte b) packedState)
{
	var (a, b) = packedState;

	var state = new byte[23];
	for (var i = 2 - 1; i >= 0; i--)
	{
		state[i + 21] = (byte)(b & 7);
		b >>= 3;
	}
	for (var i = 21 - 1; i >= 0; i--)
	{
		state[i] = (byte)(a & 7);
		a >>= 3;
	}
	return state;
}

struct ByteArrayWithEquality : IEquatable<ByteArrayWithEquality>
{
	public byte[] _bytes;
	public ByteArrayWithEquality(byte[] bytes)
	{
		_bytes = bytes;
	}

	public static bool operator ==(ByteArrayWithEquality a, ByteArrayWithEquality b)
	{
		return EqualsInternal(a, b);
	}

	public static bool operator !=(ByteArrayWithEquality a, ByteArrayWithEquality b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		throw new NotImplementedException();
	}

	public static bool EqualsInternal(ByteArrayWithEquality a, ByteArrayWithEquality other)
	{
		if (a._bytes.Length != other._bytes.Length)
		{
			return false;
		}
		for (var i = 0; i < a._bytes.Length; i++)
		{
			if (a._bytes[i] != other._bytes[i])
			{
				return false;
			}
		}
		return true;
	}

	public bool Equals(ByteArrayWithEquality other)
	{
		return EqualsInternal(this, other);
	}

	public override int GetHashCode()
	{
		//throw new NotImplementedException();
		var code = 0;
		for (var i = 0; i < _bytes.Length; i++)
		{
			code = code * 307 + _bytes[i];
		}

		return code;
	}

	public int GetHashCode(ByteArrayWithEquality obj)
	{
		throw new NotImplementedException();
	}
}

static ByteArrayWithEquality PackState3(byte[] state)
{
	var result = new ByteArrayWithEquality(state);
	return result;
}

static byte[] UnpackState3(ByteArrayWithEquality packedState)
{
	return packedState._bytes;
}

byte[] MakeInput(string input)
{
	bool IsValid(char c) => c == '.' || c >= 'A' && c <= 'D' || c >= 'a' && c <= 'd';

	var amorphids = input
		.Select((c, i) => (c, i))
		.Where(e => IsValid(e.c))
		.Select(e => e.c)
		.ToArray();

	byte AmorphidToInt(char c)
	{
		if (c == '.') return E;
		if (c == 'A' || c == 'a') return A;
		if (c == 'B' || c == 'b') return B;
		if (c == 'C' || c == 'c') return C;
		if (c == 'D' || c == 'd') return D;
		throw new Exception("Invalid input");
	}

	var state = amorphids
		.Select((c ,i) => AmorphidToInt(c))
		.ToArray();
		
	if (state.Length != 16+7) throw new Exception("Invalid input length");
	return state;
}

string FormatState(byte[] state)
{
	var chars = state
		.Select(c => c == E ? '.' : (char)('A' + c))
		.ToArray();
	var room1 = new string(chars.AsSpan(0, 4));
	var room2 = new string(chars.AsSpan(4, 4));
	var room3 = new string(chars.AsSpan(8, 4));
	var room4 = new string(chars.AsSpan(12, 4));
	var hallway = new string(chars.AsSpan(16));
	return string.Join(" ", new []{ room1, room2, room3, room4, hallway });
}

int Cost(byte amphipod)
{
	return amphipod switch
	{
		0 => 1,
		1 => 10,
		2 => 100,
		3 => 1000,
		_ => throw new Exception("nah invalid arg mate" + amphipod),
	};
}

byte[] Swap(byte[] state, int a, int b)
{
	var result = new byte[state.Length];
	Array.Copy(state, result, state.Length);
	result[a] = state[b];
	result[b] = state[a];
	return result;
}

byte[] Swap(byte[] state, byte[] result, int a, int b)
{
	Array.Copy(state, result, state.Length);
	result[a] = state[b];
	result[b] = state[a];
	return result;
}

IEnumerable<(byte[] State, int MoveCost)> NextStates(byte[] state, int[,] distances)
{
	for (var roomI = 0; roomI < 4; roomI++)
	{
		var end = roomI * 4 + 3;
		var canMoveIn = true;
		var lastAmphipodIndex = -1;
		var firstEmptyIndex = -1;
		var foundEmpty = false;
		for (var i = 0; i < 4; i++)
		{
			var cell = state[end - i];
			if (cell == E)
			{
				if (!foundEmpty)
				{
					firstEmptyIndex = end - i;
					foundEmpty = true;
				}
			}
			else
			{
				if (cell == roomI)
				{
					// Occupied by target amphipod
					lastAmphipodIndex = end - i;
				}
				else
				{
					// Occupied by someone that needs to move out
					canMoveIn = false;
					lastAmphipodIndex = end - i;
				}
			}
		}

		var hallwayLeftIndex = roomI + 1 + 16;
		var hallwayRightIndex = roomI + 2 + 16;

		if (canMoveIn)
		{
			// Generate moves from hallway into room
			var costPerMove = Cost((byte)roomI);
			var costOutOfRoom = firstEmptyIndex % 4 + 1;
			while (hallwayLeftIndex >= 16)
			{
				if (state[hallwayLeftIndex] != E)
				{
					if (state[hallwayLeftIndex] == roomI)
					{
						yield return (Swap(state, hallwayLeftIndex, firstEmptyIndex), distances[hallwayLeftIndex, firstEmptyIndex] * costPerMove);
					}
					break;
				}
				hallwayLeftIndex--;
			}
			while (hallwayRightIndex < 16 + 7)
			{
				if (state[hallwayRightIndex] != E)
				{
					if (state[hallwayRightIndex] == roomI)
					{
						yield return (Swap(state, hallwayRightIndex, firstEmptyIndex), distances[hallwayRightIndex, firstEmptyIndex] * costPerMove);
					}
					break;
				}
				hallwayRightIndex++;
			}
		}
		else
		{
			// Generate moves from room into hallway
			var costPerMove = Cost(state[lastAmphipodIndex]);
			var costOutOfRoom = lastAmphipodIndex % 4 + 1;
			while (hallwayLeftIndex >= 16)
			{
				if (state[hallwayLeftIndex] != E)
				{
					break;
				}
				yield return (Swap(state, hallwayLeftIndex, lastAmphipodIndex), distances[hallwayLeftIndex, lastAmphipodIndex] * costPerMove);
				hallwayLeftIndex--;
			}
			while (hallwayRightIndex < 16 + 7)
			{
				if (state[hallwayRightIndex] != E)
				{
					break;
				}
				yield return (Swap(state, hallwayRightIndex, lastAmphipodIndex), distances[hallwayRightIndex, lastAmphipodIndex] * costPerMove);
				hallwayRightIndex++;
			}
		}
	}
}

void NextStatesPacked<TPacked>(TPacked packedState, int[,] distances, Func<byte[], TPacked> packState, Action<TPacked, byte[]> unpackState, List<(TPacked State, int MoveCost)> moves)
{
	var state = reusableState2;
	unpackState(packedState, state);
	for (var roomI = 0; roomI < 4; roomI++)
	{
		var end = roomI * 4 + 3;
		var canMoveIn = true;
		var lastAmphipodIndex = -1;
		var firstEmptyIndex = -1;
		var foundEmpty = false;
		for (var i = 0; i < 4; i++)
		{
			var cell = state[end - i];
			if (cell == E)
			{
				if (!foundEmpty)
				{
					firstEmptyIndex = end - i;
					foundEmpty = true;
				}
			}
			else
			{
				if (cell == roomI)
				{
					// Occupied by target amphipod
					lastAmphipodIndex = end - i;
				}
				else
				{
					// Occupied by someone that needs to move out
					canMoveIn = false;
					lastAmphipodIndex = end - i;
				}
			}
		}

		var hallwayLeftIndex = roomI + 1 + 16;
		var hallwayRightIndex = roomI + 2 + 16;

		if (canMoveIn)
		{
			// Generate moves from hallway into room
			var costPerMove = Cost((byte)roomI);
			var costOutOfRoom = firstEmptyIndex % 4 + 1;
			while (hallwayLeftIndex >= 16)
			{
				if (state[hallwayLeftIndex] != E)
				{
					if (state[hallwayLeftIndex] == roomI)
					{
						Swap(state, reusableState3, hallwayLeftIndex, firstEmptyIndex);
						moves.Add((packState(reusableState3), distances[hallwayLeftIndex, firstEmptyIndex] * costPerMove));
					}
					break;
				}
				hallwayLeftIndex--;
			}
			while (hallwayRightIndex < 16 + 7)
			{
				if (state[hallwayRightIndex] != E)
				{
					if (state[hallwayRightIndex] == roomI)
					{
						Swap(state, reusableState3, hallwayRightIndex, firstEmptyIndex);
						moves.Add((packState(reusableState3), distances[hallwayRightIndex, firstEmptyIndex] * costPerMove));
					}
					break;
				}
				hallwayRightIndex++;
			}
		}
		else
		{
			// Generate moves from room into hallway
			var costPerMove = Cost(state[lastAmphipodIndex]);
			var costOutOfRoom = lastAmphipodIndex % 4 + 1;
			while (hallwayLeftIndex >= 16)
			{
				if (state[hallwayLeftIndex] != E)
				{
					break;
				}
				Swap(state, reusableState3, hallwayLeftIndex, lastAmphipodIndex);
				moves.Add((packState(reusableState3), distances[hallwayLeftIndex, lastAmphipodIndex] * costPerMove));
				hallwayLeftIndex--;
			}
			while (hallwayRightIndex < 16 + 7)
			{
				if (state[hallwayRightIndex] != E)
				{
					break;
				}
				Swap(state, reusableState3, hallwayRightIndex, lastAmphipodIndex);
				moves.Add((packState(reusableState3), distances[hallwayRightIndex, lastAmphipodIndex] * costPerMove));
				hallwayRightIndex++;
			}
		}
	}
}

bool IsGoal(byte[] state)
{
	for (var roomI = 0; roomI < 4; roomI++)
	{
		for (var i = 0; i < 4; i++)
		{
			if (state[roomI * 4 + i] != roomI) return false;
		}
	}

	return true;
}

bool IsGoalPacked<TPacked>(TPacked packedState, Action<TPacked, byte[]> unpack)
{
	unpack(packedState, reusableState);
	for (var roomI = 0; roomI < 4; roomI++)
	{
		for (var i = 0; i < 4; i++)
		{
			if (reusableState[roomI * 4 + i] != roomI) return false;
		}
	}

	return true;
}

void MakeGoalState(byte[] state)
{
	for (var roomI = 0; roomI < 4; roomI++)
	{
		for (var i = 0; i < 4; i++)
		{
			state[roomI * 4 + i] = (byte)roomI;
		}
	}
	for (var i = 16; i < state.Length; i++)
	{
		state[i] = E;
	}
}

int Heuristic(byte[] state, int[,] distances)
{
	var poses = new List<int>[4];
	poses[0] = new List<int>();
	poses[1] = new List<int>();
	poses[2] = new List<int>();
	poses[3] = new List<int>();
	for (var roomI = 0; roomI < 4; roomI++)
	{
		var needToMove = false;
		for (var x = 0; x < 4; x++)
		{
			var occupantI = roomI * 4 + 3 - x;
			var occupant = state[occupantI];
			if (!needToMove && occupant == roomI)
			{
				// amphipod doesn't have to move
			}
			else
			{
				needToMove = true;
				if (occupant != E)
				{
					poses[occupant].Add(occupantI);
				}
			}
		}
	}
	for (var i = 16; i < state.Length; i++)
	{
		if (state[i] != E)
		{
			poses[state[i]].Add(i);
		}
	}

	var cost = 0;
	for (var i = 0; i < 4; i++)
	{
		foreach (var pos in poses[i])
		{
			cost += Cost((byte)i) * distances[i * 4, pos];
		}
		if (poses[i].Count == 2)
		{
			cost += Cost((byte)i) * 1;
		}
		else if (poses[i].Count == 3)
		{
			cost += Cost((byte)i) * (1 + 2);
		}
		else if (poses[i].Count == 4)
		{
			cost += Cost((byte)i) * (1 + 2 + 3);
		}
	}
	return cost;
}

int HeuristicPacked<TPacked>(TPacked packedState, int[,] distances, Action<TPacked, byte[]> unpack)
{
	var state = reusableState;
	unpack(packedState, reusableState);
	var poses = new List<int>[4];
	poses[0] = new List<int>();
	poses[1] = new List<int>();
	poses[2] = new List<int>();
	poses[3] = new List<int>();
	for (var roomI = 0; roomI < 4; roomI++)
	{
		var needToMove = false;
		for (var x = 0; x < 4; x++)
		{
			var occupantI = roomI * 4 + 3 - x;
			var occupant = state[occupantI];
			if (!needToMove && occupant == roomI)
			{
				// amphipod doesn't have to move
			}
			else
			{
				needToMove = true;
				if (occupant != E)
				{
					poses[occupant].Add(occupantI);
				}
			}
		}
	}
	for (var i = 16; i < state.Length; i++)
	{
		if (state[i] != E)
		{
			poses[state[i]].Add(i);
		}
	}

	var cost = 0;
	for (var i = 0; i < 4; i++)
	{
		foreach (var pos in poses[i])
		{
			cost += Cost((byte)i) * distances[i * 4, pos];
		}
		if (poses[i].Count == 2)
		{
			cost += Cost((byte)i) * 1;
		}
		else if (poses[i].Count == 3)
		{
			cost += Cost((byte)i) * (1+2);
		}
		else if (poses[i].Count == 4)
		{
			cost += Cost((byte)i) * (1+2+3);
		}
	}
	return cost;
}

int[,] GetDistances()
{
	var distances = new int[16+7,16+7];

	for (var i = 0; i < 16 + 7; i++)
	{
		for (var j = i; j < 16 + 7; j++)
		{
			// both in rooms
			if (i < 16 && j < 16)
			{
				// same room
				if (i / 4 == j / 4)
				{
					//move to hallway and back
					var dist = i % 4 + j % 4 + 2;
					distances[i, j] = dist;
					distances[j, i] = dist;
				}
				// different rooms
				else
				{
					var dist = 0;
					var iPos = i;
					var jPos = j;
					if (i < 16)
					{
						dist += i % 4 + 1;
						iPos = i / 4 + 16;
					}
					if (j < 16)
					{
						dist += j % 4 + 1;
						jPos = j / 4 + 16;
					}
					dist += Math.Abs(j / 4 - i / 4) * 2;
					distances[i, j] = dist;
					distances[j, i] = dist;
				}
			}
			// both in hallway
			else if (i >= 16 && j >= 16)
			{
				// don't care. Can't move from hallway to hallway
			}
			else
			{
				if (i > j) throw new Exception("bro");
				if (i >= 16) throw new Exception("brooo");
				if (j < 16) throw new Exception("bruh");
				// i in room, j in hallway
				var costToLeaveRoom = i % 4 + 1;
				var costInHallway = (j-16) < i / 4 + 2
					? ((i / 4 + 1) - (j-16)) * 2 + 1
					: ((j-16) - (i / 4 + 2)) * 2 + 1;
				if (j == 16) costInHallway--;
				if (j == 16+6) costInHallway--;
				//costToLeaveRoom.Dump();
				//costInHallway.Dump();

				var cost = costToLeaveRoom + costInHallway;
				distances[i, j] = cost;
				distances[j, i] = cost;
				//return distances;
			}
		}
	}
	
	return distances;
}

// Uses SimplePriorityQueue
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
		if (iters % 400000 == 0)
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

				// TODO: Break ties such that the queue behaves like a stack (last in first out) to avoid searching all equal paths simultaneously.
				if (!vertexQueue.EnqueueWithoutDuplicates(v, distPlusH[v]))
				{
					vertexQueue.UpdatePriority(v, distPlusH[v]);
				}
			}
		}
	}
	
	throw new Exception("no goal found");
}

sealed class PriorityQueueNode<TValue> : StablePriorityQueueNode
{
	public TValue Value;
}

// Copy/paste of generic version, but with byte[] and with ArrayValueEqualityComparer used.
int AStarAlgoV2WithEqualityComparer(byte[] source, Func<byte[], bool> IsGoal, Func<byte[], IEnumerable<(byte[], int Cost)>> SuccessorNodes, Func<byte[], int> HeuristicCostToEnd)
{
	var stateToNode = new Dictionary<byte[], PriorityQueueNode<byte[]>>(new ArrayValueEqualityComparer());
	PriorityQueueNode<byte[]> GetNode(byte[] node)
	{
		if (stateToNode.TryGetValue(node, out var result))
		{
			return result;
		}
		var r = new PriorityQueueNode<byte[]>();
		r.Value = node;
		stateToNode.Add(node, r);
		return r;
	}

	var dist = new Dictionary<byte[], int>(new ArrayValueEqualityComparer());
	var distPlusH = new Dictionary<byte[], int>(new ArrayValueEqualityComparer());
	var prev = new Dictionary<byte[], byte[]>(new ArrayValueEqualityComparer());
	dist.Add(source, 0);
	distPlusH.Add(source, dist[source] + HeuristicCostToEnd(source));

	//var vertexQueue = new SimplePriorityQueue<TNode, int>();
	var vertexQueue = new StablePriorityQueue<PriorityQueueNode<byte[]>>(40000);
	//var vertexQueue = new FastPriorityQueue<PriorityQueueNode<byte[]>>(40000);

	vertexQueue.Enqueue(GetNode(source), default);

	long iters = 0;

	while (vertexQueue.Count > 0)
	{
		var u = vertexQueue.Dequeue().Value;
		iters++;
		if (iters % 400000 == 0)
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

				// TODO: Break ties such that the queue behaves like a stack (last in first out) to avoid searching all equal paths simultaneously.
				var n = GetNode(v);
				if (vertexQueue.Contains(n))
				{
					vertexQueue.UpdatePriority(n, distPlusH[v]);
				}
				else
				{
					vertexQueue.Enqueue(n, distPlusH[v]);
				}
			}
		}
	}

	throw new Exception("no goal found");
}

// Uses StablePriorityQueue
int AStarAlgoV2<TNode>(TNode source, Func<TNode, bool> IsGoal, Func<TNode, IEnumerable<(TNode, int Cost)>> SuccessorNodes, Func<TNode, int> HeuristicCostToEnd)
{
	var stateToNode = new Dictionary<TNode, PriorityQueueNode<TNode>>();
	PriorityQueueNode<TNode> GetNode(TNode node)
	{
		if (stateToNode.TryGetValue(node, out var result))
		{
			return result;
		}
		var r = new PriorityQueueNode<TNode>();
		r.Value = node;
		stateToNode.Add(node, r);
		return r;
	}

	var dist = new Dictionary<TNode, int>();
	var distPlusH = new Dictionary<TNode, int>();
	var prev = new Dictionary<TNode, TNode>();
	dist.Add(source, 0);
	distPlusH.Add(source, 0 + HeuristicCostToEnd(source));

	//var vertexQueue = new SimplePriorityQueue<TNode, int>();
	var vertexQueue = new StablePriorityQueue<PriorityQueueNode<TNode>>(40000);
	//var vertexQueue = new FastPriorityQueue<PriorityQueueNode<TNode>>(40000);

	vertexQueue.Enqueue(GetNode(source), default);

	long iters = 0;

	while (vertexQueue.Count > 0)
	{
		var u = vertexQueue.Dequeue().Value;
		iters++;
		if (iters % 400000 == 0)
		{
			new { iters, vertexQueue.Count, distU = dist[u], distPlusHU = distPlusH[u] }.Dump();
		}

		if (IsGoal(u))
		{
			iters.Dump("iters");
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

				// TODO: Break ties such that the queue behaves like a stack (last in first out) to avoid searching all equal paths simultaneously.
				var n = GetNode(v);
				if (vertexQueue.Contains(n))
				{
					vertexQueue.UpdatePriority(n, distPlusH[v]);
				}
				else
				{
					vertexQueue.Enqueue(n, distPlusH[v]);
				}
			}
		}
	}
	
	throw new Exception("no goal found");
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