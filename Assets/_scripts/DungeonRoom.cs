using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonRoom {
	public int x;//does NOT correspond to world points, each 1 unit in dungeon gen is a dungeon.ROOMSPACING units in environment
	public int y;
	public int rot = 0;
	public int prefabIndex;
	private DungeonGenerator dungeon;

	private List<bool> exits = new List<bool>();
	public List<DungeonRoom> children = new List<DungeonRoom>();
	
	public DungeonRoom(int _x, int _y, int _rot, DungeonRoomData data = null) {
		x = _x;
		y = _y;
		rot = _rot;
		dungeon = DungeonGenerator.instance;
		dungeon.SIZE--;

		//pick random prefab
		if(data == null) {//aka this is root
			//int r = Random.Range(0, 8);
			int r = 0;
			GameObject o = Resources.Load("_prefabs/rooms/" + r) as GameObject;//move it to the right spot later
			data = o.GetComponent<DungeonRoomData>();
		}

		data.room = this;
		prefabIndex = data.prefabIndex;
		
		exits = data.exits;
		exits = RotateExits(exits, rot);
	}
	
	public void GenerateChildren() {
		GameObject[] roomPrefabs = Resources.LoadAll<GameObject>("_prefabs/rooms");
		DungeonRoomData[] rooms = new DungeonRoomData[roomPrefabs.Length];//data of possible rooms to select from
		for(int i = 0; i < roomPrefabs.Length; i++) {
			rooms[i] = roomPrefabs[i].GetComponent<DungeonRoomData>();
		}

		for(int i = 0; i < exits.Count; i++) {//generate one room for each exit
			if(exits[i] == false)
				continue;
			Dictionary<int, List<int>> posRooms = new Dictionary<int, List<int>>();//key is room id, value is available rotation

			Vector2 dir = new Vector2();
			if(i == 0) {
				dir = new Vector2(0, 1);
			}else if(i == 1) {
				dir = new Vector2(1, 0);
			}else if(i == 2) {
				dir = new Vector2(0, -1);
			}else if(i == 3) {
				dir = new Vector2(-1, 0);
			}
			if(dungeon.IsSpaceOccupied(new Vector2(x, y) + dir)) {
				continue;
			}

			for(int j = 0; j < rooms.Length; j++) {//for each room to choose from
				posRooms.Add(j, new List<int>());//all rooms are possible

				//only add room rotations that makes a door line up
				for(int k = 0; k < 4; k++) {
					if(RotateExits(rooms[j].exits, k)[(i+2)%4]) {//check door on opposite side for each rotation
						posRooms[j].Add(k);//if there is a door, add as a possibility
						//Debug.Log("adding room "  + j + " rotation " + k);
					}
				}
			}

			Dictionary<int, List<int>> roomsToRemove = new Dictionary<int, List<int>>();//make sure to only remove the rotation values and then key if no values left
			//remove rooms that would block existing doors
			//for every false side in data.exits, test if adjacent room has a door in corresponding spot
			//remove rooms that would create blocked doors
			//for every true side in data.exits, test if adjacent room has a wall in corresponding spot
			foreach(int room in posRooms.Keys) {
				foreach(int r in posRooms[room]) {
					for(int j = 0; j < 4; j++) {//for each side on the room
						Vector2 dir2 = new Vector2();
						if(j == 0) {
							dir2 = new Vector2(0, 1);
						}else if(j == 1) {
							dir2 = new Vector2(1, 0);
						}else if(j == 2) {
							dir2 = new Vector2(0, -1);
						}else if(j == 3) {
							dir2 = new Vector2(-1, 0);
						}

						if(dungeon.GetRoom(new Vector2(x, y) + dir + dir2) == null) {
							continue;
						}
						if(RotateExits(rooms[room].exits, r)[j] != dungeon.GetRoom(new Vector2(x, y) + dir + dir2).exits[(j+2)%4]) {
							if(!roomsToRemove.ContainsKey(room))
								roomsToRemove.Add(room, new List<int>());
							roomsToRemove[room].Add(r);
						}
					}
				}
			}


			foreach(int key in roomsToRemove.Keys) {
				foreach(int r in roomsToRemove[key]) {
					//Debug.Log("removing room "  + key + " rotation " + r);
					posRooms[key].Remove(r);
				}
				if(posRooms[key].Count == 0) {
					//Debug.Log("removing room "  + key);
					posRooms.Remove(key);
				}
			}
			roomsToRemove.Clear();

			if(dungeon.SIZE > 0) {//generate a regular room

				//if possible, dont use a "cap" (room with only one door)
				foreach(int r in posRooms.Keys) {
					int c = 0;
					foreach(bool b in rooms[r].exits) {
						if(b) {
							c++;
						}
					}
					if(c == 1) {//if only one exit, remove room
						roomsToRemove.Add(r, new List<int>());
					}
				}

				foreach(int k in roomsToRemove.Keys) {
					if(posRooms.Keys.Count > 1) {//dont remove if this is last possible room
						posRooms.Remove(k);
					}
				}
				roomsToRemove.Clear();

				//pick random from rooms left and place it
				//Debug.Log("out of " + posRooms.Keys.Count + " rooms...");
				List<int> keys = new List<int>(posRooms.Keys);
				int keyIndex = Random.Range(0, keys.Count);
				int id = keys[keyIndex];
				DungeonRoomData chosenOne = rooms[id];//random room
				int ro = Random.Range(0, posRooms[id].Count);

				Debug.Log("<color=green>adding room " + id + " rotated " + posRooms[id][ro] + " at " + new Vector2(x + dir.x, y + dir.y).ToString() + "</color>");
				children.Add(dungeon.AddRoom(x + (int)dir.x, y + (int)dir.y, posRooms[id][ro], chosenOne));
			
			} else {//generate a cap
				//if no boss room and sufficient distance from 0,0 place boss room
				if(!dungeon.bossRoomPlaced && Mathf.Abs(x) + Mathf.Abs(y) > dungeon.SIZE/5f) {
					//place boss room - no exits/entrances always one spawn point so can't conflict with surrounding rooms
					dungeon.bossRoomPlaced = true;
				} else {
				}

				//remove rooms with more than one door
				//need to consider situations where this would create a blocked door from another room
				//cap rooms would have been removed in that situation, so if no cap moves place a room that doesnt create doors leading to nowhere


				//pick random room and place it

			}
		}
	}

	public static List<bool> RotateExits(List<bool> e, int r) {
		if(r == 0) {
			return new List<bool>{e[0], e[1], e[2], e[3]};
		} else if(r == 1) {//90 deg
			return new List<bool>{e[3], e[0], e[1], e[2]};
		} else if(r == 2) {//180 deg
			return new List<bool>{e[2], e[3], e[0], e[1]};
		} else if(r == 3) {//270 deg
			return new List<bool>{e[1], e[2], e[3], e[0]};
		}
		Debug.LogError("rotate was given an impossible value");
		return e;
	}
}