
input = readFile "/mnt/c/k/adventofcode/2021/day2input.txt"

part1 = do
    text <- input
    let ls = map parseLine $ lines text
    let horz = foldr (\(w, v) acc -> acc + if w == "forward" then v else 0) 0 ls
    let depth = foldr (\v acc -> acc + depthValue v) 0 ls
    let answer = horz * depth
    print ("part1 horz", horz)
    print ("part1 depth", depth)
    print ("part1 answer", answer)

depthValue (w, v) = case w of
    "up" -> -v
    "down" -> v
    _ -> 0

parseLine line =
    let (word:valueStr:[]) = words line
    in (word, (read valueStr) :: Integer)

part2 = do
    text <- input
    let ls = map parseLine $ lines text
    let (aim, horz, depth) = foldl part2f (0, 0, 0) ls
    let answer = horz * depth
    print ("part1 horz", horz)
    print ("part1 depth", depth)
    print ("part1 answer", answer)

part2f (aim, horz, depth) (w, v) = case w of
    "forward" -> (aim, horz + v, depth + aim * v)
    "up" -> (aim - v, horz, depth)
    "down" -> (aim + v, horz, depth)

--- Rewrites
--
part1v2 = do
    text <- input
    let ls = map parseLine $ lines text
    let f acc (w, v) = acc + case w of
            "forward" -> v
            _ -> 0
    let horz = foldl f 0 ls
    let depth = foldl (\acc v -> acc + depthValue v) 0 ls
    let answer = horz * depth
    print ("part1 horz", horz)
    print ("part1 depth", depth)
    print ("part1 answer", answer)
