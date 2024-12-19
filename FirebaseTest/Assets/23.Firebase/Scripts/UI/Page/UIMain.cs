using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMain : UIPage
{
	public Button signInButton;

	private void Awake()
	{
		signInButton.onClick.AddListener(SignInButtonClick);
	}

	public void SignInButtonClick()
	{
		UIManager.Instance.PageOpen<UISignIn>();
	}
}
