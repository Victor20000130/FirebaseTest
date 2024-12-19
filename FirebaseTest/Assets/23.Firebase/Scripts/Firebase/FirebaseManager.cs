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

public class FirebaseManager : MonoBehaviour
{
	public static FirebaseManager Instance { get; private set; }

	public FirebaseApp App { get; private set; }
	public FirebaseAuth Auth { get; private set; }
	public FirebaseDatabase DB { get; private set; }

	private DatabaseReference usersRef;

	public UserData currentUserData { get; private set; }


	private void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(gameObject);
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
	}
}


