using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonRoomData : MonoBehaviour {
	public int prefabIndex;
	public List<bool> exits = new List<bool>(4);
	public DungeonRoom room;
	public bool doorsOpen = false;

	void Update() {
		if(Input.GetKeyDown(KeyCode.O)) {
			doorsOpen = !doorsOpen;
		}
	}
}
