


split delim [] = [[]]
--split delim (c:cs) = case c == delim of
--    True -> let newword = ""
--            in newword:rest
--    False -> (c:w):ws
--    where rest@(w:ws) = split delim cs

split delim (c:cs) =
    let rest@(w:ws) = split delim cs
        t = "":rest
        f = (c:w):ws
    in if delim == c then t else f

testsplit = do
    testcase ':' "" [""]
    testcase ':' "abc" ["abc"]
    testcase ':' "abc:" ["abc", ""]
    testcase ':' "abc:def" ["abc", "def"]

testcase delim s expected = do
    let result = split delim s
    let pass = result == expected
    if pass
        then print "pass"
        else print ("failed", "expected", expected, "actual", result)

main = testsplit
