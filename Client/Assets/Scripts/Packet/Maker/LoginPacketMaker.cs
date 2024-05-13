
// 로그인 관련 함수

public static class LoginPacketMaker
{
	public static Packet LoginReq(int _port)
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.eClient.LoginReq)
			.Add(UserData.Inst.Nickname)
			.Add((ushort)_port);

		return packet;
	}
}