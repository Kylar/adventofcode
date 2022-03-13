<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 18);
	//input = @"[[[[3,5],1],1],1]".Replace("\r\n", "\n");
//	input = @"[[[[4,3],4],4],[7,[[8,4],9]]]
//[1,1]";

//	input = @"[[[0,[5,8]],[[1,7],[9,6]]],[[4,[1,2]],[[1,4],2]]]
//[[[5,[2,8]],4],[5,[[9,9],0]]]
//[6,[[[6,2],[5,6]],[[7,6],[4,7]]]]
//[[[6,[0,7]],[0,9]],[4,[9,[9,0]]]]
//[[[7,[6,4]],[3,[1,3]]],[[[5,5],1],9]]
//[[6,[[7,3],[3,2]]],[[[3,8],[5,7]],4]]
//[[[[5,4],[7,7]],8],[[8,3],8]]
//[[9,3],[[9,9],[6,[4,9]]]]
//[[2,[[7,7],7]],[[5,8],[[9,3],[0,2]]]]
//[[[[5,2],5],[8,[3,7]]],[[5,[7,5]],[4,4]]]";
	var textLines = input.Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);
	//textLines.Take(5).Dump();
	
	var parsedLines = textLines.Select(l => Parse(l));
	//parsedLines.Take(5).Dump();
	
	var depthLines = parsedLines.Select(l => ConvertToDepth(l)).ToArray();
	//depthLines.Take(5).Dump();

	var num = depthLines.First();
	Reduce(num);

	foreach (var n in depthLines.Skip(1))
	{
		Reduce(n);
		num = Add(num, n);
		Reduce(num);
	}
	
	string.Join(", ", num.Select(e => e.Num)).Dump("nums");
	string.Join(", ", num.Select(e => e.Depth)).Dump("depths");
	
	num.Dump();
	
	List<long> Magnitudes = new List<long>();
	for (var i = 0; i < depthLines.Count(); i++)
	{
		for (var j = 0; j < depthLines.Count(); j++)
		{
			if (i == j) continue;
			Reduce(depthLines[i]);
			Reduce(depthLines[j]);

			var sum = Add(depthLines[i], depthLines[j]);
			Reduce(sum);
			Magnitudes.Add(Magnitude(sum));
		}
	}
	
	Magnitude(num).Dump();
	
	Magnitudes.Max().Dump("part2");
}

object Parse(string num)
{
	var brackets = 0;
	var nums = new Stack<object>();
	
	for (var i = 0; i < num.Length; i++)
	{
		if (num[i] == '[') brackets++;
		if (num[i] == ']')
		{
			brackets--;
			var right = nums.Pop();
			var left = nums.Pop();
			nums.Push(new object[] { left, right });
		}
		if (num[i] >= '0' && num[i] <= '9')
		{
			nums.Push(num[i] - '0');
		}
	}
	return nums.Pop();
}

List<NumDepth> ConvertToDepth(object obj, int depth = 0)
{
	return obj switch
	{
		int n => new List<NumDepth> { new NumDepth(n, depth) },
		object[] arr => arr.SelectMany(item => ConvertToDepth(item, depth + 1)).ToList(),
	};
}

record NumDepth(long Num, int Depth)
{
	public NumDepth WithNum(long num)
	{
		return new NumDepth(Num + num, Depth);
	}

	public NumDepth WithDepth(int depth)
	{
		return new NumDepth(Num, depth);
	}
}

void Reduce(List<NumDepth> n)
{
	while (true)
	{
		if (TryExplode(n))
		{
			continue;
		}
		if (TrySplit(n))
		{
			continue;
		}
		return;
	}
}

bool TryExplode(List<NumDepth> n)
{
	var matches = 0;
	for (var i = 0; i < n.Count; i++)
	{
		if (n[i].Depth == 5)
		{
			matches++;
			if (matches == 2)
			{
				
				//n.Dump("before explode");
				var left = n[i-1].Num;
				var right = n[i].Num;
				if (i-2 >= 0)
				{
					n[i-2] = n[i-2].WithNum(left);
				}
				if (i+1 < n.Count)
				{
					n[i+1] = n[i+1].WithNum(right);
				}
				n[i - 1] = new NumDepth(0, n[i - 1].Depth - 1);
				n.RemoveAt(i);
				//n.Dump($"after explode {i - 1}");

				return true;
			}
		}
		else{
			matches = 0;
		}
	}
	
	return false;
}

bool TrySplit(List<NumDepth> n)
{
	for (var i = 0; i < n.Count; i++)
	{
		if (n[i].Num >= 10)
		{
			var left = n[i].Num / 2;
			var right = n[i].Num - left;
			//n.Dump("before");
			n.Insert(i + 1, new NumDepth(right, n[i].Depth + 1));
			n[i] = new NumDepth(left, n[i].Depth + 1);
			//n.Dump("after split");
			return true;
		}
	}
	return false;
}

List<NumDepth> Add(List<NumDepth> a, List<NumDepth> b)
{
	return a.Concat(b).Select(n => n.WithDepth(n.Depth + 1)).ToList();
}

long Magnitude(List<NumDepth> n)
{
	n = new List<NumDepth>(n);
	for (var i = 1; i < n.Count; i++)
	{
		//n.Dump($"magnitide loop {i}");
		if (n[i].Depth == n[i-1].Depth)
		{
			var left = n[i-1].Num * 3;
			var right = n[i].Num * 2;
			var newNum = new NumDepth(left + right, n[i-1].Depth - 1);
			n[i-1] = newNum;
			n.RemoveAt(i);
			i = 1 - 1;
			continue;
		}
	}
	
	return n.Single().Num;
}