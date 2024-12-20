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

	public Button rankButton;
	public Button messageButton;
	public Button inviteButton;
	public Board gameBoard;

	public Slider expSlider;

	string messageTarget;

	private Room currentRoom;

	private void Awake()
	{
		profileChangeButton.onClick.AddListener(ProfileChangeButtonClick);
		addGoldButton.onClick.AddListener(AddGoldButtonClick);
		signOutButton.onClick.AddListener(SignOutButtonClick);
		huntButton.onClick.AddListener(HuntButtonClick);
		addGemButton.onClick.AddListener(AddGemButtonClick);
		rankButton.onClick.AddListener(RankButtonClick);
		messageButton.onClick.AddListener(MessageButtonClick);
		inviteButton.onClick.AddListener(InviteButtonClick);

		gameBoard.gameObject.SetActive(false);


	}

	private void Start()
	{
		FirebaseManager.Instance.onGameStart += GameStart;
		FirebaseManager.Instance.onTurnProcceed += ProccessTurn;
	}

	public void GameStart(Room room, bool isHost)
	{
		currentRoom = room;
		gameBoard.isHost = isHost;
		gameBoard.gameObject.SetActive(true);
	}

	public void ProccessTurn(Turn turn)
	{
		//새로운 턴 입력이 추가될 때마다 호출.
		gameBoard.turnCount++;

		gameBoard.PlaceMark(turn.isHostTurn, turn.coodinate);

	}

	private void InviteButtonClick()
	{
		var popup = UIManager.Instance.PopUpOpen<UIInputFieldPopUp>();
		popup.SetPopUP("초대하기", "누구를 초대하시겠습니까?", InviteTarget);
	}

	private async void InviteTarget(string target)
	{
		Room room = new Room()
		{
			host = FirebaseManager.Instance.Auth.CurrentUser.UserId,
			guest = target,
			state = RoomState.Waiting
		};
		await FirebaseManager.Instance.CreateRoom(room);

		Message message = new Message()
		{
			type = MessageType.Invite,
			sender = FirebaseManager.Instance.Auth.CurrentUser.UserId,
			displayName = FirebaseManager.Instance.Auth.CurrentUser.DisplayName,
			message = "",
			sendTime = DateTime.Now.Ticks
		};

		await FirebaseManager.Instance.MessageToTarget(target, message);
	}

	private void MessageButtonClick()
	{
		var popup = UIManager.Instance.PopUpOpen<UIInputFieldPopUp>();
		popup.SetPopUP("메세지 보내기", "누구에게 메세지를 보내겠습니까?", SetMessageTarget);
	}

	private void SetMessageTarget(string target)
	{
		messageTarget = target;
		var popup = UIManager.Instance.PopUpOpen<UIInputFieldPopUp>();
		popup.SetPopUP($"To.{messageTarget}", "보낼 메세지를 입력해주세요.", MessageToTarget);
	}

	private async void MessageToTarget(string messageText)
	{
		Message message = new Message()
		{
			type = MessageType.Message,
			sender = FirebaseManager.Instance.Auth.CurrentUser.UserId,
			message = messageText,
			sendTime = DateTime.Now.Ticks
		};
		await FirebaseManager.Instance.MessageToTarget(messageTarget, message);
	}

	private void RankButtonClick()
	{
		UIManager.Instance.PageOpen<UIRank>();
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
