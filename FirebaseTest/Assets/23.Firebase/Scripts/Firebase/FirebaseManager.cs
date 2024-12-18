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
		//파이어베이스 초기화 상태 체크. 비동기(Async) 함수이므로 완료될 때까지 대기.
		DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();
		if (status == DependencyStatus.Available)
		{   //초기화 성공
			App = FirebaseApp.DefaultInstance;
			Auth = FirebaseAuth.DefaultInstance;
			DB = FirebaseDatabase.DefaultInstance;
		}
		else
		{   //초기화 실패
			Debug.LogWarning($"파이어베이스 초기화 실패 : {status}");
		}
	}

	//회원가입 함수
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

	//로그인
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
