<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
    var inputPath = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "day15input.txt");
    var input = File.ReadAllText(inputPath);
    //input.Dump();
    
    var lines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);
    var risk = lines.Select(l => l.Select(c => c - '0').ToArray()).ToArray();
    //risk.Dump();
    
    var width = risk.First().Length;
    var height = risk.Length;

    var newRisk = new int[height * 5][];
    for (var y = 0; y < height * 5; y++)
    {
        newRisk[y] = new int[width * 5];
    }
    
    for (var repeatY = 0; repeatY < 5; repeatY++)
    {
        for (var repeatX = 0; repeatX < 5; repeatX++)
        {
            for (var y = 0; y < width; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var oldValue = risk[y][x];
                    var newValue = ((oldValue + repeatY + repeatX) - 1) % 9 + 1;
                    newRisk[y + repeatY * height][x + repeatX * width] = newValue;
                }
            }
        }
    }

    risk = newRisk;
    width = risk.First().Length;
    height = risk.Length;


    var toCheck = new Queue<Point>();
    toCheck.Enqueue(new Point(width - 2, height - 1));
    toCheck.Enqueue(new Point(width - 1, height - 2));
    
    var totalRisk = new int?[width, height];
    
    
   
    IEnumerable<Point> Neighbours(Point p)
    {
        if (p == new Point(0, 0))
        {
            return new Point[0];
        }
        
        return new[]
        {
            new Point(p.X, p.Y - 1),
            new Point(p.X, p.Y + 1),
            new Point(p.X + 1, p.Y),
            new Point(p.X - 1, p.Y),
        }.Where(p => p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height).ToArray();
    }
    
    totalRisk[width-1, height-1] = risk[height-1][width-1];
    
    
    
    //totalRisk.Dump();
    
    while (toCheck.Any())
    {
        var next = toCheck.Dequeue();
        if (next == new Point(width - 1, height - 1))
        {
            continue;
        }
        if (next == new Point(0, 0)) continue;
        
        var minNeighbour = Neighbours(next)
            .Select(n => totalRisk[n.X, n.Y])
            .Where(n => n != null)
            .Select(n => n.Value)
            .Min();
        
        var currentTotalRisk = totalRisk[next.X, next.Y];

        var newTotalRisk = risk[next.Y][next.X] + minNeighbour;
        if (currentTotalRisk == null || currentTotalRisk.Value > newTotalRisk)
        {
            totalRisk[next.X, next.Y] = newTotalRisk;
            foreach (var neighbour in Neighbours(next))
            {
                toCheck.Enqueue(neighbour);
            }
        }
    }

    // part1 589 take min of (1, 0) and (0, 1)
    // part2: 1799 wrong
    // part2: 2885 right, messed up modulus, did x % 9 istead of (x - 1) % 9 + 1

    var answer = new[]
    {
        totalRisk[0, 1],
        totalRisk[1, 0],
    }.Min().Dump();

    //totalRisk.Dump();
}

record Point(int X, int Y);

