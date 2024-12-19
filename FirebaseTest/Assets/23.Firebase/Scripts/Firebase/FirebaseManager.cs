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
	}
}


