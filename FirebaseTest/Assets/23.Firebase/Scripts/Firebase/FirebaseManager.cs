using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;


public class FirebaseManager : MonoBehaviour
{
	public static FirebaseManager Instance { get; private set; }

	public FirebaseApp App { get; private set; }
	public FirebaseAuth Auth { get; private set; }
	public FirebaseDatabase DB { get; private set; }

	private DatabaseReference usersRef;

	private DatabaseReference msgRef;

	private DatabaseReference roomRef;

	public event Action onLogin;

	public event Action<Room, bool> onGameStart;

	public event Action<Turn> onTurnProcceed;

	private Room currentRoom;

	public UserData currentUserData { get; private set; }

	private bool isHost;

	private DatabaseReference turnRef;


	private void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(gameObject);
		onLogin += OnLogin;
	}

	private void OnLogin()
	{
		msgRef = DB.GetReference($"msg/{Auth.CurrentUser.UserId}");

		//�񵿱� ������ ����� ������ Firebase�� �˾Ƽ� ȣ�����ش�.
		msgRef.ChildAdded += OnMessageReceive;
	}

	//DatabaseReference.ChildAdded �̺�Ʈ�� ����� �̺�Ʈ �Լ�
	private void OnMessageReceive(object sender, ChildChangedEventArgs args)
	{   //������ ������
		if (args.DatabaseError == null)
		{
			string rawJson = args.Snapshot.GetRawJsonValue();

			Message message = JsonConvert.DeserializeObject<Message>(rawJson);
			//print(rawJson);

			if (message.isNew)
			{
				if (message.type == MessageType.Message)
				{
					var popup = UIManager.Instance.PopUpOpen<UIDialogPopUp>();

					popup.SetPopUp($"From.{message.sender}", $"{message.message}\n{message.GetSendTime()}");
				}
				else if (message.type == MessageType.Invite)
				{
					var popup = UIManager.Instance.PopUpOpen<UITwoButtonPopUp>();
					popup.SetPopUp("�ʴ���", $"{message.displayName}���� ���ӿ� �����Ͻðڽ��ϱ�?",
						ok => { if (ok) { JoinRoom(message.sender); } });
				}

				args.Snapshot.Reference.Child("isNew").SetValueAsync(false);
			}
		}
		else
		{   //������ �߻�
			Debug.LogWarning("missing args");
		}

	}

	public async Task CreateRoom(Room room)
	{
		currentRoom = room;

		isHost = true;

		roomRef = DB.GetReference($"rooms/{Auth.CurrentUser.UserId}");
		turnRef = DB.GetReference($"rooms/{Auth.CurrentUser.UserId}/turn");
		string json = JsonConvert.SerializeObject(room);

		await roomRef.SetRawJsonValueAsync(json);

		roomRef.Child("state").ValueChanged += OnRoomStateChange;
	}

	private void OnRoomStateChange(object sender, ValueChangedEventArgs e)
	{
		//GetValue(true)�� �ڷ����� ������ �ȿ��� �ڵ����� integer�� ��ȯ�� �ȴ�.
		object value = e.Snapshot.GetValue(true);

		int state = int.Parse(value.ToString());
		//value.Equals((int)RoomState.Playing)
		if (state == 1)
		{   //���� ��ŸƮ

			onGameStart?.Invoke(currentRoom, true);
			//������ ����� �� �̺�Ʈ ���� �����������.
			roomRef.Child("turn").ChildAdded += OnTurnAdded;
		}
		else if (state == 2)
		{   //������ ��


		}
	}

	private void OnTurnAdded(object sender, ChildChangedEventArgs e)
	{
		string json = e.Snapshot.GetRawJsonValue();
		Turn turn = JsonConvert.DeserializeObject<Turn>(json);

		onTurnProcceed?.Invoke(turn);

	}

	public void SendTurn(int turnCount, Turn turn)
	{
		turn.isHostTurn = isHost;

		string json = JsonConvert.SerializeObject(turn);
		roomRef.Child($"turn/{turnCount}").SetRawJsonValueAsync(json);
	}

	public async Task<bool> GetTurn()
	{
		DataSnapshot turnSnapshot = await turnRef.GetValueAsync();

		if (turnSnapshot.Exists)
		{
			string turnJson = turnSnapshot.Children.LastOrDefault().GetRawJsonValue();
			Turn turn = JsonConvert.DeserializeObject<Turn>(turnJson);
			if (turn != null)
			{
				return turn.isHostTurn != isHost;
			}
		}
		return isHost;
	}

	private async void JoinRoom(string host)
	{
		roomRef = DB.GetReference($"rooms/{host}");
		turnRef = DB.GetReference($"rooms/{host}/turn");

		DataSnapshot roomSnapshot = await roomRef.GetValueAsync();

		string roomJson = roomSnapshot.GetRawJsonValue();

		Room room = JsonConvert.DeserializeObject<Room>(roomJson);

		currentRoom = room;

		isHost = false;

		await roomRef.Child("state").SetValueAsync((int)RoomState.Playing);

		onGameStart?.Invoke(room, false);
		roomRef.Child("turn").ChildAdded += OnTurnAdded;
	}

	public async Task MessageToTarget(string target, Message message)
	{
		DatabaseReference targetRef = DB.GetReference($"msg/{target}");
		string messageJson = JsonConvert.SerializeObject(message);
		await targetRef.Child(message.displayName + message.sendTime).SetRawJsonValueAsync(messageJson);
	}

	private async void Start()
	{
		//���̾�̽� �ʱ�ȭ ���� üũ. �񵿱�(Async) �Լ��̹Ƿ� �Ϸ�� ������ ���.
		DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();
		if (status == DependencyStatus.Available)
		{   //�ʱ�ȭ ����
			App = FirebaseApp.DefaultInstance;
			Auth = FirebaseAuth.DefaultInstance;
			DB = FirebaseDatabase.DefaultInstance;

			//DataSnapshot�� �Ἥ DB�� �ִ� user�� �ڽ��� dummy�� ���� ������.
			DataSnapshot dummyData = await DB.GetReference("users").Child("dummy").GetValueAsync();
			//���̵����� �ȿ� ���� �ִٸ�
			if (dummyData.Exists)
			{   //dummy�ȿ� �ִ� ���� Json���� ��ȯ�ϰ� �װ��� string Object�� ��ȯ�Ͽ� ����Ʈ��.
				print(dummyData.GetRawJsonValue());
			}
		}
		else
		{   //�ʱ�ȭ ����
			Debug.LogWarning($"���̾�̽� �ʱ�ȭ ���� : {status}");
		}
	}

	//ȸ������ �Լ�
	public async void Create(string email, string passwd, Action<FirebaseUser, UserData> callBack = null)
	{
		try
		{
			var result = await Auth.CreateUserWithEmailAndPasswordAsync(email, passwd);

			//������ ȸ���� database reference�� ����
			usersRef = DB.GetReference($"users/{result.User.UserId}");
			//ȸ���� �����͸� database�� ����
			UserData userData = new UserData(result.User.UserId);

			string userDataJson = JsonConvert.SerializeObject(userData);

			await usersRef.SetRawJsonValueAsync(userDataJson);

			callBack?.Invoke(result.User, userData);
		}
		catch (FirebaseException e)
		{
			UIManager.Instance.PopUpOpen<UIDialogPopUp>().SetPopUp("ȸ������ ����", "�ùٸ� �̸��� ���� �Ǵ� 6�� �̻��� ��й�ȣ�� �Է����ּ���.");
			Debug.LogWarning(e.Message);
		}
	}

	//�α���
	public async void SignIn(string email, string passwd, Action<FirebaseUser, UserData> callBack = null)
	{
		try
		{
			var result = await Auth.SignInWithEmailAndPasswordAsync(email, passwd);

			usersRef = DB.GetReference($"users/{result.User.UserId}");

			DataSnapshot userDataValues = await usersRef.GetValueAsync();

			UserData userData = null;

			//DB�� ��ΰ� �����ϴ��� �˻�
			if (userDataValues.Exists)
			{
				string json = userDataValues.GetRawJsonValue();

				userData = JsonConvert.DeserializeObject<UserData>(json);
			}

			currentUserData = userData;

			callBack?.Invoke(result.User, currentUserData);

			onLogin?.Invoke();

		}
		catch (FirebaseException e)
		{
			UIManager.Instance.PopUpOpen<UIDialogPopUp>().SetPopUp("�α��� ����", "�̸��� �Ǵ� ��й�ȣ�� Ʋ�Ƚ��ϴ�.");
			Debug.LogWarning(e.Message);

		}
	}

	//���� ���� ����
	public async void UpdateUserProfile(string displayName, Action<FirebaseUser> callback = null)
	{   //UserProfile ����.
		UserProfile profile =
			new UserProfile()
			{
				DisplayName = displayName,
				PhotoUrl = new Uri("https://picsum.photos/120"),
			};
		await Auth.CurrentUser.UpdateUserProfileAsync(profile);
		//currentUserData.userName = displayName;

		UpdateUserData("userName", displayName);

		DataSnapshot userDataValues = await usersRef.GetValueAsync();
		UserData userData = null;
		if (userDataValues.Exists)
		{
			string json = userDataValues.GetRawJsonValue();
			userData = JsonConvert.DeserializeObject<UserData>(json);
		}
		currentUserData = userData;
		callback?.Invoke(Auth.CurrentUser);
	}




	//database�� ���� ������ ����
	public async void UpdateUserData(string childName, object value, Action<object> callback = null)
	{
		DatabaseReference targetRef = usersRef.Child(childName);
		await targetRef.SetValueAsync(value);
		await usersRef.GetValueAsync();

		callback?.Invoke(value);
	}

	internal void SignOut()
	{
		Auth.SignOut();
		msgRef.ChildAdded -= OnMessageReceive;
	}
}


