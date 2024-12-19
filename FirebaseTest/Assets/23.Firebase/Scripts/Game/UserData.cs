using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable] //json으로 변경할 것이기 때문에 직렬화
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

	public UserData() { } //기본 생성자

	//회원 가입할 때 쓸 상자
	public UserData(string userId)
	{
		this.userId = userId;
		userName = "광야의 떠돌이";
		level = 1;
		gold = 0;
		userClass = UserClass.Warrior;
		exp = 0.0f;
		gem = 0;
	}

	public UserData(string userId, string userName, int level, int gold, UserClass userClass, float exp, int gem)
	{   //로그인할 때 쓸 생성자
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