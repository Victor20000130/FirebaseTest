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

	//리스트를 안 쓴 이유는 json으로 변환했을 때 딕셔너리의 경우
	//키와 값이 페어이지만, 리스트의 경우 배열([])이기 때문에
	//데이터를 변환하는 과정에서 딕셔너리가 더 안정적이라서다.
	public Dictionary<int, Turn> turn = new Dictionary<int, Turn>();
}

[System.Serializable]
public class Turn
{
	public bool isHostTurn;
	public string coodinate;
}