using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
	public Cell[] cells;
	public Dictionary<string, Cell> cellDictionary = new Dictionary<string, Cell>();

	public GameObject blueMark;
	public GameObject redMark;

	public bool isHost;
	public int turnCount = 0;

	private void Awake()
	{
		cells = GetComponentsInChildren<Cell>();
		int cellNum = 0;
		for (int y = 0; y < 8; y++)
		{
			for (int x = 0; x < 8; x++)
			{
				cells[cellNum].coodinate = $"{(char)(x + 65)}{y + 1}";
				cellNum++;
			}
		}

		foreach (Cell cell in cells)
		{
			cell.board = this;
			cellDictionary.Add(cell.coodinate, cell);
		}
	}

	public void SelectCell(Cell cell)
	{
		Turn turn = new Turn()
		{
			isHostTurn = isHost,
			coodinate = cell.coodinate
		};
		FirebaseManager.Instance.SendTurn(turnCount, turn);
	}

	public void PlaceMark(bool isBlue, string coodinate)
	{

		GameObject prefab = isBlue ? blueMark : redMark;

		Cell targetCell = cellDictionary[coodinate];

		Instantiate(prefab, targetCell.transform, false);
		//cellDictionary.Remove(coodinate);

	}
}
