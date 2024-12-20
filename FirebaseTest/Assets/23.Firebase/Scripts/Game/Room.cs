using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomState
{
	Waiting,
	Playing,
	Finished
}

[System.Serializable]
public class Room
{
	public string host;
	public string guest;
	public RoomState state;

	//����Ʈ�� �� �� ������ json���� ��ȯ���� �� ��ųʸ��� ���
	//Ű�� ���� ���������, ����Ʈ�� ��� �迭([])�̱� ������
	//�����͸� ��ȯ�ϴ� �������� ��ųʸ��� �� �������̶󼭴�.
	public Dictionary<int, Turn> turn = new Dictionary<int, Turn>();
}

[System.Serializable]
public class Turn
{
	public bool isHostTurn;
	public string coodinate;
}