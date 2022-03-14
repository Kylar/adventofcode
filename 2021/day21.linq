<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
    // The input only contained these 2 ints, so I didn't bother parsing the input file.
    var a = 7;
    var b = 10;

    var turn = 0;

    var scores = new int[2] { 0, 0 };
    var pos = new int[2] { 7 - 1, 10 - 1 };
    var dieSize = 100;

    //pos = new[] { 4 - 1, 8 - 1 };
    //dieSize = 10;


    var r = 0;


    int numRolls = 0;
    int Roll()
    {
        numRolls++;
        r = (r % dieSize) + 1;
        return r;
        //.Dump();;
    }


    //while (true)
    //{
    //    //new { scores, pos = pos.Select(p => p + 1).ToArray(), numRolls }.Dump();
    //    
    //    //scores[turn].Dump();
    //    
    //    pos[turn] = (pos[turn] + Roll() + Roll() + Roll()) % 10;
    //    scores[turn] += pos[turn] + 1;
    //    if (scores[turn] >= 1000)
    //    {
    //        new
    //        {
    //            turn,
    //            scores,
    //            pos,
    //            numRolls,
    //        }.Dump();
    //        
    //        (scores[(turn + 1) % 2] * numRolls).Dump("p1");
    //        break;
    //    }
    //    turn = (turn + 1) % 2;
    //}

    var nums = new[] { 1, 2, 3 };

    var rollOutcomes =
        from a1 in nums
        from b1 in nums
        from c in nums
        select a1 + b1 + c;
        
    var rollHist = rollOutcomes.GroupBy(e => e)
        .ToDictionary(g => g.Key, g => g.Count());
    
    rollHist.Dump();




    (long P1Wins, long P2Wins) Part2(Dictionary<(int, int, int, int, int), (long, long)> d, int p1Score, int p2Score, int p1Pos, int p2Pos, int turn)
    {
        if (d.TryGetValue((p1Score, p2Score, p1Pos, p2Pos, turn), out var existing))
        {
            return existing;
        }

        var p1Wins = 0L;
        var p2Wins = 0L;

        foreach (var roll in rollHist)
        {
            if (turn == 0)
            {
                var newP1Pos = (p1Pos + roll.Key) % 10;
                var newP1Score = p1Score + newP1Pos + 1;
                if (newP1Score >= 21)
                {
                    p1Wins += roll.Value;
                }
                else
                {
                    var (p1RecWins, p2RecWins) = Part2(d, newP1Score, p2Score, newP1Pos, p2Pos, turn == 0 ? 1 : 0);
                    p1Wins += p1RecWins * roll.Value;
                    p2Wins += p2RecWins * roll.Value;
                }
            }
            else
            {
                var newP2Pos = (p2Pos + roll.Key) % 10;
                var newP2Score = p2Score + newP2Pos + 1;
                if (newP2Score >= 21)
                {
                    p2Wins += roll.Value;
                }
                else
                {
                    var (p1RecWins, p2RecWins) = Part2(d, p1Score, newP2Score, p1Pos, newP2Pos, turn == 0 ? 1 : 0);
                    p1Wins += p1RecWins * roll.Value;
                    p2Wins += p2RecWins * roll.Value;
                }
            }
        }

        var answer = (p1Wins, p2Wins);
        d.Add((p1Score, p2Score, p1Pos, p2Pos, turn), answer);
        return answer;
    }

    Part2(new Dictionary<(int, int, int, int, int), (long, long)>(), 0, 0, pos[0], pos[1], 0).Dump("p2");
    //Part2(new Dictionary<(int, int), (long, long)>(), 0, 0, 4 - 1, 8 - 1, 0).Dump("p2 sample");
    //Part2(new Dictionary<(int, int), (long, long)>(), 0, 0, 4, 8, 0).Dump("p2 sample");
}

