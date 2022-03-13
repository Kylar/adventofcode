<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 2);

	var textLines = input.Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);
	textLines.Dump();
}
