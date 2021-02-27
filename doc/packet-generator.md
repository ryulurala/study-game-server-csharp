---
title: "Packet Generator"
category: Game-Server
tags: [packet, packet-generator, packet-manager, xml, PDL, singleton, parsing]
date: "2021-02-24"
---

## Packet Generator

- Packet 생성 자동화

  > Packet Class는 Client, Server 모두 가지고 있어야 한다.  
  > 따라서, 매번 Code를 생성하고 정의할 수 없으므로 자동화로 편의성을 높인다.

- Packet 정의 방법

  1. `.json`
  2. `.xml`
  3. `IDL`: Interface Definition Language

### About 자동화 코드

- Packet class
  > What Packet
- PacketManager
  > Server or Client로부터 받은 Packet Deserializing + Handling
  - ClientPacketManager
    > Server로부터 온 Packet Handling 등록  
    > ex. "S\_"
  - ServerPacketManager
    > Client로부터 온 Packet Handling 등록  
    > ex. "C\_"

#### `Packet.cs`

- Client와 Server가 공통으로 송수신할 Packet 정의

```cs
public enum PacketId
{
    // Random 정책도 가능
    Packet1 = 1,
    Packet2 = 2,
    Packet3 = 3,
}

// Packet Interface
interface IPacket
{
    // Protocal Id
    ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}

class Packet1 : IPacket
{
    // 멤버 변수
    // Read(): 멤버 변수 Deserialization
    // Write(): 멤버 변수 Serialization
}
class Packet2 : IPacket
{
    // 멤버 변수
    // Read(): 멤버 변수 Deserialization
    // Write(): 멤버 변수 Serialization
}
class Packet3 : IPacket
{
    // 멤버 변수
    // Read(): 멤버 변수 Deserialization
    // Write(): 멤버 변수 Serialization
}
```

#### `PacketManager.cs`

- Server or CLient로부터 받은 Packet을 Deserializing + Handling
- Server or Client Packet 등록 차이, Class명은 동일

  - `ClientPacketManager.cs`
  - `ServerPacketManager.cs`

- Singleton Pattern

  > 오직 하나의 인스턴스만 존재

- Switch-Case -> Dictionary

  > O(N) -> O(logN): 성능 향상

- 멤버 변수

  - Dictionary\<ushort, Action\<PacketSession, ArraySegment\<byte>>> \_onRecv
    > MakePacket() 콜백으로 실행
  - Dictionary\<ushort, Action\<PacketSession, IPacket>> \_handler
    > Packet Handler 콜백 함수 실행

- 멤버 함수
  - Register()
    > MakePacket() 콜백 등록  
    > Packet Handler 함수 콜백 등록
  - OnRecvPacket()
    > Protocol Id를 읽음  
    > Protocl Id에 해당하는 Packet 조립(Deserialization) 콜백 실행
  - MakePacket()
    > Deserialization Packet  
    > 해당 Packet의 Handler 호출

```cs
class PacketManager
{
    #region Singleton
    // 오직 하나의 인스턴스
    static PacketManager _instance;
    public static PacketManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }
    }
    #endregion

    // <Protocol Id, Receive Handler>
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    // <Protocol Id, Packet Handler>
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
        // MakePacket() 등록
        _onRecv.Add((ushort)PacketId.S_Test, MakePacket<S_Test>);
        // Packet Handler 등록
        _handler.Add((ushort)PacketId.S_Test, PacketHandler.S_TestHandler);
    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        // Deserializing Packet Id, Packet Size
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        // Packet 조립 콜백 실행
        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        // Packet 생성
        T pkt = new T();

        // Deserializing Packet data
        pkt.Read(buffer);

        // Packet Handler 콜백 실행
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }
}
```

### 패킷 생성 자동화

1. PDL.xml 정의
   > What Packet
2. PacketGenerator/program 실행
   > Packet class, Packet Manager 코드 자동 생성
3. batch 파일(`.bat`) 실행
   > 각 Server, Client Project에 Copy & Paste

#### `GenPackets.bat`

- Only. Windows OS
- 명령어
  - START [실행 Path] [Argument: PDL.xml path]
  - XCOPY [Copy 파일] [Paste 파일]
    > `/Y`: 덮어 쓸지 확인 묻지 않기.

```bat
START ../PacketGenerator/bin/Debug/PacketGenerator.exe ../PacketGenerator/PDL.xml
XCOPY /Y Packet.cs "../../client/dummy/packet"
XCOPY /Y Packet.cs "../../server/test/packet"
XCOPY /Y ClientPacketManager.cs "../../client/dummy/packet"
XCOPY /Y ServerPacketManager.cs "../../server/test/packet"
```

#### `PDL.xml`

- Packet Definition List
  - \<packet> ~ \</packet>: 패킷 정의
  - \<long>, \<string>, \<list>: Type 정의
  - [C_]... Client -> Server Packet
  - [S_]... Server -> Client Packet

```cs
<?xml version="1.0" encoding="utf-8" ?>
<PDL>
  <packet name="C_PlayerInfoReq">
    <byte name="testByte"/>
    <long name="playerId"/>
    <string name="name"/>
    <list name="skill">
      <int name="id"/>
      <short name="level"/>
      <float name="duration"/>
      <list name="attribute">
        <int name="att"/>
      </list>
    </list>
  </packet>
  <packet name="S_Test">
    <int name="testInt"/>
  </packet>
</PDL>
```

#### PacketFormat.cs

- Packet을 자동화를 위한 String Format
  - managerFormat
    > PacketManager.cs 전체적인 윤곽
  - managerRegistFormat
    > Handler 콜백 Func 등록 in PacketManager.cs
  - fileFormat
    > Packet.cs 전체적인 윤곽
  - packetEnumFormat
    > Packet Protocal Id 정의
  - packetFormat
    > Packet class 정의
  - memberFormat
    > Packet 안의 Primitive type의 멤버 변수 정의
  - memberListFormat
    > Packet 안의 List type의 멤버 변수 정의
  - readFormat
    > Primitive type(byte 제외) 멤버 변수 Deserialization 구문 정의
  - readByteFormat
    > Byte type 멤버 변수 Deserialization 구문 정의
  - readStringFormat
    > String type 멤버 변수 Deserialization 구문 정의
  - readListFormat
    > List type 멤버 변수 Deserialization 구문 정의
  - writeFormat
    > Primitive type(byte 제외) 멤버 변수 Serialization 구문 정의
  - writeByteFormat
    > Byte type 멤버 변수 Serialization 구문 정의
  - writeStringFormat
    > String type 멤버 변수 Deserialization 구문 정의
  - writeListFormat
    > List type 멤버 변수 Deserialization 구문 정의

```cs
using System;

namespace tools
{
    class PacketFormat
    {
        // {0} 패킷 등록
        public static string managerFormat =
@"using System;
using System.Collections.Generic;
using core;

class PacketManager
{{
    #region Singleton
    static PacketManager _instance;
    public static PacketManager Instance
    {{
        get
        {{
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }}
    }}
    #endregion

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {{
{0}
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {{
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);

    }}

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T pkt = new T();
        pkt.Read(buffer);

        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }}
}}
";
        // {0} 패킷 이름
        public static string managerRegistFormat =
@"        _onRecv.Add((ushort)PacketId.{0}, MakePacket<{0}>);
        _handler.Add((ushort)PacketId.{0}, PacketHandler.{0}Handler);";



        // {0} 패킷 이름/번호 목록
        // {1} 패킷 목록
        public static string fileFormat =
@"using System;
using System.Collections.Generic;
using System.Text;
using core;

public enum PacketId
{{
    {0}
}}

interface IPacket
{{
    ushort Protocol {{ get; }}
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}}

{1}
";

        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";

        // {0} 패킷 이름
        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write
        public static string packetFormat =
@"
class {0} : IPacket
{{
    {1}
    public ushort Protocol {{ get {{ return (ushort)PacketId.{0}; }} }}

    public void Read(ArraySegment<byte> seg)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketId.{0});
        count += sizeof(ushort);

        {3}

        success &= BitConverter.TryWriteBytes(span, count);     // 원본

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }}
}}
";

        // {0} 변수 형식
        // {1} 변수 이름
        public static string memberFormat =
@"public {0} {1};";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        // {2} 멤버 변수들
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 Write
        public static string memberListFormat =
@"
public class {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> span, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> span, ref ushort count)
    {{
        bool success = true;

        {4}

        return true;
    }}
}}
public List<{0}> {1}s = new List<{0}>();
";

        // {0} 변수 이름
        // {1} To- 변수 형식
        // {2} 변수 형식
        public static string readFormat =
@"this.{0} = BitConverter.{1}(span.Slice(count, span.Length - count));
count += sizeof({2});";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string readByteFormat =
@"this.{0} = ({1})seg.Array[seg.Offset + count];
count += sizeof({1});";

        // {0} 변수 이름
        public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
Encoding.Unicode.GetString(span.Slice(count, {0}Len));
count += {0}Len;";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(span, ref count);
    {1}s.Add({1});
}}";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.{0});
count += sizeof({1});";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeByteFormat =
@"seg.Array[seg.Offset + count] = (byte)this.{0};
count += sizeof({1});";

        // {0} 변수 이름
        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, seg.Array, seg.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string writeListFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.{1}s.Count);
count += sizeof(ushort);
foreach ({0} {1} in this.{1}s)
    success &= {1}.Write(span, ref count);";
    }
}
```

#### PacketGenerator/Program.cs

- Program
  - Main()
    > XML parsing
  - ParsePacket()
    > Packet class를 parsing
  - ParseMembers()
    > Packet class의 멤버 변수 parsing
  - ParseList()
    > Packet class의 List type parsing
  - ToMemberType()
    > Encoding 정의(for. Deserializing)
  - FirstCharToUpper()
    > List type Naming
  - FirstCharToLower()
    > List variable Naming

```cs
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
            IgnoreComments = true,  // 주석 무시
            IgnoreWhitespace = true,  // 스페이스바 무시
        };

        // Packet Generator Program을 실행할 시 argument로 PDL.xml Path 지정
        if (args.Length >= 1)
            pdlPath = args[0];

        // 자동화
        using (XmlReader reader = XmlReader.Create(pdlPath, settings))
        {
            // 헤더를 건너 뛰고 핵심 내용부터
            reader.MoveToContent();

            // PDL parsing: String으로 읽음
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
            File.WriteAllText("Packets.cs", fileText);

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
            // packet 안쪽 내용이 아닐 경우 return
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
```

---
