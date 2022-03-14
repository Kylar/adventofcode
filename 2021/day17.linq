<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
    var inputPath = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "day17input.txt");
    var input = File.ReadAllText(inputPath);
    input.Dump();
    var m = Regex.Match(input.Trim(), @"^target area: x=(-?\d+)..(-?\d+), y=(-?\d+)..(-?\d+)$");
    var xLow = m.Groups[1].Value;
    var xHigh = m.Groups[2].Value;
    var yLow = m.Groups[3].Value;
    var yHigh = m.Groups[4].Value;

    var xRange = new Range(int.Parse(xLow), int.Parse(xHigh));
    var yRange = new Range(int.Parse(yLow), int.Parse(yHigh));
    
    (xRange, yRange).Dump();
    
    var hits = 0;
    
    for (var y = 500; y >= -160; y--)
    {
        for (var x = 0; x < 600; x++)
        {
            var hit = IsHit(0, 0, x, y, xRange, yRange, out var maxY);
            if (hit)
            {
                //hit.Dump();
                //x.Dump();
                //y.Dump();
                //maxY.Dump("part1");
                //return;
            }
            if (hit)
            {
                hits++;
            }
            //Console.Write(hit ? 'x' : '-');
        }
        //Console.WriteLine();
    }
    hits.Dump("part2");
    
    // Slow in part1 because:
    // 1. Didn't have script to DL input
    // 2. When I saw input, I wrote regex to parse it instead of just copy/pasting values into variables
    // 3. I created a range record which probably wasn't worth doing
}

record Range(int Low, int High)
{
    public bool IsHit(int x)
    {
        return x >= Low && x <= High;
    }
}

bool IsHit(int x, int y, int velX, int velY, Range targetX, Range targetY, out int maxY)
{
    maxY = y;
    while (true)
    {
        if (y > maxY) maxY = y;
        if (targetX.IsHit(x) && targetY.IsHit(y))
        {
            return true;
        }

        if (y < targetY.Low && velY < 0)
        {
            maxY = 0;
            return false;
        }
        
        x += velX;
        y += velY;
        if (velX < 0) velX++;
        if (velX > 0) velX--;
        velY--;
        
        
    }
}
