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

		//비동기 식으로 변경될 때마다 Firebase가 알아서 호출해준다.
		msgRef.ChildAdded += OnMessageReceive;
	}

	//DatabaseReference.ChildAdded 이벤트에 등록할 이벤트 함수
	private void OnMessageReceive(object sender, ChildChangedEventArgs args)
	{   //에러가 없으면
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
					popup.SetPopUp("초대장", $"{message.displayName}님의 게임에 참가하시겠습니까?",
						ok => { if (ok) { JoinRoom(message.sender); } });
				}

				args.Snapshot.Reference.Child("isNew").SetValueAsync(false);
			}
		}
		else
		{   //에러가 발생
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
		//GetValue(true)는 자료형이 스냅샷 안에서 자동으로 integer로 변환이 된다.
		object value = e.Snapshot.GetValue(true);

		int state = int.Parse(value.ToString());
		//value.Equals((int)RoomState.Playing)
		if (state == 1)
		{   //게임 스타트

			onGameStart?.Invoke(currentRoom, true);
			//게임이 종료될 시 이벤트 구독 해제해줘야함.
			roomRef.Child("turn").ChildAdded += OnTurnAdded;
		}
		else if (state == 2)
		{   //게임이 끝


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
		//파이어베이스 초기화 상태 체크. 비동기(Async) 함수이므로 완료될 때까지 대기.
		DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();
		if (status == DependencyStatus.Available)
		{   //초기화 성공
			App = FirebaseApp.DefaultInstance;
			Auth = FirebaseAuth.DefaultInstance;
			DB = FirebaseDatabase.DefaultInstance;

			//DataSnapshot을 써서 DB에 있는 user의 자식인 dummy의 값을 가져옴.
			DataSnapshot dummyData = await DB.GetReference("users").Child("dummy").GetValueAsync();
			//더미데이터 안에 값이 있다면
			if (dummyData.Exists)
			{   //dummy안에 있는 값을 Json으로 변환하고 그것을 string Object로 반환하여 프린트함.
				print(dummyData.GetRawJsonValue());
			}
		}
		else
		{   //초기화 실패
			Debug.LogWarning($"파이어베이스 초기화 실패 : {status}");
		}
	}

	//회원가입 함수
	public async void Create(string email, string passwd, Action<FirebaseUser, UserData> callBack = null)
	{
		try
		{
			var result = await Auth.CreateUserWithEmailAndPasswordAsync(email, passwd);

			//생성된 회원의 database reference를 설정
			usersRef = DB.GetReference($"users/{result.User.UserId}");
			//회원의 데이터를 database에 생성
			UserData userData = new UserData(result.User.UserId);

			string userDataJson = JsonConvert.SerializeObject(userData);

			await usersRef.SetRawJsonValueAsync(userDataJson);

			callBack?.Invoke(result.User, userData);
		}
		catch (FirebaseException e)
		{
			UIManager.Instance.PopUpOpen<UIDialogPopUp>().SetPopUp("회원가입 실패", "올바른 이메일 형식 또는 6자 이상의 비밀번호를 입력해주세요.");
			Debug.LogWarning(e.Message);
		}
	}

	//로그인
	public async void SignIn(string email, string passwd, Action<FirebaseUser, UserData> callBack = null)
	{
		try
		{
			var result = await Auth.SignInWithEmailAndPasswordAsync(email, passwd);

			usersRef = DB.GetReference($"users/{result.User.UserId}");

			DataSnapshot userDataValues = await usersRef.GetValueAsync();

			UserData userData = null;

			//DB에 경로가 존재하는지 검사
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
			UIManager.Instance.PopUpOpen<UIDialogPopUp>().SetPopUp("로그인 실패", "이메일 또는 비밀번호가 틀렸습니다.");
			Debug.LogWarning(e.Message);

		}
	}

	//유저 정보 수정
	public async void UpdateUserProfile(string displayName, Action<FirebaseUser> callback = null)
	{   //UserProfile 생성.
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




	//database의 유저 데이터 수정
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


