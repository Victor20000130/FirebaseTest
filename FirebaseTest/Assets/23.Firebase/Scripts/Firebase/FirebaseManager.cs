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

public class FirebaseManager : MonoBehaviour
{
	public static FirebaseManager Instance { get; private set; }

	public FirebaseApp App { get; private set; }
	public FirebaseAuth Auth { get; private set; }
	public FirebaseDatabase DB { get; private set; }


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
		}
		else
		{   //�ʱ�ȭ ����
			Debug.LogWarning($"���̾�̽� �ʱ�ȭ ���� : {status}");
		}
	}

	//ȸ������ �Լ�
	public async void Create(string email, string passwd, Action<FirebaseUser> callBack = null)
	{
		try
		{
			var result = await Auth.CreateUserWithEmailAndPasswordAsync(email, passwd);

			callBack?.Invoke(result.User);
		}
		catch (FirebaseException e)
		{
			Debug.LogError(e.Message);
		}
	}

	//�α���
	public async void SignIn(string email, string passwd, Action<FirebaseUser> callBack = null)
	{
		try
		{
			var result = await Auth.SignInWithEmailAndPasswordAsync(email, passwd);

			callBack?.Invoke(result.User);
		}
		catch (FirebaseException e)
		{
			Debug.LogError(e.Message);
		}
	}

}
