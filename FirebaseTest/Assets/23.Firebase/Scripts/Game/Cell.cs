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

	public bool isClick = false;

	private GameObject blueMark;
	private GameObject redMark;

	private void Awake()
	{
		blueMark = Instantiate(board.blueMark, transform);
		redMark = Instantiate(board.redMark, transform);
		blueMark.SetActive(false);
		redMark.SetActive(false);
	}

	public async void OnPointerClick(PED eventData)
	{
		//print(coodinate);
		if (isClick) return;

		bool ismyFukingTurn = await FirebaseManager.Instance.GetTurn();
		if (ismyFukingTurn)
		{
			board.SelectCell(this);
			//isClick = true;
		}
	}

	public void OnPointerEnter(PED eventData)
	{
		if (isClick) return;
		if (board.isHost)
		{
			blueMark.SetActive(true);
		}
		else
		{
			redMark.SetActive(true);
		}
	}

	public void OnPointerExit(PED eventData)
	{
		if (isClick) return;
		if (board.isHost)
		{
			blueMark.SetActive(isClick);
		}
		else
		{
			redMark.SetActive(isClick);
		}
	}


}
