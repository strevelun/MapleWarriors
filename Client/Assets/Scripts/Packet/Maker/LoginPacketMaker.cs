
// 로그인 관련 함수

public static class LoginPacketMaker
{
	public static Packet ExitGame()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.Exit);
		return pkt;
	}

	public static Packet CheckNickname()
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.eClient.LoginReq)
			.Add(UserData.Inst.Nickname);

		return packet;
	}
}