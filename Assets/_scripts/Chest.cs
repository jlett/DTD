using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Holoville.HOTween;

public class Chest : MonoBehaviour {
	public bool open = false;
	bool showMessage = false;
	public int level = 1;
	public int size = 1; //number of items to come out
	public GameObject dropItemPrefab;
	public Transform itemSpawn;
	public List<Item> items;//if this is set, the chest will drop these and only these items

	private PhotonView photonView;
	/*possible chest loot: 
	 * duct tape (in stacks based on level)
	 * ammo (more likely to drop ammo that you are low on)
	 * gun part
	 * full gun (more rare)
	 * if level > 90 small chance for an epic
	 */

	void Start() {
		items = new List<Item>();
		photonView = PhotonView.Get(gameObject);

		if(photonView.instantiationData != null) {
			object[] data = photonView.instantiationData;
			if(((string)data[0]).Equals("itemlist")) {
				Item[] itemlist = new Item[data.Length-1];
				for(int i = 1; i < data.Length; i++) {
					MemoryStream stream = new MemoryStream((byte[])data[1]);
					stream.Position = 0;
					BinaryFormatter bf = new BinaryFormatter();
					bf.Binder = new VersionFixer();
					Item item = (Item)bf.Deserialize(stream);
					itemlist[i-1] = item;
				}
				Init(itemlist);
			} else if(((string)data[0]).Equals("level-size")) {
				Init((int)data[1], (int)data[2]);
			}
		}
	}

	void Init(Item[] itemlist) {
		foreach(Item item in itemlist){
			items.Add(item);
		}
	}

	void Init(int level, int size) {
		this.level = level;
		this.size = size;
	}

	void OnTriggerEnter(Collider other) {
		if(other.tag.Equals("MyPlayer") && !open) {
			showMessage = true;
		}
	}
	
	void OnTriggerExit(Collider other) {
		if(other.tag.Equals("MyPlayer")) {
			showMessage = false;
		}
	}
	
	void Update() {
		if(!open && showMessage && Input.GetKeyDown(KeyCode.E)) {
			PhotonView.Get(gameObject).RPC("ChestWasOpened", PhotonTargets.AllBuffered);
			
			//generate random loot and throw out at random velocity generally upward
			if(this.items.Count < 0) { 
				for(int i = 0; i < size; i++) {
					float r = UnityEngine.Random.value;
					if(level > 90 && r > .98) {
						//throw an epic based on level
					} else if(r < .2) {
						//duct tape
						Item item = new Item();
						item.InitDuctTape((int)(level * 10 * UnityEngine.Random.value));
						items.Add(item);
					} else if(r < .5) {
						//ammo
						Item item = new Item();
						item.InitAmmo((int)(level * 10 * UnityEngine.Random.value), AmmoType.Pistol);
						items.Add(item);
					} else if(r < .85) {
						//gun part
						Item item = new Item();
						item.InitGunPart(level);
						items.Add(item);
					} else {
						//full random gun
						Item item = new Item();
						item.InitGun(level);
						items.Add(item);
					}	
				}
			}
			
			foreach(Item item in items) {
				//generate random velocity, attach to item renderer with pickupAble, and throw it at specified velocity
				photonView.RPC("MakeItem", PhotonNetwork.masterClient, Item.SerializeObject<Item>(item));//but the master client needs to be the one that does it
			}
		}
	}
	
	void OnGUI() {
		if(showMessage)
			GUI.Label(new Rect(500, 500, 200, 20), "Press E to Open");
	}

	[RPC]
	void MakeItem(byte[] dataStream) {//only ever sent to master client
		GameObject g = PhotonNetwork.InstantiateSceneObject("_prefabs/DroppedItem", itemSpawn.position, Quaternion.identity, 0, null) as GameObject;
		Vector3 randVel = new Vector3(UnityEngine.Random.Range(-1f, 1f)/*sideways*/, UnityEngine.Random.Range(10f, 20f)/*up*/, UnityEngine.Random.Range(-10f, -1f)/*forward*/);
		photonView.RPC("InitializeItem", PhotonTargets.All, PhotonView.Get(g).viewID, dataStream, randVel);
	}
	
	[RPC]
	void ChestWasOpened() {
		open = true;
		showMessage = false;
		transform.GetComponentInChildren<BoxCollider>().enabled = false;
		GameObject g = Instantiate(Resources.Load("_prefabs/Particles"), transform.position + new Vector3(0, 1, 0), Quaternion.identity) as GameObject;
		g.GetComponent<particleController>().Init(2.5f);
		GetComponent<Animator>().Play(Animator.StringToHash("open"));
		HOTween.To(transform, 4, new TweenParms().Prop("localScale", new Vector3(0, 0, 0)).OnComplete(onCompletion));
		//animation.Play("open");
	}

	void onCompletion() {
		photonView.RPC("DestroyChest", PhotonNetwork.masterClient);
	}

	[RPC]
	void DestroyChest() {
		PhotonNetwork.Destroy(PhotonView.Get(gameObject));
		//Destroy(gameObject);
	}
	
	[RPC]
	void InitializeItem(int n, byte[] dataStream, Vector3 v) {
		GameObject g = PhotonView.Find(n).gameObject;
		
		MemoryStream stream = new MemoryStream(dataStream);
		stream.Position = 0;
		BinaryFormatter bf = new BinaryFormatter();
		bf.Binder = new VersionFixer();
		Item item = (Item)bf.Deserialize(stream);
		
		g.GetComponent<Rigidbody>().velocity = g.transform.TransformDirection(v);
		g.GetComponent<ItemRenderer>().item = item;
		g.GetComponentInChildren<PickupAble>().item = item;
	}
}