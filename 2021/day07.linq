<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 7);
	//input.Dump("input");

	var textLines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);
	var nums = textLines.Single().Split(",").Select(e => int.Parse(e)).ToArray();
	//nums.Dump("nums");
	var average = (double)nums.Sum() / nums.Count();
	var bestPos = Math.Round(average);
	average.Dump();
	// 353240 wrong part1
	// 336120 correct p1
	nums.Select(n => Math.Abs(n - bestPos + 2)).Sum().Dump("part1");
	Enumerable.Range(0, 1500)
		.Select(i => (i, sum: nums.Sum(n => Math.Abs(n - i))))
		.OrderBy(e => e.sum)
		.First()
		.Dump("part1");

	Enumerable.Range(0, 1500)
		//.Select(i => (i, sum: nums.Sum(n => Math.Abs(n - i))))
		.Select(i => (i, sum: nums.Sum(n =>
		{
			var dist = Math.Abs(n - i);
			if (dist == 0) return 0;
			var cost = (dist + 1) * dist / 2.0;
			return cost;
		})))
		.OrderBy(e => e.sum)
		.First()
		// 352820 wrong part2
		// 96864235 part2 correct
		.Dump("part2");
}
