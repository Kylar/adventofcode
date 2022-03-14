module Main where

import Data.List
import Data.List.Split

input = readFile "/mnt/c/k/adventofcode/2021/day4/day4input.txt"
sampleinput = do
    txt <- readFile "/mnt/c/k/adventofcode/2021/day4/day4exampleinput.txt"
    return (filter (/= '\r') txt)

main :: IO ()
main = do
    part1 input
    part2 input

type Cell = Integer
type MarkedCell = (Integer, Bool)

type Board = [[Cell]]
type MarkedBoard = [[MarkedCell]]

part1 input = do
    txt <- input
    let (nums, boards) = parseInput txt
    let markedBoards = map (map (map (\x -> (x, False)))) boards
    let (winningBoard, n) = solvePart1 nums markedBoards
    let answer = boardValue winningBoard * n
    --print ("answer", (take 10 nums, take 3 boards, take 1 markedBoards))
    print ("answer", answer)


solvePart1 :: [Integer] -> [MarkedBoard] -> (MarkedBoard, Integer)
solvePart1 (n:ns) bs =
    let nextBs = map (markBoard n) bs
        winningBoards = filter isWinningBoard nextBs
    in case winningBoards of
        (b:_) -> (b, n)
        _ -> solvePart1 ns nextBs

boardValue b = sum (map fst (filter (not . snd) (concat b)))

markBoard :: Integer -> [[(Integer, Bool)]] -> [[(Integer, Bool)]]
markBoard n b = map (map (markCell n)) b

markCell :: Integer -> MarkedCell -> MarkedCell
markCell n cell@(v, _) = if v == n then (v, True) else cell

isWinningBoard b =
    let isRowWinning = all snd
        isAnyRowWinning = any isRowWinning
        isBoardWinning b = any isAnyRowWinning [b ++ transpose b]
    in isBoardWinning b

isMarked x y b = snd (b !! y !! x)

parseInput txt = let
    ls = splitOn "\n\n" txt
    nums = parseNums (head ls)
    boards = map parseBoard (tail ls)
    in (nums, boards)
    
parseNums l = map read (splitOn "," l) :: [Integer]

parseBoard txt = let
    ls = lines txt
    in map parseBoardRow ls

parseBoardRow :: [Char] -> [Integer]
parseBoardRow l = map read $ filter (/= "") $ splitOn " " l



part2 input = do
    txt <- input
    let (nums, boards) = parseInput txt
    let markedBoards = map (map (map (\x -> (x, False)))) boards
    let (winningBoard, n) = solvePart2 nums markedBoards
    let answer = boardValue winningBoard * n
    --print ("answer", (take 10 nums, take 3 boards, take 1 markedBoards))
    print ("answer", answer)

solvePart2 :: [Integer] -> [MarkedBoard] -> (MarkedBoard, Integer)
solvePart2 ns bs = last (allWinningBoards ns bs)

allWinningBoards :: [Integer] -> [MarkedBoard] -> [(MarkedBoard, Integer)]
allWinningBoards ns [] = []
allWinningBoards [] bs = []
allWinningBoards (n:ns) bs =
    let nextBs = map (markBoard n) bs
        winningBoards = filter isWinningBoard nextBs
        otherBoards = filter (not . isWinningBoard) nextBs
    in (map (\b -> (b, n)) winningBoards) ++ allWinningBoards ns otherBoards

