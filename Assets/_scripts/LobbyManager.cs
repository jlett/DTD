using UnityEngine;
using System.Collections;

public class LobbyManager : MonoBehaviour {

	public GUISkin skin;
	bool joinGuiOn = false;

	void Start() {
		if(!PhotonNetwork.connected)
			PhotonNetwork.ConnectUsingSettings("0.0.1");
	}
	
	void OnGUI() {
		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
		if(skin != null) {
			GUI.skin = skin;
		}
		if(joinGuiOn) {
			Rect r = new Rect(0, 0, 600, 400);
			r.center = new Vector2(Screen.width/2, Screen.height/2);
			GUI.Window(0, r, DoLobbyWindow, "Lobby");
			GUI.FocusWindow(0);
		}
	}

	string roomName = "Room Name";
	string playerName = "Player Name";
	void DoLobbyWindow(int windowID) {
		//need room list, create room button (with text feild for name)
		if(GUI.Button(new Rect(20, 50, 160, 50), "Join Random Game")) {
			PhotonNetwork.JoinRandomRoom();
		}
		GUI.Label(new Rect(20, 120, 160, 50), "Public Rooms:");
		RoomInfo[] rooms = PhotonNetwork.GetRoomList();
		for(int i = 0; i < rooms.Length; i++) {
			if(GUI.Button(new Rect(20, 170 + 55*i, 160, 50), rooms[i].name + " " + rooms[i].playerCount + "/" + rooms[i].maxPlayers, skin.GetStyle("Box"))) {
				PhotonNetwork.JoinRoom(rooms[i].name);
			}
		}

		roomName = GUI.TextArea(new Rect(220, 110, 160, 40), roomName, 100);
		playerName = GUI.TextArea(new Rect(220, 160, 160, 40), playerName, 100);
		if(GUI.Button(new Rect(220, 50, 160, 50), "Create Room")) {
			if(roomName != "") {
				PhotonNetwork.CreateRoom(roomName, true, true, 4);
			}
		}
	}

	void OnLeftLobby() {
		joinGuiOn = false;
		for(int i = 0; i < PhotonNetwork.otherPlayers.Length; i++) {
			if(playerName.Equals(PhotonNetwork.otherPlayers[i].name)) {
				playerName += " " + (PhotonNetwork.countOfPlayers + 1);
			}
		}
		PhotonNetwork.playerName = playerName;
		Debug.Log("you are " + PhotonNetwork.playerName);
	}

	void OnJoinedLobby() {
		joinGuiOn = true;
	}

	void OnPhotonRandomJoinFailed() {
		Debug.Log("no games to join!");

	}

	void OnPhotonPlayerConnected(PhotonPlayer player) {
		Debug.Log(player.name + " joined!");
	}

	void OnJoinedRoom() {
		//need to get rid of gui, switch scene, if masterclient initialize room stuff, spawn player
		PhotonNetwork.LoadLevel("TestWorld");
		//PhotonNetwork.LoadLevel("ForestScene");
		//GetComponent<LoadingScreen>().DisplayLoadingScreen("ForestScene");
	}

	void OnLevelWasLoaded(int index) {
		if(index != 0) {//not menu
			SpawnPlayer();
		}
		//init other players

	}

	private void SpawnPlayer() {
		Instantiate(Resources.Load("_prefabs/camera"));
		PhotonNetwork.Instantiate("_prefabs/Player", new Vector3(0f, -.5f, 0f), Quaternion.identity, 0);
		JsonReader j = new JsonReader();
	}
}
