input = readFile "/mnt/c/k/adventofcode/2021/day3input.txt"

part1 = do
    txt <- input
    --ls <- map (\l -> map (\c -> if c == '1' then 1 else 0)) $ lines input
    let ls = lines txt
    let n = length (head ls)
    let bitIndices = [0..n-1]
    let bitSlices = map (takeBitSlice ls) bitIndices
    let mostCommonBits = map mostCommonBit bitSlices
    let leastCommonBits = map invert mostCommonBits
    let answer = binToDec mostCommonBits * (binToDec leastCommonBits)
    print answer

mostCommonBit = mostCommonBitWithTiebreak '0'

mostCommonBitWithTiebreak tiebreak bits =
    let (c0, c1) = foldl (\(a, b) v -> case v of
            '1' -> (a, b + 1)
            _ -> (a + 1, b)) (0, 0) bits
    in
        if c0 == c1 then tiebreak
        else if c0 > c1 then '0'
        else '1'

takeBitSlice lines index = map (!! index) lines

invert bit = if bit == '0' then '1' else '0'

binToDec bits =
    let (res, _) = foldr (\v acc@(r, n) -> case v of
            '1' -> (r + n, n * 2)
            '0' -> (r, n * 2)) (0, 1) bits
    in res

part2 = do
    txt <- input
    let ls = lines txt
    let n = length (head ls)
    let gen = search filterGen ls 0
    let scrub = search filterScrub ls 0
    let answer = foldr (*) 1 (map binToDec [gen, scrub])
    print gen
    print scrub
    print ("answer", answer)


search f ls index =
    let nextLs = f ls index
        answer = case nextLs of
            [] -> last ls
            l:[] -> l
            _ -> search f nextLs (index + 1)
    in if length ls == 1 then head ls else answer

filterGen ls index =
    let bitSlice = takeBitSlice ls index
        mostCommonBitV = mostCommonBitWithTiebreak '1' bitSlice
        nextLs = filter (\l -> (l !! index) == mostCommonBitV) ls
    in nextLs

filterScrub ls index =
    let bitSlice = takeBitSlice ls index
        mostCommonBitV = leastCommonBitWithTiebreak '0' bitSlice
        nextLs = filter (\l -> (l !! index) == mostCommonBitV) ls
    in nextLs

leastCommonBitWithTiebreak tiebreak bits =
    let (c0, c1) = foldl (\(a, b) v -> case v of
            '1' -> (a, b + 1)
            _ -> (a + 1, b)) (0, 0) bits
    in
        if c0 == c1 then tiebreak
        else if c0 < c1 then '0'
        else '1'

