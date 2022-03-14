<Query Kind="Program" />

void Main()
{
    var inputPath = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "advent2020puzzle4.txt");
    var file = File.ReadAllText(inputPath);
    //Part1(file);
    Part2(file);
    //Part2(Part2DebugInput());
    
    // Part2: 2 wrong (I had cm and in flipped for height)
    // Part2: 176 wrong (forgot to put ^ and $ in regex)
}

void Part1(string file)
{
    var passportTexts = file.Split("\n\n");
    var passportFields = passportTexts
        .Select(text => text
            .Split(new[] { "\n", " " }, StringSplitOptions.RemoveEmptyEntries)
            .Select(field => field.Split(":"))
            .Where(field => field[0] != "cid")
        )
        .Count(e => e.Count() == 7)
        .Dump();
}

string Part2DebugInput()
{
    return @"eyr:1972 cid:100
hcl:#18171d ecl:amb hgt:170 pid:186cm iyr:2018 byr:1926

iyr:2019
hcl:#602927 eyr:1967 hgt:170cm
ecl:grn pid:012533040 byr:1946

hcl:dab227 iyr:2012
ecl:brn hgt:182cm pid:021572410 eyr:2020 byr:1992 cid:277

hgt:59cm ecl:zzz
eyr:2038 hcl:74454a iyr:2023
pid:3556412378 byr:2007

pid:087499704 hgt:74in ecl:grn iyr:2012 eyr:2030 byr:1980
hcl:#623a2f

eyr:2029 ecl:blu cid:129 byr:1989
iyr:2014 pid:896056539 hcl:#a97842 hgt:165cm

hcl:#888785
hgt:164cm byr:2001 iyr:2015 cid:88
pid:545766238 ecl:hzl
eyr:2022

iyr:2010 hgt:158cm hcl:#b6652a ecl:blu byr:1944 eyr:2021 pid:093154719".Replace("\r\n", "\n");

}

void Part2(string file)
{
       
    var passportTexts = file.Split("\n\n");
    var passportFields = passportTexts
        .Select(text => text
            .Split(new[] { "\n", " " }, StringSplitOptions.RemoveEmptyEntries)
            .Select(field => field.Split(":"))
            .Where(field => field[0] != "cid")
            .Where(field => field[0] != "byr" || IsInRange(field[1], 1920, 2002))
            .Where(field => field[0] != "iyr" || IsInRange(field[1], 2010, 2020))
            .Where(field => field[0] != "eyr" || IsInRange(field[1], 2020, 2030))
            .Where(field => field[0] != "hgt" || IsValidHeight(field[1]))
            .Where(field => field[0] != "hcl" || Regex.IsMatch(field[1], "^#[0-9a-f]{6}$"))
            .Where(field => field[0] != "ecl" || "amb blu brn gry grn hzl oth".Split(" ").Contains(field[1]))
            .Where(field => field[0] != "pid" || Regex.IsMatch(field[1], "^[0-9]{9}$"))
        )
        .Count(e => e.Count() == 7)
        .Dump();
}

bool IsValidHeight(string value)
{
    if (value.EndsWith("cm"))
    {
        return IsInRange(value.Substring(0, value.Length - 2), 150, 193);
    }
    if (value.EndsWith("in"))
    {
        return IsInRange(value.Substring(0, value.Length - 2), 59, 76);
    }
    return false;
}

bool IsInRange(string value, int min, int max)
{
    return int.TryParse(value, out var num)
        && num >= min
        && num <= max;
}

// You can define other methods, fields, classes and namespaces here