using UnityEngine;
using System;
using System.Collections;

public class ItemRenderer : MonoBehaviour {
	
	//class is used for dropped items, held guns, gun icons(?), and any other instance where an item needs to be shown
	//use: attach script to an entity with an item component and it will do its best to render it
	
	public Item item;
	public bool collidersOn = true;
	
	// Use this for initialization
	void Start() {
		RemakeMesh();
	}
	
	public void RemakeMesh() {
		foreach(Transform t in GetComponentsInChildren<Transform>()) {
			if(t.gameObject.name.Equals("ItemRender"))
				Destroy(t.gameObject);
		}
		
		if(item.itemType == ItemType.Gun) {
			string s = "_prefabs/guns/";
			s += item.gunBody.gunType.ToString().ToLower() + "/" + item.gunBody.gunType.ToString().ToLower() + "_body";
			s += (int)(item.gunBody.company + 1);
			if(item.gunBody.gunType != GunType.Pistol && item.gunBody.gunType != GunType.SMG)
				s = "_prefabs/guns/pistol/pistol_body0";
			GameObject o = Instantiate(Resources.Load(s), Vector3.zero, transform.rotation) as GameObject;
			o.transform.parent = transform;
			o.transform.localPosition = o.GetComponent<GunPartPoints>().offset * .1f;
			o.name = "ItemRender";
			
			string s2 = "_prefabs/guns/";
			s2 += item.gunClip.gunType.ToString().ToLower() + "/" + item.gunClip.gunType.ToString().ToLower() + "_clip";
			s2 += (int)(item.gunClip.company + 1);
			if(item.gunClip.gunType != GunType.Pistol && item.gunClip.gunType != GunType.SMG)
				s2 = "_prefabs/guns/pistol/pistol_clip0";
			GameObject o2 = Instantiate(Resources.Load(s2), Vector3.zero, transform.rotation) as GameObject;
			o2.transform.parent = o.transform;
			o2.transform.localPosition = o.GetComponent<GunPartPoints>().clipAttach * .1f;
			o2.name = "ItemRender";
		
			string s3 = "_prefabs/guns/";
			s3 += item.gunBarrel.gunType.ToString().ToLower() + "/" + item.gunBarrel.gunType.ToString().ToLower() + "_barrel";
			s3 += (int)(item.gunBarrel.company + 1);
			if(item.gunBarrel.gunType != GunType.Pistol && item.gunBarrel.gunType != GunType.SMG)
				s3 = "_prefabs/guns/pistol/pistol_barrel0";
			GameObject o3 = Instantiate(Resources.Load(s3), Vector3.zero, transform.rotation) as GameObject;
			o3.transform.parent = o.transform;
			o3.transform.localPosition = o.GetComponent<GunPartPoints>().barrelAttach * .1f;
			o3.name = "ItemRender";
			
			item.bulletSpawnX = o.GetComponent<GunPartPoints>().barrelAttach.x + o3.GetComponent<GunPartPoints>().bulletSpawn.x;
			item.bulletSpawnY = o.GetComponent<GunPartPoints>().barrelAttach.y + o3.GetComponent<GunPartPoints>().bulletSpawn.y;
			item.bulletSpawnZ = o.GetComponent<GunPartPoints>().barrelAttach.z + o3.GetComponent<GunPartPoints>().bulletSpawn.z;
		} else if(item.itemType == ItemType.GunPart) {
			string s = "_prefabs/guns/";
			s += item.gunType.ToString().ToLower() + "/" + item.gunType.ToString().ToLower() + "_";
			s += item.partType.ToString().ToLower();
			s += (int)(item.company + 1);
			if(item.gunType != GunType.Pistol && item.gunType != GunType.SMG)
				s = "_prefabs/guns/pistol/pistol_" + item.partType.ToString().ToLower() + "0";
			GameObject o = Instantiate(Resources.Load(s), Vector3.zero, transform.rotation) as GameObject;
			o.transform.parent = transform;
			o.transform.localPosition = Vector3.zero;
			o.name = "ItemRender";
		} else if(item.itemType == ItemType.DuctTape) {
			GameObject o = Instantiate(Resources.Load("_prefabs/ductTape"), Vector3.zero, transform.rotation) as GameObject;
			o.transform.parent = transform;
			o.transform.localPosition = Vector3.zero;
			o.name = "ItemRender";
		} else {
			string s = "_prefabs/guns/pistol/pistol_barrel0";
			GameObject o = Instantiate(Resources.Load(s), Vector3.zero, transform.rotation) as GameObject;
			o.transform.parent = transform;
			o.transform.localPosition = Vector3.zero;
			o.name = "ItemRender";
		}
		
		foreach(Transform t in GetComponentsInChildren<Transform>()) {
			t.gameObject.layer = gameObject.layer;
			if(!collidersOn)
				Destroy(t.gameObject.GetComponent<MeshCollider>());
		}
	}
}