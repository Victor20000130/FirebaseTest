using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UITwoButtonPopUp : UIPopUp
{
	public Button okButton;
	private bool isOk = false;

	protected override void Awake()
	{
		base.Awake();
		okButton.onClick.AddListener(OkButtonClick);

	}

	public void SetPopUp(string title, string message, Action<bool> callback)
	{
		base.SetPopUp(title, message, () => callback?.Invoke(isOk));
	}

	private void OkButtonClick()
	{
		isOk = true;
		callBack?.Invoke();
		UIManager.Instance.PopUpClose();
	}

	protected override void CloseButtonClick()
	{
		isOk = false;
		callBack?.Invoke();
		UIManager.Instance.PopUpClose();
	}
}
