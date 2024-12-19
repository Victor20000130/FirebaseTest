using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable] //json���� ������ ���̱� ������ ����ȭ
public class UserData
{
	public enum UserClass
	{
		Warrior,
		Wizard,
		Rogue,
		Archer
	}

	private string userId { get; set; }
	public string userName;
	public int level;
	public int gold;
	public UserClass userClass;
	public float exp;
	public int gem;

	public UserData() { } //�⺻ ������

	//ȸ�� ������ �� �� ����
	public UserData(string userId)
	{
		this.userId = userId;
		userName = "������ ������";
		level = 1;
		gold = 0;
		userClass = UserClass.Warrior;
		exp = 0.0f;
		gem = 0;
	}

	public UserData(string userId, string userName, int level, int gold, UserClass userClass, float exp, int gem)
	{   //�α����� �� �� ������
		this.userId = userId;
		this.userName = userName;
		this.level = level;
		this.gold = gold;
		this.userClass = userClass;
		this.exp = exp;
		this.gem = gem;
	}
}

public class DatabasePacket
{
	//username
	public string aa;
	//level
	public int ab;
}