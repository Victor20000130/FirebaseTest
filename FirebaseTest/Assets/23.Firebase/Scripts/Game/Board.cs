using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Xml.Serialization;
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

	//private int[][] board;


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
				//board[y][x] = 0;
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

		if (isBlue)
		{
			targetCell.whoisyourMaster = 1;
		}
		else
		{
			targetCell.whoisyourMaster = 2;
		}
		Instantiate(prefab, targetCell.transform, false);
		targetCell.isClick = true;
		Parssing(coodinate);
	}


	private void Parssing(string coodinate)
	{
		int x = coodinate[0] - 65;
		int y = coodinate[1] - 49;
		Vector2 point = new Vector2(x, y);
		CheckUpDown(point, 0, 1);
	}

	int count = 0;
	private void CheckUpDown(Vector2 point, int xDir, int yDir)
	{
		string recood = $"{(char)(point.x + 65)}{point.y + 1}";
		Cell nextCell = cellDictionary[recood];
		if (nextCell.isClick)
		{
			count += nextCell.whoisyourMaster;
		}
		else
		{
			CheckUpDown(point, xDir, yDir);
		}


		if (count == 4)
		{
			return;
		}

	}
}