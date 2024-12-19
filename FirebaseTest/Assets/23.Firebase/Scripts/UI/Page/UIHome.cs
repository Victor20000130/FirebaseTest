using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using UniRan = UnityEngine.Random;

public class UIHome : UIPage
{
	public Image profileImage;
	public TextMeshProUGUI displayName;
	public TextMeshProUGUI gold;
	public TextMeshProUGUI level;
	public TextMeshProUGUI userClass;
	public TextMeshProUGUI exp;
	public TextMeshProUGUI gem;

	public Button profileChangeButton;
	public Button addGoldButton;
	public Button signOutButton;
	public Button huntButton;
	public Button addGemButton;

	public Slider expSlider;

	private void Awake()
	{
		profileChangeButton.onClick.AddListener(ProfileChangeButtonClick);
		addGoldButton.onClick.AddListener(AddGoldButtonClick);
		signOutButton.onClick.AddListener(SignOutButtonClick);
		huntButton.onClick.AddListener(HuntButtonClick);
		addGemButton.onClick.AddListener(AddGemButtonClick);
	}

	private void HuntButtonClick()
	{
		UserData data = FirebaseManager.Instance.currentUserData;
		data.exp += 10.5f;

		data.level = ((int)data.exp / 100) + 1;
		FirebaseManager.Instance.UpdateUserData("exp", data.exp, (x) => { SetUserData(data); });
		FirebaseManager.Instance.UpdateUserData("level", data.level, (x) => { SetUserData(data); });
	}


	private void SignOutButtonClick()
	{
		FirebaseManager.Instance.SignOut();
		UIManager.Instance.PageOpen<UIMain>();
	}

	public void AddGoldButtonClick()
	{
		UserData data = FirebaseManager.Instance.currentUserData;
		data.gold += 10;

		FirebaseManager.Instance.UpdateUserData("gold", data.gold, (x) => { SetUserData(data); });
	}
	public void AddGemButtonClick()
	{
		UserData data = FirebaseManager.Instance.currentUserData;
		data.gem += 10;
		FirebaseManager.Instance.UpdateUserData("gem", data.gem, (x) => { SetUserData(data); });
	}

	public void ProfileChangeButtonClick()
	{
		UIManager.Instance.PopUpOpen<UIInputFieldPopUp>().SetPopUP("닉네임 변경", "변경할 닉네임 입력.", ProfileChangeCallback);
	}

	private void ProfileChangeCallback(string newName)
	{
		FirebaseManager.Instance.UpdateUserProfile(newName, SetInfo);
	}


	public void SetInfo(FirebaseUser user)
	{
		UserData data = FirebaseManager.Instance.currentUserData;
		displayName.text = data.userName;
		FirebaseManager.Instance.UpdateUserData("userName", data.userName, (x) => { SetUserData(data); });
		if (user.PhotoUrl != null)
		{
			SetPhoto(user.PhotoUrl.AbsoluteUri);
		}
		else
		{
			SetPhoto("");
		}
	}

	public async void SetPhoto(string url)
	{
		if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
		{
			using (HttpClient client = new HttpClient())
			{
				byte[] response = await client.GetByteArrayAsync(uri);

				Texture2D texture = new Texture2D(1, 1);

				texture.LoadImage(response);

				Sprite sprite =
					Sprite.Create(
					texture,
					new Rect(0, 0, texture.width, texture.height),
					new Vector2(0.5f, 0.5f)
					);
				profileImage.sprite = sprite;
			}
		}
		else
		{
			profileImage.sprite = null;
		}
	}

	public void SetUserData(UserData userData)
	{
		try
		{
			gold.text = userData.gold.ToString();
			userClass.text = userData.userClass.ToString();
			level.text = userData.level.ToString();
			exp.text = (userData.exp % 100).ToString();
			expSlider.value = (userData.exp % 100) * 0.01f;
			gem.text = userData.gem.ToString();
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message);
		}
	}

}
