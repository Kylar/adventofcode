<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 10);
	var xinput = @"[({(<(())[]>[[{[]{<()<>>
[(()[<>])]({[<{<<[]>>(
{([(<{}[<>[]}>{[]{[(<()>
(((({<>}<{<{<>}{[]{[]{}
[[<[([]))<([[{}[[()]]]
[{[{({}]{}}([{[{{{}}([]
{<[[]]>}<{[{[{[]{()[[[]
[<(<(<(<{}))><([]([]()
<{([([[(<>()){}]>(<<{{
<{([{{}}[<[[[<>{}]]]>[]]".Replace("\r\n", "\n");

	var textLines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);
	
	
	textLines.Dump();	
	
	textLines.Select(Solve).Dump();

	var cost = new Dictionary<char, long>()
	{
		{ ')', 3 },
		{ ']', 57 },
		{ '}', 1197 },
		{ '>', 25137 },
	};
	
	// 611574 wrong
	textLines
		.Select(Solve)
		.Where(s => !s.Valid)
		.Select(e => cost[e.FirstInvalidChar])
		//.Dump()
		.Sum()
		//.Dump("apart1")
		;

	var incomplete = textLines
		.Select(Solve)
		.Where(s => s.Valid)
		.ToArray()
		.Dump();
		
	var values = new Dictionary<char, int>
	{
		{ ')', 1 },
		{ ']', 2 },
		{ '}', 3 },
		{ '>', 4 },
	};
	var scores = incomplete.Select(e => e.CompletionString.Aggregate(0L, (acc, c) => acc * 5 + values[c]))
		.OrderBy(e => e)
		.Dump()
		.ToArray();

	// wrong 601165323
	// wrong 601165323 -- Fixed missing OrderBy() but got the same answer anyway? Weird coincidence or just failed somehow
	// correct 2904180541 -- Fixed scores being negative because I didn't use long
	var answer = scores[(scores.Length - 1) / 2].Dump("answer2");





}

(bool Valid, char FirstInvalidChar, int InvalidIndex, IEnumerable<char> CompletionString) Solve(string line)
{
	var open = "{([<";
	var close = "})]>";
	
	var stack = new Stack<char>();
	for (var i = 0; i < line.Length; i++)
	{
		var c = line[i];
		if (open.Contains(c))
		{
			//"open".Dump();
			stack.Push(c);
		}
		else if (close.Contains(c))
		{
			//"close".Dump();
			var lastC = stack.Pop();
			var lastCClose = close[open.IndexOf(lastC)];
			if (lastCClose != c)
			{
				//"mismatch".Dump();
				return (false, c, i, null);
			}
			else{
				//"match".Dump();
			}
		}
		else
		{
			throw new Exception("unexpected char");
		}
	}
	
	return (true, default, default, stack.Select(c => close[open.IndexOf(c)]));
}