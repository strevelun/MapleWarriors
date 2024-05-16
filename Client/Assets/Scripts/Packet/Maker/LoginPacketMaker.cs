
// 로그인 관련 함수

public static class LoginPacketMaker
{
	public static Packet LoginReq(int _udpPort, byte[] _ipBytes, int _port)
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.eClient.LoginReq)
			.Add(UserData.Inst.Nickname)
			.Add((ushort)_udpPort)
			.Add(_ipBytes[0])
			.Add(_ipBytes[1])
			.Add(_ipBytes[2])
			.Add(_ipBytes[3]);

		return packet;
	}
}