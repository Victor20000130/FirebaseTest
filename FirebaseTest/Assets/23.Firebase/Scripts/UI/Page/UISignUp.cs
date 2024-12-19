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
				.SetPopUp("�˸�", "6���� �̻��� ��й�ȣ�� �Է����ּ���.");
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
			.SetPopUp("ȸ������ �Ϸ�", "ȸ�������� �Ϸ�Ǿ����ϴ�.\n�α����� ���ּ���.");
	}

	private void SignInButtonClick()
	{
		UIManager.Instance.PageOpen<UISignIn>();
	}
}
