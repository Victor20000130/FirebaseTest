using Firebase;
using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseTest : MonoBehaviour
{
	private async void Start()
	{
		DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();
		print(status);
		try
		{
			var result =
				await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync("abc@abc.abc", "abcabd");
			print(result.User.UserId);
		}
		catch (ApplicationException ae)
		{
			print(ae.Message);
		}
		catch (Exception e)
		{
			print(e.Message);

		}
		finally
		{
			print("¹º°¡ Àß¸øµÆ´Âµ¥?");
		}
	}
}
