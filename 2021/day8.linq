<Query Kind="Program" />

void Main()
{
    var inputText = @"be cfbegad cbdgef fgaecd cgeb fdcge agebfd fecdb fabcd edb | fdgacbe cefdb cefbgd gcbe
edbfga begcd cbg gc gcadebf fbgde acbgfd abcde gfcbed gfec | fcgedb cgb dgebacf gc
fgaebd cg bdaec gdafb agbcfd gdcbef bgcad gfac gcb cdgabef | cg cg fdcagb cbg
fbegcd cbd adcefb dageb afcb bc aefdc ecdab fgdeca fcdbega | efabcd cedba gadfec cb
aecbfdg fbg gf bafeg dbefa fcge gcbea fcaegb dgceab fcbdga | gecf egdcabf bgf bfgea
fgeab ca afcebg bdacfeg cfaedg gcfdb baec bfadeg bafgc acf | gebdcfa ecba ca fadegcb
dbcfg fgd bdegcaf fgec aegbdf ecdfab fbedc dacgb gdcebf gf | cefg dcbef fcge gbcadfe
bdfegc cbegaf gecbf dfcage bdacg ed bedf ced adcbefg gebcd | ed bcgafe cdgba cbgef
egadfb cdbfeg cegd fecab cgb gbdefca cg fgcdab egfdb bfceg | gbdfcae bgc cg cgb
gcafb gcf dcaebfg ecagb gf abcdeg gaef cafbge fdbac fegbdc | fgae cfgab fg bagce".Replace("\r\n", "\n");

    var inputPath = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "day8input.txt");
    inputText = File.ReadAllText(inputPath);

    var lines = inputText.Split("\n", StringSplitOptions.RemoveEmptyEntries).Select(ParseLine).ToArray();
    //lines.Dump();
    
    foreach (var line in lines)
    {
        foreach (var input in line.Inputs)
        {
            if (input.Contains(" "))
            {
                input.Dump("has space");
            }
        }
        foreach (var output in line.Outputs)
        {
            if (output.Contains(" "))
            {
                output.Dump("has space");
            }
        }
    }
    
    lines.Select(SolveClean).Sum().Dump("answer");
}

record Entry(string[] Inputs, string[] Outputs);

Entry ParseLine(string line)
{
    var parts = line.Split(" | ");
    var inputs = parts[0].Split(" ").Select(SortString).ToArray();
    var outputs = parts[1].Split(" ").Select(SortString).ToArray();
    return new Entry(inputs, outputs);
}

string SortString(IEnumerable<char> s) => new string(s.OrderBy(e => e).ToArray());

Dictionary<int, string> numToRealSegments = new()
{
    { 0, "abcefg" },
    { 1, "cf" },
    { 2, "acdeg" },
    { 3, "acdfg" },
    { 4, "bcdf" },
    { 5, "abdfg" },
    { 6, "abdefg" },
    { 7, "acf" },
    { 8, "abcdefg" },
    { 9, "abcdfg" },
};

int Solve(Entry entry)
{
    entry.Dump();

    // Mixed inputs of length N must repesent one of the numbers with N signals.
    var lengthGroups = entry.Inputs
        .GroupBy(e => e.Length)
        .Select(e => (Inputs: e.ToArray(), PossibleNums: numToRealSegments.Where(kvp => kvp.Value.Length == e.Key).Select(e => e.Key).ToArray()));

    lengthGroups.Dump("lengthToInputs");


    var mixedSegmentToPossibleRealSegments = new Dictionary<char, HashSet<char>>();
    foreach (var c in "abcdefg")
    {
        mixedSegmentToPossibleRealSegments.Add(c, "abcdefg".ToHashSet());
    }
    
    void MustBeOneOf(char c, IEnumerable<char> possibilities)
    {
        var before = mixedSegmentToPossibleRealSegments[c];
        var after = before.Intersect(possibilities).ToHashSet();
        mixedSegmentToPossibleRealSegments[c] = after;
        if (before.Count != after.Count)
        {
            var log = $"Char {c} must be one of {SortString(possibilities)}. Was {SortString(before)}, removed {SortString(before.Except(after))}, now {SortString(after)}";
            //log.Dump();
        }
    }
    
    void CannotBeOneOf(char c, IEnumerable<char> nonPossibilities)
    {
        var before = mixedSegmentToPossibleRealSegments[c];
        var after = before.Except(nonPossibilities).ToHashSet();
        mixedSegmentToPossibleRealSegments[c] = after;
        if (before.Count != after.Count)
        {
            var log = $"Char {c} cannot be one of {SortString(nonPossibilities)}. Was {SortString(before)}, removed {SortString(before.Except(after))}, now {SortString(after)}";
            //log.Dump();
        }
    }

    //foreach (var lengthGroup in lengthGroups)
    //{
    //    // When a group has exactly one input
    //    // 1. The input's mixed segments must be one of the real segments for the corresponding number.
    //    // 2. The input's mixed segments cannot be the real segments for segments not part of the corresponding number.
    //    if (lengthGroup.Inputs.Length == 1)
    //    {
    //        var input = lengthGroup.Inputs.Single();
    //        var possibleNum = lengthGroup.PossibleNums.Single();
    //        foreach (var c in input)
    //        {
    //            //MustBeOneOf(c, numToRealSegments[possibleNum]);
    //        }
    //        
    //        foreach (var c in "abcdefg".Except(input))
    //        {
    //            //CannotBeOneOf(c, numToRealSegments[possibleNum]);
    //        }
    //    }
    //}

    //mixedSegmentToPossibleRealSegments.Dump("mixedSegmentToPossibleRealSegments after groups of length 1 processing");
    // At this point we've determined d corresponds to a, and every other mixed segment is mapped to one of two possibilities.

    var groupsWithNumSegmentOccurences = lengthGroups
        .Select(g =>
        {
            // Analyse inputs to find how many inputs each segment appears in.
            // Do the same on the real segments for the possible numbers.
            // The set of mixed segments that appears N times, corresponds to the real segments that appear N times.

            var inputs = g.Inputs;
            var mixedSegmentToNumOccurences = inputs
                .SelectMany(e => e)
                .GroupBy(e => e)
                .ToDictionary(e => e.Key, e => e.Count());
            var realSegmentToNumOccurences = g.PossibleNums
                .Select(n => numToRealSegments[n])
                .SelectMany(e => e)
                .GroupBy(e => e)
                .ToDictionary(e => e.Key, e => e.Count());

            return (g.Inputs, g.PossibleNums, mixedSegmentToNumOccurences, realSegmentToNumOccurences);
        })
        .Dump("length groups with mixedSegmentToNumOccurences");
    
    foreach (var g in groupsWithNumSegmentOccurences)
    {
        foreach (var g2 in g.mixedSegmentToNumOccurences.GroupBy(e => e.Value))
        {
            var numOccurences = g2.Key;
            var mixedSegments = g2
                .Select(e => e.Key)
                .ToArray();
            var realSegments = g.realSegmentToNumOccurences
                .Where(e => e.Value == numOccurences)
                .Select(e => e.Key)
                .ToArray();

            (mixedSegments, realSegments).Dump("MixedSegments to RealSegments");

            foreach (var mixedSegment in mixedSegments)
            {
                MustBeOneOf(mixedSegment, realSegments);
            }
            
            foreach (var otherSegment in "abcdefg".Except(mixedSegments))
            {
                CannotBeOneOf(otherSegment, realSegments);
            }
        }
    }
    
    
    
    mixedSegmentToPossibleRealSegments.Dump("mixedSegmentToPossibleRealSegments");
    
    var possibleRealSegmentsToMixedSegments = mixedSegmentToPossibleRealSegments
        .Select(e => (e.Key, SortString(new String(e.Value.ToArray()))))
        .GroupBy(e => e.Item2)
        .ToDictionary(e => e.Key, e => e.Select(f => f.Key).ToArray());
    possibleRealSegmentsToMixedSegments.Dump("possibleRealSegmentsToMixedSegments");


    var mixedSegmentToRealSegment = mixedSegmentToPossibleRealSegments
        .ToDictionary(e => e.Key, e => e.Value.Single());

    var realSignalToNum = numToRealSegments
        .ToDictionary(e => SortString(e.Value), e => e.Key);

    var decodedOutputs = entry.Outputs
        .Select(output => SortString(output.Select(c => mixedSegmentToRealSegment[c])))
        .Select(e => realSignalToNum[e])
        .ToArray();

    return decodedOutputs[0] * 1000 + decodedOutputs[1] * 100 + decodedOutputs[2] * 10 + decodedOutputs[3];
}

// Same as solve but simplified.
int SolveClean(Entry entry)
{
    // For each segment, store the set of possible real segments that it could correspond to.
    // Anything is possible (for now) if you set your mind to it!
    var mixedSegmentToPossibleRealSegments = new Dictionary<char, HashSet<char>>();
    foreach (var c in "abcdefg")
    {
        mixedSegmentToPossibleRealSegments.Add(c, "abcdefg".ToHashSet());
    }

    void MustBeOneOf(char c, IEnumerable<char> possibilities)
    {
        mixedSegmentToPossibleRealSegments[c] = mixedSegmentToPossibleRealSegments[c].Intersect(possibilities).ToHashSet();
    }

    void CannotBeOneOf(char c, IEnumerable<char> nonPossibilities)
    {
        mixedSegmentToPossibleRealSegments[c] = mixedSegmentToPossibleRealSegments[c].Except(nonPossibilities).ToHashSet();;
    }

    var lengthGroups = entry.Inputs
        .GroupBy(e => e.Length);

    foreach (var g in lengthGroups)
    {
        var length = g.Key;
        var inputs = g.ToArray();

        // Inputs of length N must correspond to one of the numbers with N signals.
        var possibleNums = numToRealSegments
             .Where(kvp => kvp.Value.Length == length)
             .Select(e => e.Key)
             .ToArray();

        // Analyse segments in the inputs to find how many numbers they appear in.
        // Segments that appear in N input numbers, must correspond to segments that appear N times in the corresponding possible nums.
        var mixedSegmentToNumOccurences = inputs
            .SelectMany(e => e)
            .GroupBy(e => e)
            .ToDictionary(e => e.Key, e => e.Count());
        var realSegmentToNumOccurences = possibleNums
            .Select(n => numToRealSegments[n])
            .SelectMany(e => e)
            .GroupBy(e => e)
            .ToDictionary(e => e.Key, e => e.Count());

        foreach (var numOccurences in mixedSegmentToNumOccurences.Values.Distinct())
        {
            var mixedSegments = mixedSegmentToNumOccurences
                .Where(e => e.Value == numOccurences)
                .Select(e => e.Key)
                .ToArray();
            var realSegments = realSegmentToNumOccurences
                .Where(e => e.Value == numOccurences)
                .Select(e => e.Key)
                .ToArray();

            foreach (var mixedSegment in mixedSegments)
            {
                MustBeOneOf(mixedSegment, realSegments);
            }

            foreach (var otherSegment in "abcdefg".Except(mixedSegments))
            {
                CannotBeOneOf(otherSegment, realSegments);
            }
        }
    }
    
    var mixedSegmentToRealSegment = mixedSegmentToPossibleRealSegments
        .ToDictionary(e => e.Key, e => e.Value.Single());

    var realSegmentsToNum = numToRealSegments
        .ToDictionary(e => SortString(e.Value), e => e.Key);

    var decodedOutputs = entry.Outputs
        .Select(output => SortString(output.Select(c => mixedSegmentToRealSegment[c])))
        .Select(e => realSegmentsToNum[e])
        .ToArray();
        
    return decodedOutputs[0] * 1000 + decodedOutputs[1] * 100 + decodedOutputs[2] * 10 + decodedOutputs[3];
}
