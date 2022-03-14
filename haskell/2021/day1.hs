
input = readFile "/mnt/c/k/adventofcode/2021/day1input.txt"

-- Original solution
part1 = do
    text <- input
    let strLines = lines text
    let intLines = (map read strLines) :: [Integer]
    let pairs = zip intLines (tail intLines)
    let increasing = filter (\(x, y) -> y > x) pairs
    let answer = length increasing
    -- 1139
    print answer

-- Original solution
part2 = do
    text <- input
    let strLines = lines text
    let intLines = (map read strLines) :: [Integer]
    let triples = zip (zip intLines (tail intLines)) (tail (tail intLines))
    let sums = map (\((x, y), z) -> x + y + z) triples
    let sumPairs = zip sums (tail sums)
    let increasing = filter (uncurry (<)) sumPairs
    let answer = length increasing
    -- 1103
    print answer

readInt :: [Char] -> Integer
readInt = read

-- Rewritten solution
part1v2 = do
    text <- input
    let intLines = map readInt $ lines text
    let pairs = zip intLines $ tail intLines
    let answer = length $ filter (uncurry (<)) pairs
    -- 1139
    print answer

-- Rewritten solution
part2v2 = do
    text <- input
    let intLines = map readInt $ lines text
    --let triples = zip (zip intLines $ tail intLines) (tail $ tail intLines)
    --let sums = map (uncurry ((+) . uncurry (+))) triples
    let triples = zip3 intLines (tail intLines) (tail $ tail intLines)
    let sums = map (\(a,b,c) -> a+b+c) triples
    let sumPairs = zip sums (tail sums)
    let increasing = filter (uncurry (<)) sumPairs
    let answer = length increasing
    -- 1103
    print ("part2 answer", answer)

