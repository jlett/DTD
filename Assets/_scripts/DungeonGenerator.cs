using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonGenerator : MonoSingleton<DungeonGenerator> {
	
	public int SIZE = 10;//how many rooms to add before starting to add room caps
	public int ROOMSPACING = 1000;//how far apart to put the rooms
	public Dictionary<Vector3, DungeonRoom> rooms = new Dictionary<Vector3, DungeonRoom>();//list of all rooms, x is x, y is y, z is rotation from 0 to 3 (multiply by 90 deg)
	public bool bossRoomPlaced = false;
	public Dictionary<Vector2, GameObject> roomObjects = new Dictionary<Vector2, GameObject>();

	public override void Init() {
		if(!PhotonNetwork.isMasterClient)
			return;
		GenerateDungeon();
		GenerateRooms();
	}
	
	public void GenerateDungeon() {
		DungeonRoom root = AddRoom(0, 0, 0, null);
		BFChildGen(root);
	}

	void BFChildGen(DungeonRoom r) {
		Queue<DungeonRoom> q = new Queue<DungeonRoom>();
		q.Enqueue(r);
		while(q.Count > 0) {
			DungeonRoom d = q.Dequeue();
			d.GenerateChildren();
			foreach(DungeonRoom c in d.children) {
				q.Enqueue(c);
			}
		}
	}
	
	public void GenerateRooms() {//make actual rooms in world from the data in rooms
		foreach(Vector3 key in rooms.Keys) {
			DungeonRoom room = rooms[key];
			GameObject g = PhotonNetwork.Instantiate("_prefabs/rooms/" + room.prefabIndex, new Vector3(room.x*ROOMSPACING, 0, room.y*ROOMSPACING), Quaternion.Euler(0, 90*room.rot, 0), 0) as GameObject;
			g.GetComponent<DungeonRoomData>().room = room;
			roomObjects.Add(new Vector2(room.x, room.y), g);
		}
	}
	
	public DungeonRoom AddRoom(int x, int y, int rot, DungeonRoomData data) {
		DungeonRoom r = new DungeonRoom(x, y, rot, data);
		rooms.Add(new Vector3(x, y, rot), r);
		return r;
	}

	public bool IsSpaceOccupied(Vector2 pos) {
		for(int i = 0; i < 4; i++) {
			if(rooms.ContainsKey(new Vector3(pos.x, pos.y, i))) {
				return true;
			}
		}
		return false;
	}

	public DungeonRoom GetRoom(Vector2 pos) {
		for(int i = 0; i < 4; i++) {
			if(rooms.ContainsKey(new Vector3(pos.x, pos.y, i))) {
				return rooms[new Vector3(pos.x, pos.y, i)];
			}
		}
		return null;
	}
}
