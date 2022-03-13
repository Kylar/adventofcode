<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load "..\common\client"

async Task Main()
{
	using var c = Configure2021();
	var input = await c.GetInput(dayNumber: 24);
	//input.Dump();
	
	//AnalyzeInput(input);

	//Test1();
	//Test2();
	//Test3();
	Part1(input);

}

void AnalyzeInput(string input)
{
	var lines = input.Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);
	for (var i = 0; i < 14; i++)
	{
		for (var j = 0; j < 18; j++)
		{
			Console.Write(lines[i * 18 + j].PadRight(9));
		}
		Console.WriteLine();
	}
}

void Test1()
{
	var m = ExecuteInput(@"inp x
mul x -1", "3").Dump();
}

void Test2()
{
	var m = ExecuteInput(@"inp z
inp x
mul z 3
eql z x", "26").Dump();
}

void Test3()
{
	var m = ExecuteInput(@"inp w
add z w
mod z 2
div w 2
add y w
mod y 2
div w 2
add x w
mod x 2
div w 2
mod w 2", "7");	
	m.Dump();
}

void Part1(string codeInput)
{
	var textLines = codeInput.Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);
	var instrs = textLines.Select(ParseLine)
		.ToArray();
	//instrs.Dump();

	long ComputeWithMachine(string input, int steps = 14, long initialZ = 0)
	{
		var m = new Machine(input);
		m._z = initialZ;
		for (var i = 0; i < steps; i++)
		{
			for (var j = 0; j < 18; j++)
			{
				var instr = instrs[i * 18 + j];
				m.Execute(instr);
			}
		}
		
		return m._z;
	}
	
	var codeLiterals = Enumerable.Range(0, 14)
		.Select(i =>
		{
			var divZ = ((instrs[4 + i * 18] as BinaryOpInstruction).B as LiteralValue).Value;
			var addToX = ((instrs[5 + i * 18] as BinaryOpInstruction).B as LiteralValue).Value;
			var addToW = ((instrs[15 + i * 18] as BinaryOpInstruction).B as LiteralValue).Value;
			return (DivZ: divZ, AddToX: addToX, AddToW: addToW);
		})
		.ToArray();
	// DivZ either 1 or 26,
	// AddToW from 1 to 15
	// AddToX from -13 to -1, or from 11 to 14
	codeLiterals.Dump("codeLiterals (divZ, addToW, addToX)");

	long ComputeEfficient(string input, int steps = 14, long initialZ = 0)
	{
		var z = initialZ;
		for (var i = 0; i < steps; i++)
		{
			var (divZ, addToX, addToW) = codeLiterals[i];
			var digit = input[i] - '0';
			z = ComputeZ(z, digit, codeLiterals[i]);
		}
		return z;
	}

	var input = "11419939669999";
	input = "13579246899999";
		
	ComputeWithMachine(input).Dump("result via machine");
	ComputeEfficient(input).Dump("result via C# expressions");

	ComputeWithMachine("11419939669999", initialZ: -140).Dump("result via machine");
	ComputeEfficient("11419939669999", initialZ: -140).Dump("result via C# expressions");

	// Answer 99999999999999 wrong (addToX == 9 ? 8 : 9) for each digit
	//new string(part1Digits.Select(d => (char)(d - 0 + '0')).ToArray()).Dump("part1");
	
	var oneToNine = Enumerable.Range(1, 9);
	
	((int Digit, long Z) Previous, (int Digit, long Z) Result)[] ComputePass((int Digit, long Z)[] previous, (int DivZ, int AddToW, int AddToX) literals)
	{
		return previous
			.SelectMany(previous => oneToNine.Select(digit =>
			{
				return (Previous: previous, Result: (Digit: previous.Digit * 10 + digit, Z: ComputeZ(previous.Z, digit, literals)));
			}))
			.ToArray();
	}

	//var pass = new[] { (Digit: 0, Z: 0L) };
	//pass = Enumerable.Range(0, 2000)
	//	.Select(i => {
	//		return (Digit: 0, Z: i - 1000L);
	//	})
	//	.ToArray();
	//
	//for (var i = 13; i < 14; i++)
	//{
	//	pass = ComputePass(pass, codeLiterals[i]).Select(p => p.Result).ToArray();
	//}
	
	var requiredInputForZero = new (int Digit, long Z)[14][];
	
	for (var i = 14 - 1; i >= 0; i--)
	{
		var lotsOfInputs = Enumerable.Range(0, 20000)
			.SelectMany(i => oneToNine.Select(digit =>
			{
				return (Digit: digit, Z: i - 10000L);
			}))
			.ToArray();
		if (i == 0)
		{
			lotsOfInputs = lotsOfInputs
				.Where(e => e.Z == 0)
				.ToArray();
		}
			
		var currentToNextPossibilities = lotsOfInputs
			.Select(input => (input, z: ComputeZ(input.Z, input.Digit, codeLiterals[i])))
			.ToArray();

		var validNextZ = (i == 13 ? new[] { 0L } : requiredInputForZero[i+1].Select(e => e.Z)).ToHashSet();
		
		requiredInputForZero[i] = currentToNextPossibilities
			.Where(e => validNextZ.Contains(e.z))
			.Select(e => e.input)
			.ToArray();
	}
	
	string ComputeAnswer(long initialZ = 0)
	{
		var z = initialZ;
		var digits = new List<int>();
		for (var i = 0; i < 14; i++)
		{
			var digit = requiredInputForZero[i]
				.Where(e => e.Z == z)
				.OrderByDescending(e => e.Digit)
				.First()
				.Digit;
			z = ComputeZ(z, digit, codeLiterals[i]);
			digits.Add(digit);
		}
		return new string(digits.Select(i => (char)(i + '0')).ToArray());
	}
	//ComputeAnswer(initialZ: -140).Dump("answer when z starts at -140");

	//requiredInputForZero.Dump("requiredInputForZero");

	//ComputeZ(0L, 2, (1, 11, 1)).Dump("help");

	//pass.Dump("pass");
	//pass
	//	.OrderBy(e => e.Digit)
	//	.Select((e, i) => (Index: i, Digit: e.Digit, Z: e.Z))
	//	.Chart(xFunc: x => x.Index, yFunc: x => x.Z, LINQPad.Util.SeriesType.Point)
	//	.Dump($"pass");

	//pass.Where(p => p.Z == 0).Dump("final pass with result 0");

	//ComputeZInputs(digit: 1, codeLiterals[13].DivZ == 26, codeLiterals[13].AddToX, codeLiterals[13].AddToW, outputZ: 0L).Dump("output Z")
	//	.Select(z => ComputeZ(z, 1, codeLiterals[13]))
	//	.Dump("recomputed Z");





	(int Digit, long Z)[] PartialSolveFromStart(int numDigits)
	{
		var possibleOutputs = new[] { (Digit: 0, Z: 0L) };
		
		for (var i = 0; i < numDigits; i++)
		{
			var literals = codeLiterals[i];
			possibleOutputs = oneToNine
				.SelectMany(digit =>
				{
					return possibleOutputs.Select(previousOutput =>
					{
						var z = ComputeZ(previousOutput.Z, digit, literals);
						return (Digit: previousOutput.Digit * 10 + digit, Z: z);
					});
				})
				.ToArray();
		}
		
		//possibleOutputs.Dump();
		//possibleOutputs.Min(e => e.Z).Dump("minz");
		//possibleOutputs.Max(e => e.Z).Dump("maxz");
		return possibleOutputs;
	}

	int numStartDigits = 7;
	var startPossibilities = PartialSolveFromStart(numStartDigits);

	void ValidateRequiredInput((int Digit, long Z)[][] arr)
	{
		
	}

	void SolvePart1Attempt2((int Digit, long Z)[] startPossibilities, int numStartDigits)
	{
		var requiredInputForZero = new (int Digit, long Z)[14][];

		for (var i = 14 - 1; i >= numStartDigits; i--)
		{
			i.Dump();
			var validNextZ = (i == 13 ? new[] { 0L } : requiredInputForZero[i + 1].Select(e => e.Z)).ToHashSet();

			var inputDigitAndZ = oneToNine
				.SelectMany(digit => validNextZ.Select(targetZ => (Digit: digit, TargetZ: targetZ)))
				.SelectMany(e =>
				{
					var inputs = ComputeZInputs(digit: e.Digit, codeLiterals[i].DivZ == 26, codeLiterals[i].AddToX, codeLiterals[i].AddToW, outputZ: e.TargetZ).Select(z => (Digit: e.Digit, Z: z));
					//foreach (var input in inputs)
					//{
					//	if (ComputeZ(input.Z, input.Digit, codeLiterals[i].DivZ, codeLiterals[i].AddToX, codeLiterals[i].AddToW) != e.TargetZ)
					//	{
					//		codeLiterals[i].Dump($"codeLiterals[{i}]");
					//		(digit: e.Digit, codeLiterals[i].DivZ == 26, codeLiterals[i].AddToX, codeLiterals[i].AddToW, outputZ: e.TargetZ).Dump("ComputeZInputs args");
					//		e.Dump("e");
					//		input.Dump("input");
					//		ComputeZ(input.Z, input.Digit, codeLiterals[i].DivZ, codeLiterals[i].AddToX, codeLiterals[i].AddToW).Dump("ComputeZ(input.Z, ...)");
					//		inputs.Dump("inputs");
					//		//                                                    9        1            26                    -8                      6,                      -7,                                                target: 0
					//		throw new Exception($"input for output is incorrect {(input.Z, input.Digit, codeLiterals[i].DivZ, codeLiterals[i].AddToX, codeLiterals[i].AddToW, ComputeZ(input.Z, input.Digit, codeLiterals[i].DivZ, codeLiterals[i].AddToX, codeLiterals[i].AddToW), e.TargetZ)}");
					//	}
					//}
					return inputs;
				})
				.Distinct()
				.ToArray();
			
			//.Select(z => ComputeZ(z, 1, codeLiterals[13])).Dump("recomputed Z");

			if (i == 0)
			{
				inputDigitAndZ = inputDigitAndZ
					.Where(e => e.Z == 0)
					.ToArray();
			}

			requiredInputForZero[i] = inputDigitAndZ;
			//requiredInputForZero[i].Count().Dump("input count");
			//requiredInputForZero[i].Dump();
		}
		
	
		var validMiddleZs = requiredInputForZero[numStartDigits].Select(e => e.Z).ToHashSet();

		var (startDigits, z) = startPossibilities.Where(e => validMiddleZs.Contains(e.Z))
			//.OrderByDescending(e => e.Digit)
			.OrderBy(e => e.Digit)
			//.Dump("multiple?")
			.First();

		//requiredInputForZero[numStartDigits]
		//	.Where(e => validZInputs.Contains(e.Z))
		//	.Dump("answer kinda")
		//	.Count()
		//	.Dump("num answers kinda");
		
		startDigits.Dump("startDigits");
		//z.Dump("z");
					
		for (var i = numStartDigits; i < 14; i++)
		{
			var nextChoice = requiredInputForZero[i]
				.Where(e => e.Z == z)
				//.OrderByDescending(e => e.Digit)
				.OrderBy(e => e.Digit)
				.First();
			//nextChoice.Dump("next choice");
			nextChoice.Digit.Dump("next digit");
			z = nextChoice.Z;
			z = ComputeZ(z, nextChoice.Digit, codeLiterals[i]);
		}
		
	}
//
//	var digit = 1;
//	var targetZ = 6;
//	var inputs = ComputeZInputs(digit, codeLiterals[10].DivZ == 26, codeLiterals[10].AddToX, codeLiterals[10].AddToW, targetZ).Dump("inputs");
//	inputs.Select(inputZ =>
//	{
//		var recomputedZ = ComputeZ(inputZ, digit, codeLiterals[10].DivZ, codeLiterals[10].AddToX, codeLiterals[10].AddToW);
//		var pass = recomputedZ == targetZ;
//		return new { recomputedZ, pass };
//	}).Dump("input debugging");
	//ComputeZ(243, 1, (26, -8, 6)).Dump();


	SolvePart1Attempt2(startPossibilities, numStartDigits);

	//ComputeWithMachine("9999969", steps: 7).Dump();

	//InputsForAdd(10, 3).Select(e => e + 3).Dump();
	//InputsForMul(10, 2).Select(e => e * 2).Dump();
	//InputsForDiv26(15).Select(e => e / 26).Dump();
	//InputsForConstantOutput(45).Dump();

	// 92969593497992 part1 correct
	ComputeWithMachine("92969593497992").Dump("part1 answer");
	ComputeWithMachine("81514171161381").Dump("part2 answer");
}

long ComputeZ(long z, int digit, (int divZ, int addToX, int addToW) literals)
{
	var x = (z % 26 + literals.addToX) != digit ? 1 : 0;
	return z / literals.divZ * ((25 * x) + 1) + (digit + literals.addToW) * x;
}

long ComputeZ(long z, int digit, int divZ, int addToX, int addToW)
{
	var x = (z % 26 + addToX) != digit ? 1 : 0;
	return z / divZ * ((25 * x) + 1) + (digit + addToW) * x;
}

long ComputeZV2(long z, int digit, int divZ, int addToX, int addToW)
{
	if ((z % 26 + addToX) != digit)
	{
		return z / divZ * 26 + (digit + addToW);
	}
	else
	{
		return z / divZ;
	}
}

long ComputeZV3(long z, int digit, bool isDivZ26, int addToX, int addToW)
{
	if ((z % 26 + addToX) != digit)
	{
		if (isDivZ26)
		{
			return (z / 26) * 26 + (digit + addToW);
			// return Floor(z / 26) * 26 + (digit + addToW);
		}
		else
		{
			return z * 26 + (digit + addToW);
		}
	}
	else
	{
		if (isDivZ26)
		{
			return z / 26;
			// return Floor(z / 26)
		}
		else
		{
			return z;
		}
	}
}

IEnumerable<long> ComputeZInputs(int digit, bool isDivZ26, int addToX, int addToW, long outputZ)
{
	if (isDivZ26)
	{
		var mainBranch = InputsForAdd(outputZ, digit + addToW)
			.SelectMany(e => InputsForMul(e, 26))
			.SelectMany(e => InputsForDiv26(e));

		var elseBranch = InputsForDiv26(outputZ);
		
		return mainBranch 
			.Where(e => e % 26 != digit - addToX)
			.Concat(elseBranch
				.Where(e => e % 26 == digit - addToX));
	}
	else
	{
		var mainBranch = InputsForAdd(outputZ, digit + addToW)
			.SelectMany(e => InputsForMul(e, 26));

		var elseBranch = InputsForConstantOutput(outputZ);

		return mainBranch
			.Where(e => e % 26 != digit - addToX)
			.Concat(elseBranch
				.Where(e => e % 26 == digit - addToX));
	}
}
//
//IEnumerable<long> InputsForMod26Output(long output)
//{
//}

IEnumerable<long> InputsForConstantOutput(long output)
{
	return new[] { output };
}

IEnumerable<long> InputsForAdd(long output, long b)
{
	return new[] { output - b };
}

IEnumerable<long> InputsForMul(long output, long b)
{
	if (output % b != 0) yield break;
	yield return output / b;
}

IEnumerable<long> InputsForDiv26(long output)
{
	return Enumerable.Range(0, 26)
		.Select(i => output * 26 + i);
}

record Range(long Min, long Max);


string ReadableInstr(Instruction instr)
{
	return instr switch
	{
		InputInstruction i => $"{i.Register.Name} = Read()",
		BinaryOpInstruction op => op.Op switch
		{
			BinaryOp.Add => $"{op.A.Name} += {op.B.ToString()}",
		}
	};
}

int DebugProgram(string codeInput, string machineInput)
{
	var textLines = codeInput.Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);
	var instrs = textLines.Select(ParseLine)
		.ToArray();
	var m = new Machine(machineInput);
	m.Dump();
	foreach (var instr in instrs)
	{
		m.Execute(instr);
		new { Instruction = instr, Machine = m }.Dump();
	}
	return 0;
}

Machine ExecuteInput(string codeInput, string machineInput)
{
	var textLines = codeInput.Replace("\r\n", "\n").Split("\n", StringSplitOptions.RemoveEmptyEntries);
	var instrs = textLines.Select(ParseLine)
		.ToArray();
	var m = new Machine(machineInput);
	foreach (var instr in instrs)
	{
		m.Execute(instr);
	}
	return m;
}

class Machine
{
	private int _inputPos = 0;
	private string _input;

	public long _w;
	public long _x;
	public long _y;
	public long _z;
	
	public Machine(string input)
	{
		_input = input;
	}
	
	public void Execute(Instruction instruction)
	{
	
		switch (instruction)
		{
			case InputInstruction i: Store(i.Register, _input[_inputPos++] - '0'); break;
			case BinaryOpInstruction op:
				switch (op.Op)
				{
					case BinaryOp.Add: Store(op.A, Load(op.A) + Load(op.B)); break;
					case BinaryOp.Mul: Store(op.A, Load(op.A) * Load(op.B)); break;
					case BinaryOp.Div: Store(op.A, Load(op.A) / Load(op.B)); break;
					case BinaryOp.Mod: Store(op.A, Load(op.A) % Load(op.B)); break;
					case BinaryOp.Eql: Store(op.A, Load(op.A) == Load(op.B) ? 1 : 0); break;
				}
				break;
		};
	}

	private long Load(ValueRef valueRef)
	{
		return valueRef switch
		{
			Register r => Load(r),
			LiteralValue v => v.Value,
			_ => throw new Exception("oops"),
		};
	}

	private long Load(Register r)
	{
		return r.Name switch
		{
			'x' => _x,
			'y' => _y,
			'z' => _z,
			'w' => _w,
			_ => throw new Exception("oops"),
		};
	}
	
	public void Store(Register r, long v)
	{
		_ = r.Name switch
		{
			'x' => _x = v,
			'y' => _y = v,
			'z' => _z = v,
			'w' => _w = v,
			_ => throw new Exception("oops"),
		};
	}
}

Instruction ParseLine(string line)
{
	var parts = line.Split();
	return parts[0] switch
	{
		"inp" => new InputInstruction(ParseRegister(parts[1])),
		"add" => new BinaryOpInstruction(BinaryOp.Add, ParseRegister(parts[1]), ParseRef(parts[2])),
		"mul" => new BinaryOpInstruction(BinaryOp.Mul, ParseRegister(parts[1]), ParseRef(parts[2])),
		"div" => new BinaryOpInstruction(BinaryOp.Div, ParseRegister(parts[1]), ParseRef(parts[2])),
		"mod" => new BinaryOpInstruction(BinaryOp.Mod, ParseRegister(parts[1]), ParseRef(parts[2])),
		"eql" => new BinaryOpInstruction(BinaryOp.Eql, ParseRegister(parts[1]), ParseRef(parts[2])),
		_ => throw new Exception("oops")
	};
}

ValueRef ParseRef(string part)
{
	if (int.TryParse(part, out var integer))
	{
		return new LiteralValue(integer);
	}
	return ParseRegister(part);
}

Register ParseRegister(string part)
{
	return new Register(part.Single());
}

record Instruction
{
}

record InputInstruction(Register Register) : Instruction
{
}

enum BinaryOp
{
	Add,
	Mul,
	Div,
	Mod,
	Eql,
}

record BinaryOpInstruction(BinaryOp Op, Register A, ValueRef B) : Instruction;

record ValueRef
{
	
}

record LiteralValue(int Value) : ValueRef;

record Register(char Name) : ValueRef;
