using System;
using System.IO;
using System.Xml;

namespace tools
{
    class Program
    {
        static string packetClass;   // Packet class
        static ushort packetId;     // Packet Protocol Id
        static string packetEnums;  // Packet enum 지정

        static string clientRegister;   // for. ClientPacketManager
        static string serverRegister;   // for. ServerPacketManager

        static void Main(string[] args)
        {
            // PDL Path
            string pdlPath = "PDL.xml";

            // 환경 설정
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            // Packet Generator Program을 실행할 시 argument로 PDL.xml Path 지정
            if (args.Length >= 1)
                pdlPath = args[0];

            // 자동화
            using (XmlReader reader = XmlReader.Create(pdlPath, settings))
            {
                // 헤더를 건너 뜀.
                reader.MoveToContent();

                // PDL parsing
                while (reader.Read())
                {
                    // Depth가 0: <PDL>, </PDL>
                    // XmlNodeType.Element: <packet>
                    // XmlNodeType.EndElement: </packet>
                    if (reader.Depth == 1 && reader.NodeType == XmlNodeType.Element)
                        ParsePacket(reader);
                }
                // Packet.cs
                string fileText = string.Format(PacketFormat.fileFormat, packetEnums, packetClass);
                File.WriteAllText("Packet.cs", fileText);

                // ClientManager.cs
                string clientManagerText = string.Format(PacketFormat.managerFormat, clientRegister);
                File.WriteAllText("ClientPacketManager.cs", clientManagerText);

                // ServerManager.cs
                string serverManagerText = string.Format(PacketFormat.managerFormat, serverRegister);
                File.WriteAllText("ServerPacketManager.cs", serverManagerText);
            }
        }

        static void ParsePacket(XmlReader reader)
        {
            // </> 일 경우 return
            if (reader.NodeType == XmlNodeType.EndElement)
                return;

            // <packet>이 아닐 경우 return
            if (reader.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invalid packet node");
                return;
            }

            // <packet name="">일 경우 return
            string packetName = reader["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }

            // Packet.cs format append
            Tuple<string, string, string> tuple = ParseMembers(reader);
            packetClass += string.Format(PacketFormat.packetFormat, packetName, tuple.Item1, tuple.Item2, tuple.Item3);

            // Packet.cs의 enum 증가
            packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId) + Environment.NewLine + "\t";

            // ClientPacketManager: Client's Packet Handler를 다룸, "S_": Server's Packet
            // ServerPacketManager: Server's Packet Handler를 다룸.
            if (packetName.StartsWith("S_") || packetName.StartsWith("s_"))
                clientRegister += string.Format(PacketFormat.managerRegistFormat, packetName) + Environment.NewLine;
            else
                serverRegister += string.Format(PacketFormat.managerRegistFormat, packetName) + Environment.NewLine;
        }

        static Tuple<string, string, string> ParseMembers(XmlReader reader)
        {
            string memberCode = ""; // 멤버 변수
            string readCode = "";   // 멤버 변수 Deserializing
            string writeCode = "";  // 멤버 변수Serializing

            int depth = reader.Depth + 1;
            while (reader.Read())
            {
                // packet class 안쪽 depth만
                if (reader.Depth != depth)
                    break;

                // <type name="">일 경우 return
                string memberName = reader["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                // 개행 추가
                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine;
                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine;
                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine;

                // type별 parsing
                string memberType = reader.Name.ToLower();
                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        // Only. byte, sbyte type
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                        writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        // Only. bool, short, ushort, int, long, float, double type
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        // Only. string type
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        // Only. list type
                        Tuple<string, string, string> tuple = ParseList(reader);
                        memberCode += tuple.Item1;
                        readCode += tuple.Item2;
                        writeCode += tuple.Item3;
                        break;
                    default:
                        break;
                }
            }
            // 탭, 개행 정리
            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        static Tuple<string, string, string> ParseList(XmlReader reader)
        {
            // <list name="">일 경우 return
            string listName = reader["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            // list안의 멤버 변수 parsing
            Tuple<string, string, string> tuple = ParseMembers(reader);

            // 멤버 변수
            string memberCode = string.Format(PacketFormat.memberListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName),
                tuple.Item1,
                tuple.Item2,
                tuple.Item3);

            // list 멤버 변수 Deserializing 구문
            string readCode = string.Format(PacketFormat.readListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            // list 멤버 변수 Serializing 구문
            string writeCode = string.Format(PacketFormat.writeListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        static string ToMemberType(string memberType)
        {
            // 멤버 변수별 Encoding
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        static string FirstCharToUpper(string input)
        {
            // for. List type naming
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }
        static string FirstCharToLower(string input)
        {
            // for. List naming
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}
