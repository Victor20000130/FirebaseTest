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
		//ã�� �˾��� ������
		if (@return != null)
		{   //�˾��� Ȱ��ȭ�Ѵ�.
			@return.gameObject.SetActive(true);
			//�˾��� ���ÿ� �߰��Ѵ�.
			openPopups.Push(@return);
		}
		return @return;
	}

	public void PopUpClose()
	{   //�˾� ���ÿ� �˾��� �� ������
		if (openPopups.Count > 0)
		{   //�˾��� ������.
			UIPopUp targetPopup = openPopups.Pop();
			//�˾��� ��Ȱ��ȭ ��Ų��.
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
