
// 로그인 관련 함수

public static class LoginPacketMaker
{
	public static Packet CheckNickname()
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.Client.LoginReq)
			.Add(UserData.Inst.Nickname);
		return packet;
	}
}