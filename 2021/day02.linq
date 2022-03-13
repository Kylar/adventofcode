<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(2);
	
	var lines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToArray();
	//lines.Dump();	
	
	var x = lines
		.Select(line => line.Split())
		.Select(e => (e[0], int.Parse(e[1])))
	.Dump();
	
	var horz = x.Where(e => e.Item1 == "forward").Select(e => e.Item2).Sum();
	var depth = x.Where(e => e.Item1 == "up").Select(e => -e.Item2).Sum()
		+ x.Where(e => e.Item1 == "down").Select(e => e.Item2).Sum();
	(horz * depth).Dump();

	var aim = 0;
	var depth2 = 0;
	var horz2 = 0;
	foreach (var item in x)
	{
		if (item.Item1 == "down")
		{
			aim += item.Item2;
		}
		if (item.Item1 == "up")
		{
			aim -= item.Item2;
		}
		if (item.Item1 == "forward")
		{
			horz2 += item.Item2;
			depth2 += aim * item.Item2;
		}
	}
	(horz2 * depth2).Dump();

	//var aimDepth = x.Aggregate((0, new List<(string, int)>()), (acc, item) =>
	//{
	//	
	//var newAim = acc + item.Item1 == "down"
	//	? acc + item.Item2
	//	: item.Item1 == "up"
	//	? acc - item.Item2
	//	: acc;
	//	return (newAim, item);
	//}
	
	
	// Part1
	
	
	// Part2

	// Notes after completion:
	// Wasted heaps of time trying to use Aggregate() instead of just looping and mutating an aim variable.
	// Wasted some time because I created horz2 and depth2 but typed horz and depth. Maybe I should have reused the other vars?
}
