using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using RAIN.Core;

public class PrefabSpawner : MonoBehaviour {

	public GameObject prefab;
	public List<Item> items;
	public Transform position;
	public Vector3 offset;
	public Vector3 rotation;

	bool spawning = false;
	public float spawnInt = 0f;

	void Start() {
		if(position == null) 
			position = transform;
	}

	void Update() {	
		if(Input.GetKeyDown(KeyCode.P)) {
			if(spawnInt <= 0)
				Spawn();
			else {
				spawning = !spawning;
				StartCoroutine(SpawnOnInt());
			}
		}
	}

	IEnumerator SpawnOnInt() {
		while(spawning) {
			Spawn();
			yield return new WaitForSeconds(spawnInt);
		}
	}

	public void Spawn() {
		if(prefab != null) {
			if(prefab.name.Equals("Chest") && items.Count > 0) {
				for(int i = 0; i < items.Count; i++)
					PhotonView.Get(this).RPC("MakeChest", PhotonNetwork.masterClient, Item.SerializeObject<Item>(items[i]), position.position + offset, Quaternion.Euler(rotation));
			} else if(prefab.name.ToLower().StartsWith("ai")) {
				PhotonView.Get(this).RPC("MakeAI", PhotonNetwork.masterClient, prefab.name, position.position + offset, Quaternion.Euler(rotation));
			} else
				PhotonView.Get(this).RPC("MakePrefab", PhotonNetwork.masterClient, prefab.name, position.position + offset, Quaternion.Euler(rotation));
		}
		else if(items != null) {
			for(int i = 0; i < items.Count; i++)
				PhotonView.Get(this).RPC("MakeItem", PhotonNetwork.masterClient, Item.SerializeObject<Item>(items[i]), position.position + offset, Quaternion.Euler(rotation));
		}
		else
			Debug.Log("nothing to spawn");
	}

	[RPC]
	void MakePrefab(string name, Vector3 p, Quaternion r) {
		PhotonNetwork.InstantiateSceneObject("_prefabs/"+name, p, r, 0, null);
	}

	[RPC]
	void MakeAI(string name, Vector3 p, Quaternion r) {
		GameObject g = PhotonNetwork.InstantiateSceneObject("_prefabs/AI/"+name, p, r, 0, null) as GameObject;
		g.GetComponentInChildren<AIRig>().enabled = true;
	}

	[RPC]
	void MakeItem(byte[] dataStream, Vector3 p, Quaternion r) {
		GameObject g = PhotonNetwork.InstantiateSceneObject("_prefabs/DroppedItem", p, r, 0, null) as GameObject;
		PhotonView.Get(this).RPC("InitItem", PhotonTargets.All, PhotonView.Get(g).viewID, dataStream);
	}
	
	[RPC]
	void InitItem(int n, byte[] dataStream) {
		GameObject g = PhotonView.Find(n).gameObject;
		
		MemoryStream stream = new MemoryStream(dataStream);
		stream.Position = 0;
		BinaryFormatter bf = new BinaryFormatter();
		bf.Binder = new VersionFixer();
		Item item = (Item)bf.Deserialize(stream);

		g.GetComponent<ItemRenderer>().item = item;
		g.GetComponentInChildren<PickupAble>().item = item;
	}

	[RPC]
	void MakeChest(byte[] dataStream, Vector3 p, Quaternion r) {
		GameObject g = PhotonNetwork.InstantiateSceneObject("_prefabs/Chest", p, r, 0, null) as GameObject;
		PhotonView.Get(this).RPC("InitChestItem", PhotonTargets.All, PhotonView.Get(g).viewID, dataStream);
	}
	
	[RPC]
	void InitChestItem(int n, byte[] dataStream) {
		GameObject g = PhotonView.Find(n).gameObject;
		
		MemoryStream stream = new MemoryStream(dataStream);
		stream.Position = 0;
		BinaryFormatter bf = new BinaryFormatter();
		bf.Binder = new VersionFixer();
		Item item = (Item)bf.Deserialize(stream);
		
		g.GetComponent<Chest>().items.Add(item);
	}
}