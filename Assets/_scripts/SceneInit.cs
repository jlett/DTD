using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class SceneInit : MonoBehaviour {

	//Class Description:
	//searches for game objects tagged as spawn points
	//for each of these, spawns a corresponding object over network based on name and destroys the empty (so that it will not be spawned by new clients)

	GameObject[] objects;
	void Start () {
		if(PhotonNetwork.isMasterClient) {
			objects = GameObject.FindGameObjectsWithTag("SpawnPoint");
			foreach(GameObject o in objects) {
				if(Random.value <= o.GetComponent<SpawnPoint>().spawnChance) {
					JSONNode N = JSON.Parse(o.GetComponent<SpawnPoint>().data);
					string name = N["name"];
					name = name.ToLower();
					Transform t = o.transform;
					object[] data = new object[10];//10 is an arbitrary limit that can be extended if needed


					if(name.Equals("chest")) {
						if(N["size"] != null || N["level"] != null) {
							data[0] = "level-size";
							data[1] = N["level"] != null ? N["level"].AsInt : 1;
							data[2] = N["size"] != null ? N["size"].AsInt : 1;
						} else {
							data[0] = "itemlist";
							data[1] = Item.SerializeObject<Item>(new Item("SUPER AWESOME ITEM", Rarity.epic, "test item"));
						}
						PhotonNetwork.InstantiateSceneObject("_prefabs/Chest", t.position, t.rotation, 0, data);
					}


					else if(name.Equals("goblin")) {
						data[0] = N["damage"] != null ? N["damage"].AsInt : 0;
						PhotonNetwork.InstantiateSceneObject("_prefabs/AI/Goblin", t.position, t.rotation, 0, data);
					}
				}
				Destroy(o);
			}
		}
		Destroy(GameObject.Find("Spawnpoints"));
	}
}
