using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInputFieldPopUp : UIPopUp
{
	public TMP_InputField inputField;

	public void SetPopUP(string title, string message, Action<string> callback)
	{
		base.SetPopUp(title, message, () => { callback?.Invoke(inputField.text); });
	}
}
