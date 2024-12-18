using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDialog : MonoBehaviour
{
	public TextMeshProUGUI title;
	public TextMeshProUGUI message;
	public Button closeButton;

	private Action callBack;

	private void Awake()
	{
		closeButton.onClick.AddListener(CloseButtonClick);
	}

	public void SetDialog(string title, string message, Action callBack = null)
	{
		this.title.text = title;
		this.message.text = message;
		this.callBack = callBack;
	}
	private void CloseButtonClick()
	{
		if (callBack is null)
		{
			UIManager.Instance.PageOpen<UIHome>();
		}
		else
		{
			callBack();
		}
	}
}
