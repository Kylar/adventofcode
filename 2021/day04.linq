<Query Kind="Program" />

#load "..\common\client"

class Blah
{
	public int[][] Board {get;set;}
	public bool[][] Marks {get;set;}
	public bool Won { get;set;}
}

async void Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 4);
	//input = testInput();

	var textLines = input.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
	var nums = textLines[0].Split(",");//.Dump();

	var boardTexts = textLines.Skip(1).ToList();

	//boardTexts.Dump();
	var n = 0;

	var boards = boardTexts
		.Select(b =>
		{
			var rows = b.Split("\n", StringSplitOptions.RemoveEmptyEntries);
			var cols = rows
				.Select(r => r.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)
				.ToArray()).ToArray();
			return cols;
		});

	var marks = boards.Select(b =>
	{
		var blah = from x in Enumerable.Range(0, 5)
				   select Enumerable.Range(0, 5).Select(e => false).ToArray();
		return blah.ToArray();
	})
	.ToArray();

	var boardAndMarks = boards.Zip(marks, (a, b) => new Blah { Board = a, Marks = b })
		.ToArray();

	//boardAndMarks.Dump("board and marks");

	//nums.Dump();

	
	int winners = 0;
	Blah winner = null;
	int lastNum = 0;
	int winnerScore = 0;
	foreach (var num in nums.Select(n => int.Parse(n)))
	{
		foreach (var boardAndMark in boardAndMarks)
		{
			for (var x = 0; x < 5; x++)
			{
				for (var y = 0; y < 5; y++)
				{
					//boardAndMark.Board[y][x].Dump();
					//num.Dump();
					//new { num, boardAndMark, x, y }.Dump();
					if (boardAndMark.Board[y][x] == num)
					{
						//"marking".Dump();
						//boardAndMark.Marks.Dump("before");
						boardAndMark.Marks[y][x] = true;
						//boardAndMark.Marks.Dump("after");
						if (IsWinning(boardAndMark.Marks) && !boardAndMark.Won)
						{
							winner = boardAndMark;
							lastNum = num;
							boardAndMark.Won = true;
							var sum = 0;
							foreach (var x2 in Enumerable.Range(0, 5))
							{
								foreach (var y2 in Enumerable.Range(0, 5))
								{
									if (!winner.Marks[y2][x2])
									{
										sum += winner.Board[y2][x2];
									}
								}

							}
							winnerScore = sum;
						}
					}
				}
			}
		}

	}
after:
//
//	var sum = 0;
//	foreach (var x in Enumerable.Range(0, 5))
//	{
//		foreach (var y in Enumerable.Range(0, 5))
//		{
//			if (!winner.Marks[y][x])
//			{
//				sum += winner.Board[y][x];
//			}
//		}
//
//	}

	new { winner, lastNum, winnerScore }.Dump();
	
	// 4512 wrong part 1
	
	// 8496 wrong part 2
	var answer = winnerScore * lastNum;
	answer.Dump("answer");
	
	
	
	//boardAndMarks.Dump();
		
	//boards.Dump();
	
	
	
	// Part 1
	
	
	
	// Part 2

	// Fuckups:
	// - Didn't ToArray() the zip, so each num was modifying a new instance of boards...
	// 
	
}


bool IsWinning(bool[][] board)
{
	var rows = Enumerable.Range(0, 5).Select(y => Enumerable.Range(0, 5).Select(x => (X: x, Y: y)));
	var cols = Enumerable.Range(0, 5).Select(y => Enumerable.Range(0, 5).Select(x => (X: y, Y: x)));
	
	foreach (var line in rows.Concat(cols))
	{
		if (line.All(e => board[e.Y][e.X]))
		{
			return true;
		}
	}
	return false;
	
}

string testInput()
{

	return @"7,4,9,5,11,17,23,2,0,14,21,24,10,16,13,6,15,25,12,22,18,20,8,19,3,26,1

22 13 17 11  0
 8  2 23  4 24
21  9 14 16  7
 6 10  3 18  5
 1 12 20 15 19

 3 15  0  2 22
 9 18 13 17  5
19  8  7 25 23
20 11 10 24  4
14 21 16 12  6

14 21 17 24  4
10 16 15  9 19
18  8 23 26 20
22 11 13  6  5
 2  0 12  3  7".Replace("\r\n", "\n");
}