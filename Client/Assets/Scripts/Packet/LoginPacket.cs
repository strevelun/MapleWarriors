
// �α��� ���� �Լ�

public static class LoginPacketMaker
{
	public static Packet CheckNickname(string _nickname)
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.Client.LoginReq)
			.Add(_nickname)
			.Add(77);
		return packet;
	}
}