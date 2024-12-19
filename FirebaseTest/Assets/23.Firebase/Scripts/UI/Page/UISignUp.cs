using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISignUp : UIPage
{
	public TMP_InputField email;
	public TMP_InputField passwd;
	public Button signUpButton;
	public Button signInButton;

	private void Awake()
	{
		signInButton.onClick.AddListener(SignInButtonClick);
		signUpButton.onClick.AddListener(SignUpButtonClick);
	}
	private void SignUpButtonClick()
	{
		if (passwd.text.Length < 6)
		{
			UIManager.Instance.PopUpOpen<UIDialogPopUp>()
				.SetPopUp("알림", "6글자 이상의 비밀번호로 입력해주세요.");
		}
		else
		{
			FirebaseManager.Instance.Create(email.text, passwd.text, CreateCallback);
		}
	}
	private void DialogCallback()
	{
		UIManager.Instance.PageOpen(GetType().Name);
	}

	private void CreateCallback(FirebaseUser user, UserData userData)
	{
		UIManager.Instance.PopUpOpen<UIDialogPopUp>()
			.SetPopUp("회원가입 완료", "회원가입이 완료되었습니다.\n로그인을 해주세요.");
	}

	private void SignInButtonClick()
	{
		UIManager.Instance.PageOpen<UISignIn>();
	}
}
