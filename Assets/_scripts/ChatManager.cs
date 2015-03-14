using UnityEngine;
using System.Collections;

public class ChatManager : MonoBehaviour {
	
	public ArrayList log = new ArrayList();
	int maxLogMessages = 100;
	bool visible = true;
	public string curMessage = "";
	public GUISkin guiSkin;
	
	Vector2 scrollPos = new Vector2(0, 0);
	float maxLogHeight = 100.0f;
	float logWidth = 300.0f;
	int lastLogLen = 0;
	
	// Use this for initialization
	void Start() {
		log.Add("Duct Tape Destruction v. 0.0.0.0.0.0.3");
	}
	
	// Update is called once per frame
	void Update() {

	}
	
	void OnGUI() {
		if(guiSkin != null) {
			GUI.skin = guiSkin;
		}
		GUI.skin.textField.fontSize = 16;
		GUI.skin.label.fontSize = 16;

		if(Input.GetKeyDown(KeyCode.T)) {
			GUI.FocusControl("chatWindow");
			return;	
		}

		Event e = Event.current;
		if(e.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl().Equals("chatWindow")) {
			if(curMessage.Length != 0) {
				SendMessage();
				return;
			}
			GUI.FocusControl("");
		}
		if(!visible)
			return;
		GUI.SetNextControlName("chatWindow");
		curMessage = GUI.TextField(new Rect(0f, Screen.height - 50, logWidth, 26), curMessage, 50);
		float[] logBoxHeights = new float[log.Count];
		float totalHeight = 0f;
		
		int i = 0;
		float logBoxHeight = 0f;
		foreach(string s in log) {
			logBoxHeight = Mathf.Min(maxLogHeight, 26f);
			logBoxHeights[i++] = logBoxHeight;
			totalHeight += logBoxHeight;
		}
		
		float innerScrollHeight = totalHeight;
		if(lastLogLen != log.Count) {
			scrollPos = new Vector2(0f, innerScrollHeight);
			lastLogLen = log.Count;
		}
		
		scrollPos = GUI.BeginScrollView(new Rect(0f, Screen.height - 150f - 50f, logWidth + 8, 150), scrollPos, new Rect(0f, 0f, logWidth, innerScrollHeight));
		float curY = 0f;
		
		i = 0;
		foreach(string s in log) {
			logBoxHeight = logBoxHeights[i++];
			GUI.Label(new Rect(10, curY, logWidth, logBoxHeight), s);
			curY += logBoxHeight;
		}
		GUI.EndScrollView();

		GUI.SetNextControlName("");
		GUI.Box(new Rect(-10, -10, 0, 0), "");
	}

	public void AddMessage(string s) {
		curMessage = s;
		SendMessage();
	}

	void SendMessage() {
		log.Add(curMessage);
		PhotonView.Get(this).RPC("ReceiveMessage", PhotonTargets.Others, curMessage);
		curMessage = "";
		if(log.Count > maxLogMessages)
			log.Remove(0);
	}
	
	[RPC]
	void ReceiveMessage(string s) {
		log.Add(s);
		if(log.Count > maxLogMessages)
			log.Remove(0);
	}
}