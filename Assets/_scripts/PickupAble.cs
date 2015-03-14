using UnityEngine;
using System.Collections;

public class PickupAble : MonoBehaviour {

	public Item item; //item to be added to inv when picked up
	bool showMessage = false;
	
	void Start() {
		
	}
	
	void OnTriggerEnter(Collider other) {
		if(other.tag.Equals("MyPlayer")) {
			showMessage = true;
		}
	}
	
	void Update() {
		if(showMessage && Input.GetKeyDown(KeyCode.E)) {
			if(item.itemType == ItemType.DuctTape) 
				GameObject.FindGameObjectsWithTag("MyPlayer")[0].GetComponent<PlayerStats>().ductTape += item.amount;
			else if(item.itemType == ItemType.Ammo)
				GameObject.FindGameObjectsWithTag("MyPlayer")[0].GetComponent<PlayerStats>().ammo[(int)item.ammoType].amount += item.amount;
			else
				GameObject.FindGameObjectsWithTag("MyPlayer")[0].GetComponent<PlayerStats>().AddItem(item);
			if(PhotonView.Get(this).isMine)
				PhotonNetwork.Destroy(PhotonView.Get(transform.parent.gameObject));
			else
				PhotonView.Get(this).RPC("PickedUp", PhotonNetwork.masterClient);
			//GameObject.Destroy(gameObject);
		}
	}
	
	void OnTriggerExit(Collider other) {
		if(other.tag.Equals("MyPlayer")) {
			showMessage = false;
		}
	}
	
	void OnGUI() {
		if(showMessage)
			GUI.Label(new Rect(500, 500, 200, 20), "Press E to Pickup");
	}

	[RPC]
	void PickedUp() {
		PhotonNetwork.Destroy(PhotonView.Get(transform.parent.gameObject));
	}
}
