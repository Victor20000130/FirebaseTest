using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }

	public List<UIPage> uiPages;
	public List<UIPopUp> popups;

	private Stack<UIPopUp> openPopups = new Stack<UIPopUp>();

	private void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}
	private void Start()
	{
		_ = PageOpen("UIMain");
		foreach (UIPopUp popup in popups)
		{
			popup.gameObject.SetActive(false);
		}
	}

	public T PopUpOpen<T>() where T : UIPopUp
	{
		T @return = popups.Find((popup) => popup is T) as T;
		//찾는 팝업이 있으면
		if (@return != null)
		{   //팝업을 활성화한다.
			@return.gameObject.SetActive(true);
			//팝업을 스택에 추가한다.
			openPopups.Push(@return);
		}
		return @return;
	}

	public void PopUpClose()
	{   //팝업 스택에 팝업이 들어가 있으면
		if (openPopups.Count > 0)
		{   //팝업을 꺼낸다.
			UIPopUp targetPopup = openPopups.Pop();
			//팝업을 비활성화 시킨다.
			targetPopup.gameObject.SetActive(false);
		}
	}

	public T PageOpen<T>() where T : UIPage
	{
		T @return = null;
		foreach (MonoBehaviour uiPage in uiPages)
		{
			bool isActive = uiPage is T;
			uiPage.gameObject.SetActive(isActive);
			if (isActive) @return = uiPage as T;
		}
		return @return;
	}

	public UIPage PageOpen(string pageName)
	{
		UIPage @return = null;
		foreach (UIPage uiPage in uiPages)
		{
			bool isActive = uiPage.GetType().Name.Equals(pageName);
			uiPage.gameObject.SetActive(isActive);
			if (isActive) @return = uiPage;
		}
		return @return;
	}

}
