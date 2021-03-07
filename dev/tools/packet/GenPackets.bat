START ../PacketGenerator/bin/Debug/PacketGenerator.exe ../PacketGenerator/PDL.xml
XCOPY /Y Packet.cs "../../client/dummy/packet"
XCOPY /Y Packet.cs "../../client/unity/unity-client/Assets/Scripts/Packet"
XCOPY /Y Packet.cs "../../server/test/packet"
XCOPY /Y ClientPacketManager.cs "../../client/dummy/packet"
XCOPY /Y ClientPacketManager.cs "../../client/unity/unity-client/Assets/Scripts/Packet"
XCOPY /Y ServerPacketManager.cs "../../server/test/packet"