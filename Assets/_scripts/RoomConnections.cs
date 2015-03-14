using UnityEngine;
using System.Collections;

public class RoomConnections : MonoBehaviour {

	DungeonRoom room;
	private float offset = 5f;
	private int rot;

	void Start() {
		room = transform.parent.GetComponent<DungeonRoomData>().room;
	}

	void OnTriggerEnter(Collider other) {
		rot = room.rot;

		int exitSide;
		if(name.Equals("exitN")) {
			exitSide = 0 + rot;
		} else if(name.Equals("exitE")) {
			exitSide = 1 + rot;
		} else if(name.Equals("exitS")) {
			exitSide = 2 + rot;
		} else if(name.Equals("exitW")) {
			exitSide = 3 + rot;
		} else {
			Debug.LogError("EXIT IMPROPERLY NAMED OR DNE OR SOMETHING");
			exitSide = -1;
		}
		exitSide = exitSide%4;

		if(other.GetComponent<PlayerStats>() == null)
			return;
		if(exitSide == 0) {
			GameObject dest = DungeonGenerator.instance.roomObjects[new Vector2(room.x, room.y + 1)];
			RoomConnections[] connections = dest.GetComponentsInChildren<RoomConnections>();

			int destRot = dest.GetComponent<DungeonRoomData>().room.rot;
			int destSide;
			string destName = "";
			destSide = (6 - destRot)%4;
			if(destSide == 0) {
				destName = "exitN";
			} else if(destSide == 1) {
				destName = "exitE";
			} else if(destSide == 2) {
				destName = "exitS";
			} else if(destSide == 3) {
				destName = "exitW";
			}
			Debug.Log(destName);

			foreach(RoomConnections c in connections) {
				if(c.name.Equals(destName)) {
					other.GetComponent<Rigidbody>().position = c.gameObject.transform.position + new Vector3(0, 0, offset);
				}
			}
			dest.GetComponent<DungeonRoomData>().doorsOpen = false;
		} else if(exitSide == 1) {
			GameObject dest = DungeonGenerator.instance.roomObjects[new Vector2(room.x + 1, room.y)];
			RoomConnections[] connections = dest.GetComponentsInChildren<RoomConnections>();
			
			int destRot = dest.GetComponent<DungeonRoomData>().room.rot;
			int destSide;
			string destName = "";
			destSide = (7 - destRot)%4;
			if(destSide == 0) {
				destName = "exitN";
			} else if(destSide == 1) {
				destName = "exitE";
			} else if(destSide == 2) {
				destName = "exitS";
			} else if(destSide == 3) {
				destName = "exitW";
			}
			Debug.Log(destName);
			
			foreach(RoomConnections c in connections) {
				if(c.name.Equals(destName)) {
					other.transform.position = c.gameObject.transform.position + new Vector3(offset, 0, 0);
				}
			}
			dest.GetComponent<DungeonRoomData>().doorsOpen = false;
		} else if(exitSide == 2) {
			GameObject dest = DungeonGenerator.instance.roomObjects[new Vector2(room.x, room.y - 1)];
			RoomConnections[] connections = dest.GetComponentsInChildren<RoomConnections>();
			
			int destRot = dest.GetComponent<DungeonRoomData>().room.rot;
			int destSide;
			string destName = "";
			destSide = (4 - destRot)%4;
			if(destSide == 0) {
				destName = "exitN";
			} else if(destSide == 1) {
				destName = "exitE";
			} else if(destSide == 2) {
				destName = "exitS";
			} else if(destSide == 3) {
				destName = "exitW";
			}
			Debug.Log(destName);
			
			foreach(RoomConnections c in connections) {
				if(c.name.Equals(destName)) {
					other.transform.position = c.gameObject.transform.position + new Vector3(0, 0, -offset);
				}
			}
			dest.GetComponent<DungeonRoomData>().doorsOpen = false;
		} else if(exitSide == 3) {
			GameObject dest = DungeonGenerator.instance.roomObjects[new Vector2(room.x - 1, room.y)];
			RoomConnections[] connections = dest.GetComponentsInChildren<RoomConnections>();
			
			int destRot = dest.GetComponent<DungeonRoomData>().room.rot;
			int destSide;
			string destName = "";
			destSide = (5 - destRot)%4;
			if(destSide == 0) {
				destName = "exitN";
			} else if(destSide == 1) {
				destName = "exitE";
			} else if(destSide == 2) {
				destName = "exitS";
			} else if(destSide == 3) {
				destName = "exitW";
			}
			Debug.Log(destName);
			
			foreach(RoomConnections c in connections) {
				if(c.name.Equals(destName)) {
					other.transform.position = c.gameObject.transform.position + new Vector3(-offset, 0, 0);
				}
			}
			dest.GetComponent<DungeonRoomData>().doorsOpen = false;
		}
	}

	void Update() {
		if(transform.parent.GetComponent<DungeonRoomData>().doorsOpen) {
			GetComponent<BoxCollider>().isTrigger = true;
		} else {
			GetComponent<BoxCollider>().isTrigger = false;
		}
	}
}
