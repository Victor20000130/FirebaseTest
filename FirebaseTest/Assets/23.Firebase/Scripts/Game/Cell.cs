using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PED = UnityEngine.EventSystems.PointerEventData;

public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public string coodinate;

	public Board board;

	public async void OnPointerClick(PED eventData)
	{
		//print(coodinate);

		bool ismyFukingTurn = await FirebaseManager.Instance.GetTurn();
		if (ismyFukingTurn) board.SelectCell(this);

	}

	public void OnPointerEnter(PED eventData)
	{

	}

	public void OnPointerExit(PED eventData)
	{

	}


}
