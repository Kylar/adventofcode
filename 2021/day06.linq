<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 6);
	//input = "3,4,3,1,2";

	var textLines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);
	//textLines.Dump();
	var daysPerNewFish = 7;
	
	var timerToNumFish = textLines.Single().Split(",", StringSplitOptions.RemoveEmptyEntries)
		.Select(int.Parse)
		.GroupBy(e => e)
		.ToDictionary(e => e.Key, e => e.Count());
	
	timerToNumFish.Dump();

	timerToNumFish.Select(e => FishCreatedInDays(e.Key, 80) * e.Value + e.Value)
		//.Dump()
		.Sum()
		.Dump("answer part1")
		;
		
	var cache = new Dictionary<(int, int), long>();
	timerToNumFish.Select(e => FishCreatedInDays(cache, e.Key, 256) * e.Value + e.Value)
		//.Dump()
		.Sum()
		.Dump("answer part2")
		;

	BetterAnswer(timerToNumFish);
	BetterAnswer2(timerToNumFish, 80).Dump("better answer 2, part1");
	BetterAnswer2(timerToNumFish, 256).Dump("better answer 2, part2");

	//textLines.Count().Dump();
		
	//FishCreatedInDays(0, 10)
	//	.Dump()
	//	;
		
	// 1054701 wrong
	// 374694 wrong
	//374994 right part1
	
	
	// 1686252324092 right part2
}

long FishCreatedInDays(int timer, int days)
{
	long answer = 0;
	while (days > 0)
	{
		days--;
		timer--;
		if (timer == -1)
		{
			answer++;
			answer += FishCreatedInDays(8, days);
			timer = 6;
		}
		else
		{
		}
	}
	return answer;
}

long FishCreatedInDays(Dictionary<(int, int), long> cache, int timer, int days)
{
	var key = (timer, days);
	if (cache.TryGetValue(key, out var answer))
	{
		return answer;
	}
	
	answer = 0;
	
	while (days > 0)
	{
		days--;
		timer--;
		if (timer == -1)
		{
			answer++;
			answer += FishCreatedInDays(cache, 8, days);
			timer = 6;
		}
		else
		{
		}
	}
	cache.Add(key, answer);
	return answer;
}

long BetterAnswer(Dictionary<int, int> timerToNumFish)
{
	// timer is used as the index
	var numFish = new long[9];
	var nextDayNumFish = new long[9];
	for (var i = 0; i < 9; i++)
	{
		timerToNumFish.TryGetValue(i, out var n);
		numFish[i] = n;
	}
	
	var daysPassed = 0;
	while (daysPassed <= 255)
	{
		daysPassed++;

		Array.Clear(nextDayNumFish, 0, nextDayNumFish.Length);
		for (var i = 0; i < numFish.Length; i++)
		{
			var n = numFish[i];
			var nextIndex = i - 1;
			if (nextIndex < 0)
			{
				nextIndex = 8;
				nextDayNumFish[6] += n;
			}
			
			nextDayNumFish[nextIndex] += n;
		}

		var temp = numFish;
		numFish = nextDayNumFish;
		nextDayNumFish = temp;

		if (daysPassed == 80)
		{
			numFish.Sum().Dump("better part1");
		}
		else if (daysPassed == 256)
		{
			numFish.Sum().Dump("better part2");
		}
	}
	
	return 0;
}


long BetterAnswer2(Dictionary<int, int> input, int days)
{
	var timerToNumFish = input.ToDictionary(e => e.Key, e => (long)e.Value);
	foreach (var day in Enumerable.Range(0, days))
	{
		timerToNumFish = timerToNumFish
			.SelectMany(kvp => kvp.Key == 0
				? new[] { (8, kvp.Value), (6, kvp.Value) }
				: new[] { (kvp.Key - 1, kvp.Value) })
			.GroupBy(e => e.Item1)
			.ToDictionary(e => e.Key, e => e.Sum(t => t.Value));
	}
	
	return timerToNumFish.Values.Sum();
}