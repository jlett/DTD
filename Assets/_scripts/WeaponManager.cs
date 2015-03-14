using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class WeaponManager : MonoBehaviour {
	
	public GameObject gun;
	public List<Item> equiped = new List<Item>();
	public int curEquiped = 0;

	private PhotonView photonView;
	// Use this for initialization
	void Start () {
		photonView = PhotonView.Get(gameObject);
	}
	
	public void SwapGun(int n) {
		photonView.RPC("SwapGunRPC", PhotonTargets.OthersBuffered, PhotonView.Get(this).owner.name, Item.SerializeObject<Item>(equiped[n]));
		gun.GetComponent<Gun>().SwapItem(equiped[n]);
		curEquiped = n;
	}
	
	public void Shoot() {
		gun.GetComponent<Gun>().Shoot();
	}
	
	public void EquipGun(Item gun, int index) {
		equiped[index] = gun;
	}
	
	public void UpdateGun() {
		if(gun.GetComponent<Gun>().gun != equiped[curEquiped]) {
			photonView.RPC("SwapGunRPC", PhotonTargets.OthersBuffered, PhotonView.Get(this).owner.name, Item.SerializeObject<Item>(equiped[curEquiped]));
			gun.GetComponent<Gun>().SwapItem(equiped[curEquiped]);
		}
	}
	
	[RPC]
	void SwapGunRPC(string owner, byte[] dataStream) {
		MemoryStream stream = new MemoryStream(dataStream);
		stream.Position = 0;
		BinaryFormatter bf = new BinaryFormatter();
		bf.Binder = new VersionFixer();
		Item item = (Item)bf.Deserialize(stream);
		
		GameObject.Find(owner).GetComponentInChildren<WeaponManager>().gun.GetComponent<Gun>().SwapItem(item);
	}
}
