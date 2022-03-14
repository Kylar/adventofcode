<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
    var inputPath = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "day20input.txt");
    var input = File.ReadAllText(inputPath);
//    input.Dump();
    //input = TestInput();
    
    var lines = input.Replace("\r\n", "\n").Split("\n");
    var algo = lines[0];
    
    var pixels = lines
        .Skip(2)
        .SelectMany((line, i) => line.Select((c, j) => (i, j, c)))
        .ToDictionary(e => (e.i, e.j), e => e.c);

   
    
    foreach (var iter in Enumerable.Range(0, 50))
    {
        var minX = pixels.Keys.Select(k => k.i).Min() - 2;
        var maxX = pixels.Keys.Select(k => k.i).Max() + 2;
        var minY = pixels.Keys.Select(k => k.j).Min() - 2;
        var maxY = pixels.Keys.Select(k => k.j).Max() + 2;

        Dictionary<(int, int), char> newDict = new Dictionary<(int, int), char>();

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                var inputPixels = from dx in Enumerable.Range(-1, 3)
                                  from dy in Enumerable.Range(-1, 3)
                                  select GetPixel(pixels, x + dx, y + dy, iter);

                var bits = inputPixels.Select(p => p == '#' ? 1 : 0)
                    .ToArray();
                int n = 0;
                for (var bitI = 0; bitI < bits.Length; bitI++)
                {
                    n = n * 2 + bits[bitI];
                }
                
                var newPixel = algo[n];
                newDict.Add((x, y), newPixel);
            }
        }
        pixels = newDict;
    }

    //5319 p1 wrong
    // 5464 p1 wrong
    // 5291 p1 right (# as default)
    // 44160 p2 wrong
    // 16665 p2 right (forgot to use % 2)
    pixels.Values.Where(v => v == '#').Count().Dump();
}

void DumpImage(Dictionary<(int, int), char> dict)
{
}

char GetPixel(Dictionary<(int, int), char> dict, int x, int y, int i)
{
    if (dict.TryGetValue((x, y), out var existing))
    {
        return existing;
    }
    else
    {
        return i == 0 ? '.' : '#';
    }
}


string TestInput()
{

    return @"..#.#..#####.#.#.#.###.##.....###.##.#..###.####..#####..#....#..#..##..###..######.###...####..#..#####..##..#.#####...##.#.#..#.##..#.#......#.###.######.###.####...#.##.##..#..#..#####.....#.#....###..#.##......#.....#..#..#..##..#...##.######.####.####.#.#...#.......#..#.#.#...####.##.#......#..#...##.#.##..#...##.#.##..###.#......#.#.......#.#.#.####.###.##...#.....####.#..#..#.##.#....##..#.####....##...##..#...#......#.#.......#.......##..####..#...#.#.#...##..#.#..###..#####........#..####......#..#

#..#.
#....
##..#
..#..
..###";
}