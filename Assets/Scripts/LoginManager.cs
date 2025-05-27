using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoginManager : MonoBehaviour
{
	public static LoginManager lom;
	[SerializeField] public GameObject Keyboard;
	[Header("Login Field")]
	[SerializeField] GameObject Login;
	[SerializeField] TMP_Text LoginID, LoginPW;
	[SerializeField] UnityEvent SuccessEvent;

	[Header("Sign Field")]
	[SerializeField] GameObject Sign;
	[SerializeField] TMP_Text SignID, SignPW, SignName,SignHeight,SignWeight;

	Users UserDatas;
	private void Awake()
	{
		lom = this;
		string path = Path.Combine(Application.persistentDataPath,"User.json");
		if (File.Exists(path)) UserDatas = JsonConvert.DeserializeObject<Users>(File.ReadAllText(path));
		else UserDatas = new Users();
		gameObject.SetActive(false);
	}


	private void OnDisable()
	{
		Login.SetActive(true);
		Sign.SetActive(false); SignID.text = ""; SignPW.text = ""; SignName.text = ""; SignHeight.text = ""; SignWeight.text = "";
		/*string json = JsonConvert.SerializeObject(UserDatas);
		File.WriteAllText(Path.Combine(Application.persistentDataPath, "User.json"),json);*/
	}

	public void LoginAct()
	{
		foreach (var j in UserDatas.User) if (j.id.Equals(LoginID.text) && j.pw.Equals(LoginPW.text)) 
			{
				if (SuccessEvent != null) SuccessEvent.Invoke();
				gameObject.SetActive(false);
				return;
			}
		ShowError("ID or PW 불일치");
	}



	[SerializeField] List<Image> GenderImages;
	[SerializeField] List<TMP_Text> GenderTexts;
	Color GenderColor = new Color(0.1725f, 0.1725f, 0.1725f), GenderColor2 = new Color(0.3f,0.3f,0.3f);
	bool CurMan;
	public void GenderToggle(bool IsMan)
	{
		CurMan = IsMan;
		if (IsMan)
		{
			GenderImages[0].color = GenderColor2; GenderImages[1].color = Color.gray; GenderTexts[0].color = Color.gray;
			GenderImages[2].color = GenderColor; GenderImages[3].color = Color.white; GenderTexts[1].color = Color.white;
		}
		else
		{
            GenderImages[2].color = GenderColor2; GenderImages[3].color = Color.gray; GenderTexts[1].color = Color.gray;
            GenderImages[0].color = GenderColor; GenderImages[1].color = Color.white; GenderTexts[0].color = Color.white;
        }
	}

	public void SignUp()
	{
		if (SignID.text.Equals("") || SignPW.text.Equals("") || SignName.text.Equals("")) { ShowError("필수 항목 누락"); return; }
		foreach(var j in UserDatas.User)
		{
			if (j.id.Equals(SignID.text)) { ShowError("ID 중복"); return; }
		}
        UserDatas.User.Add(new UserData(SignID.text, SignPW.text, SignName.text, 0, 0, 0,CurMan));
		Login.SetActive(true); Sign.SetActive(false);
		ShowError("가입 성공");
    }
	[SerializeField] GameObject ErrorObj;
	[SerializeField] TMP_Text ErrorMessage;
	public void ShowError(string text)
	{
		ErrorObj.SetActive(true); ErrorMessage.text = text;
	}
}

