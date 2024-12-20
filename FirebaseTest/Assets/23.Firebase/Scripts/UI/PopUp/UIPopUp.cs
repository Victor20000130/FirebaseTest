using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPopUp : MonoBehaviour
{
	protected Action callBack;

	public TextMeshProUGUI title;
	public TextMeshProUGUI message;

	public Button closeButton;

	protected virtual void Awake()
	{
		closeButton.onClick.AddListener(CloseButtonClick);

	}

	public virtual void SetPopUp(string title, string message, Action callBack = null)
	{
		this.title.text = title;
		this.message.text = message;
		this.callBack = callBack;
	}


	protected virtual void CloseButtonClick()
	{
		UIManager.Instance.PopUpClose();
		callBack?.Invoke();
	}
}
