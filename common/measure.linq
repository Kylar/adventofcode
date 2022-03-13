<Query Kind="Program" />

Measurement Measure(Action a)
{
	var sw = Stopwatch.StartNew();
	var gc = GC.GetAllocatedBytesForCurrentThread();
	a();
	var time = sw.Elapsed;
	var gc2 = GC.GetAllocatedBytesForCurrentThread();

	return new Measurement(time, gc2 - gc);
}

(TResult Result, Measurement) Measure<TResult>(Func<TResult> a)
{
	var sw = Stopwatch.StartNew();
	var gc = GC.GetAllocatedBytesForCurrentThread();
	var result = a();
	var time = sw.Elapsed;
	var gc2 = GC.GetAllocatedBytesForCurrentThread();

	return (result, new Measurement(time, gc2 - gc));
}

Measurement Measure<TResult>(Func<TResult> a, out TResult result)
{
	var sw = Stopwatch.StartNew();
	var gc = GC.GetAllocatedBytesForCurrentThread();
	result = a();
	var time = sw.Elapsed;
	var gc2 = GC.GetAllocatedBytesForCurrentThread();

	return new Measurement(time, gc2 - gc);
}

void Main()
{
	Measure((Action)(() => Enumerable.Range(0, 1000).Sum())).Dump("sum 0 to 999");
	Measure(() => AllocStuff(1)).Dump();

	var (result, _) = Measure(() => Enumerable.Range(0, 1000).Sum()).Dump("sum 0 to 999");
	Measure(() => Enumerable.Range(0, 1000).Sum(), out var result2).Dump("sum 0 to 999");
}

void AllocStuff(int n)
{
	for (var i = 0; i < n; i++)
	{
		var x = (object)3L;
	}
}

record Measurement(TimeSpan ExecutionTime, long TotalBytesAllocated);