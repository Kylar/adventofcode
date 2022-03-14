<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
    var inputPath = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "day16input.txt");
    var input = File.ReadAllText(inputPath);
    //input = "D2FE28"; // literal
    //input = "38006F45291200"; // 2 sub-packets (sub-packets length)
    //input = "EE00D40C823060"; // 3 sub-packets (num sub-packets)

   
    //input.Dump();
    //bits.Dump();

    var packet = ParseInput(input);
    
    packet.Dump();
    
    Part1(packet).Dump("p[art 1");
    Part2(packet).Dump("part2");
    
    
    new[]
    {
"C200B40A82",// finds the sum of 1 and 2, resulting in the value 3.
"04005AC33890",// finds the product of 6 and 9, resulting in the value 54.
"880086C3E88112",// finds the minimum of 7, 8, and 9, resulting in the value 7.
"CE00C43D881120",// finds the maximum of 7, 8, and 9, resulting in the value 9.
"D8005AC2A8F0",// produces 1, because 5 is less than 15.
"F600BC2D8F",// produces 0, because 5 is not greater than 15.
"9C005AC2F8F0",// produces 0, because 5 is not equal to 15.
"9C0141080250320F1802104A08",//produces 1, because 1 + 3 = 2 * 2.
    }
    .Select(s => Part2(ParseInput(s)))
        .Dump();
    
}

Packet ParseInput(string input)
{
    var hexToDigits = "0123456789ABCDEF"
    .Select((c, i) => (Char: c, Index: i))
    .ToDictionary(e => e.Char, e =>
    {
        var index = e.Index;
        var bit0 = index & 8;
        var bit1 = index & 4;
        var bit2 = index & 2;
        var bit3 = index & 1;
        return new[] { bit0, bit1, bit2, bit3 }.Select(b => b > 0 ? '1' : '0');
    });

    string bits = new String(input.Trim().SelectMany(c => hexToDigits[c]).ToArray());

    var (pos, packet) = ParseOnePacket(bits, pos: 0);
    return packet;
}

long Part1(Packet packet)
{
    
    return packet.version + packet.subPackets.Sum(p => Part1(p));
}

long Part2(Packet packet)
{
    if (packet.typeId == 4)
    {
        return packet.value.Value;
    }
    if (packet.typeId == 0)
    {
        return packet.subPackets.Select(Part2).Sum();
    }
    if (packet.typeId == 1)
    {
        return packet.subPackets.Select(Part2).Aggregate(1L, (acc, v) => acc * v);
    }
    if (packet.typeId == 2)
    {
        return packet.subPackets.Select(Part2).Min();
    }
    if (packet.typeId == 3)
    {
        return packet.subPackets.Select(Part2).Max();
    }
    if (packet.typeId == 5)
    {
        var values = packet.subPackets.Select(Part2).ToArray();
        return values[0] > values[1] ? 1 : 0;
    }
    if (packet.typeId == 6)
    {
        var values = packet.subPackets.Select(Part2).ToArray();
        return values[0] < values[1] ? 1 : 0;
    }
    if (packet.typeId == 7)
    {
        var values = packet.subPackets.Select(Part2).ToArray();
        return values[0] == values[1] ? 1 : 0;
    }
    throw new Exception("allaslfjasdlk");
}

(int Pos, Packet Packet) ParseOnePacket(string bits, int pos)
{
    string ReadBits(int n)
    {
        var result = bits.Substring(pos, n);
        pos += n;
        return result;
    }

    var version = ParseBinary(ReadBits(3));
    //version.Dump("version");
    var typeId = ParseBinary(ReadBits(3));
    //typeId.Dump("type id");
    if (typeId == 4)
    {
        //"processing literal".Dump();
        // literal
        var valueParts = new StringBuilder();
        bool hasMore;
        do
        {
            hasMore = ReadBits(1) == "1";
            var valuePart = ReadBits(4);
            //valuePart.Dump("value part");
            valueParts.Append(valuePart);
        } while (hasMore);
        return (pos, new Packet(version, typeId, ParseBinary(valueParts.ToString()), new Packet[0]));//.Dump("return value");
    }
    else
    {
        //"processing operator".Dump();
        // operator
        var lengthTypeId = ReadBits(1);
        var packets = new List<Packet>();
        if (lengthTypeId == "0")
        {
            //"length type: packet length".Dump();
            var subPacketsLength = ParseBinary(ReadBits(15));
            //subPacketsLength.Dump("subpackets length");
            var finalPos = subPacketsLength + pos;
            while (pos < finalPos)
            {
                var (newPos, packet) = ParseOnePacket(bits, pos);
                pos = newPos;
                packets.Add(packet);
            }
            
        }
        else
        {
            //"length type: num packets".Dump();

            var numSubPackets = ParseBinary(ReadBits(11));
            //numSubPackets.Dump("num sub-packets");
            for (var i = 0; i < numSubPackets; i++)
            {
                var (newPos, packet) = ParseOnePacket(bits, pos);
                pos = newPos;
                packets.Add(packet);
            }
        }
        return (pos, new Packet(version, typeId, null, packets.ToArray()));//.Dump("return value");
    }
}

record Packet(long version, long typeId, long? value, Packet[] subPackets);

long ParseBinary(string bits)
{
    long result = 0;
    for (var i = 0; i < bits.Length; i++)
    {
        result = result * 2 + (bits[i] == '0' ? 0 : 1);
    }
    return result;
}
