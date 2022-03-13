<Query Kind="Program" />

#load "..\common\client"

async void Main()
{
	using var c = Configure2020();
	var input = await c.GetInput(1);
	var nums = input.Split("\n").Where(s => !string.IsNullOrWhiteSpace(s)).Select(int.Parse)
		.ToArray();
	Array.Sort(nums);
	
	//nums.Dump();

	//Part1(nums);
	Part2(nums);

}

async void Part1(int[] nums)
{
	foreach (var n1 in nums)
	{
		foreach (var n2 in nums)
		{
			if (n1 + n2 == 2020)
			{
				n1.Dump();
				n2.Dump();
				(n1 * n2).Dump("Answer");
				return;
			}
		}
	}
}

async void Part2(int[] nums)
{
	foreach (var n1 in nums)
	{
		foreach (var n2 in nums)
		{
			foreach (var n3 in nums)
			{
				if (n1 + n2 + n3 == 2020)
				{
					n1.Dump();
					n2.Dump();
					n3.Dump();
					(n1 * n2 * n3).Dump("Answer");
					return;
				}
			}
		}
	}
}

// You can define other methods, fields, classes and namespaces here