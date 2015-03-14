using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerStats : MonoBehaviour {
	
	public int ductTape = 100;
	public int walkingSpeed = 200;
	public Window windowOpen = Window.None;
	public Item detailsItem = null;
	public bool rolling = false;
	public WeaponManager weaponManager;
	public List<Item> inventory = new List<Item>();
	public Item[] ammo = new Item[6];
	
	//gui stuff
	public GUISkin skin;
	private int selected = -1; //index of selected item in... (-1 if nothing selected)
	private bool selectedInInventory = true; //inventory or equpied
	private bool droppedAnItem = false; //set true if already dropped something this frame
	public Texture itemPreview;
	public GameObject itemPreviewObject;
	public Texture2D tx_barBG;
	public Texture2D tx_healthBar;
	public Texture2D tx_armorBar;
	public Texture2D tx_ammoBar;

	private PhotonView photonView;
	
	void Start() {
		photonView = PhotonView.Get(this);
		gameObject.name = PhotonView.Get(this).owner.name;
		if(photonView.isMine) {
			gameObject.tag = "MyPlayer";//more convenient way of finding if: PhotonView.Get(gameObject).isMine == true
			GetComponent<PlayerMoveController>().viewIsMine = true;
			gameObject.AddComponent<AudioListener>();
		
			GetComponent<FreeMovementMotor>().walkingSpeed = walkingSpeed;
			GetComponent<Health>().maxHealth = 1000;
			GetComponent<Health>().curHealth = 800;
			GetComponent<Health>().maxShield = 100;
			GetComponent<Health>().curShield = 100;
			itemPreviewObject = GameObject.Find("ItemPreviewRenderer");
			
			for(int i = 0; i < ammo.Length; i++) {
				Item item = new Item();
				item.InitAmmo(0, (AmmoType)i);
			}

			//set up status bars
			tx_barBG = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			tx_barBG.SetPixel(0, 0, new Color(1.0f, 1.0f, 1.0f, 0.1f));
			tx_barBG.Apply();

			tx_healthBar = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			tx_healthBar.SetPixel(0, 0, new Color(0.5f, 0f, 0f, 0.5f));
			tx_healthBar.Apply();

			tx_armorBar = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			tx_armorBar.SetPixel(0, 0, new Color(0f, 1.0f, 1.0f, 0.5f));
			tx_armorBar.Apply();

			tx_ammoBar = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			tx_ammoBar.SetPixel(0, 0, new Color(1.0f, 1.0f, 0f, 0.5f));
			tx_ammoBar.Apply();
			
			//------------------------DEBUG STUFF--------------------------------------------
			AddItem(new Item("Item1", Rarity.common, "An uninteresting item"));
			
			Item _gunPart = new Item("GunPart", Rarity.uncommon, "Meh, it's okay i guess");
			_gunPart.InitGunPart(10, 0, 0, 69, 0, 0, 10, 21, GunCompany.Isoid, GunType.Pistol, PartType.Body);
			AddItem(_gunPart);
			
			Item _gunItem = new Item("Cool Gun", Rarity.rare, "this is a description");
			_gunItem.InitGun(30);
			_gunItem.magsize = 10;
			_gunItem.bullets = 3;
			_gunItem.pellets = 4;
			_gunItem.spread = 30;
			_gunItem.explosionRadius = 5;
			weaponManager.equiped.Add(_gunItem);
			Item _gunItem2 = new Item("Awesome Gun", Rarity.epic, "this is a description again");
			_gunItem2.InitGun(90);
			//_gunItem2.specialEffects.Add(SpecialEffects.InfiniteAmmo);
			weaponManager.equiped.Add(_gunItem2);
		}
	}

	void Update() {
		if(photonView.isMine) {
			if(rolling) {
				GetComponent<Rigidbody>().AddForce(transform.forward * 10000000 * Time.deltaTime);
				return;
			}

			//input stuff
			if(Input.GetKeyDown(KeyCode.Alpha1) && windowOpen == Window.None) {
				weaponManager.SwapGun(0);
			} else if(Input.GetKeyDown(KeyCode.Alpha2) && windowOpen == Window.None) {
				weaponManager.SwapGun(1);
			} /*else if(Input.GetKeyDown(KeyCode.LeftShift)) {
				StartCoroutine(Roll());
			}*/ else if(Input.GetKeyDown(KeyCode.Escape)) {
				if(windowOpen == Window.None) {
					windowOpen = Window.Inventory;
					selected = -1;
				} else if(windowOpen == Window.Details) {
					windowOpen = Window.Inventory;
				}else
					windowOpen = Window.None;
			}
			if(Input.GetMouseButton(0) && windowOpen == Window.None) {
				weaponManager.Shoot();
			} else if(Input.GetMouseButtonUp(0) && windowOpen == Window.None) {
				StartCoroutine(Camera.main.GetComponent<ShakeManager>().StopShake());
			}

			//reset on each frame
			droppedAnItem = false;
		}
	}

	void OnGUI() {
		if(photonView.isMine) {
			//----------SET GUI SKIN-----------
			if(skin != null) {
				GUI.skin = skin;
			}

			//----------INVENTORY WINDOW-----------
			if(windowOpen == Window.Inventory) {
				Rect r = new Rect(0, 0, 800, 600);
				r.center = new Vector2(Screen.width/2, Screen.height/2);
				GUI.Window(0, r, DoInvWindow, "Inventory");
				GUI.FocusWindow(0);
			}

			//----------HUD STUFF-----------
			Vector2 pos = new Vector2(20,20);
			Vector2 size = new Vector2(200,20);
			int barMargin = 5;

			//health
			GUI.BeginGroup(new Rect(pos.x, pos.y, size.x, size.y));
				GUI.DrawTexture(new Rect(0,0, size.x, size.y), tx_barBG);
				//draw the filled-in part:
				GUI.BeginGroup(new Rect(0,0, size.x * ((float)GetComponent<Health>().curHealth/(float)GetComponent<Health>().maxHealth), size.y));
					GUI.DrawTexture(new Rect(0,0, size.x, size.y), tx_healthBar);
				GUI.EndGroup();
			GUI.EndGroup();

			//armor
			GUI.BeginGroup(new Rect(pos.x, pos.y + size.y + barMargin, size.x, size.y));
				GUI.DrawTexture(new Rect(0,0, size.x, size.y), tx_barBG);
				//draw the filled-in part:
				GUI.BeginGroup(new Rect(0,0, size.x * ((float)GetComponent<Health>().curShield/(float)GetComponent<Health>().maxShield), size.y));
					GUI.DrawTexture(new Rect(0,0, size.x, size.y), tx_armorBar);
				GUI.EndGroup();
			GUI.EndGroup();

			//ammo
			GUI.BeginGroup(new Rect(pos.x, pos.y + 2*(size.y + barMargin), size.x, size.y));
				GUI.DrawTexture(new Rect(0,0, size.x, size.y), tx_barBG);
				//draw the filled-in part:
				if(weaponManager.gun.GetComponent<Gun>().gun.bullets > 0)
			   		GUI.BeginGroup(new Rect(0,0, size.x * ((float)weaponManager.gun.GetComponent<Gun>().gun.bullets/(float)weaponManager.gun.GetComponent<Gun>().gun.magsize), size.y));
				else
					GUI.BeginGroup(new Rect(0,0, size.x * (weaponManager.gun.GetComponent<Gun>().reloadPer), size.y));
				GUI.DrawTexture(new Rect(0,0, size.x, size.y), tx_ammoBar);
				GUI.EndGroup();
			GUI.EndGroup();
		}
	}
	
	Vector2 scrollPosition = Vector2.zero;
	void DoInvWindow(int windowId) {
		int WINDOW_WIDTH = 800;
		int WINDOW_HEIGHT = 600;
		int COLUMN_WIDTH = 220;
		int ITEM_HEIGHT = 40;
		bool detailsUp = false;

		/*REGULAR INVENTORY*/
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;
		Rect[] itemRects = new Rect[inventory.Count];
		scrollPosition = GUI.BeginScrollView(new Rect(30, 50, COLUMN_WIDTH+13, WINDOW_HEIGHT-70), scrollPosition, new Rect(0, 0, COLUMN_WIDTH, (ITEM_HEIGHT+2)*inventory.Count));
		for(int i = 0; i < inventory.Count; i++) {
			Rect rect = new Rect(0, (ITEM_HEIGHT + 2) * i, COLUMN_WIDTH, ITEM_HEIGHT);
			Item item = inventory[i];
			string boxStyle = "Box";
			if(selectedInInventory && selected == i)//this is the selected item
				boxStyle = "BoxSelected";
			if(GUI.Button(rect, UseRarityColor(item.rarity, item.title), skin.GetStyle(boxStyle))) {
				if(selected == -1) {
					selected = i;
					selectedInInventory = true;
				} else {
					if(selectedInInventory || (!selectedInInventory && item.itemType == ItemType.Gun))
						SwapItems(i, true);
					selected = -1;
				}
				AudioSource.PlayClipAtPoint(Resources.Load("_sounds/Selection_Weapon_Select_02") as AudioClip, transform.position);
			}
			itemRects[i] = rect;
		}
		GUI.EndScrollView();

		/*REGULAR INVENTORY DETAILS*/
		GUI.skin.box.alignment = TextAnchor.MiddleLeft;
		for(int i = 0; i < inventory.Count; i++) {
			Rect rect = new Rect(30, 50	 - scrollPosition.y + itemRects[i].y, COLUMN_WIDTH, itemRects[i].height);
			Item item = inventory[i];
			if(rect.Contains(Event.current.mousePosition)) {
				if(Input.GetKeyDown(KeyCode.Q) && !droppedAnItem) {
					DropItem(i);
					droppedAnItem = true;
				}
				
				Rect detailsRect = new Rect(WINDOW_WIDTH/2 - (COLUMN_WIDTH/2), WINDOW_HEIGHT - 250, WINDOW_WIDTH/2 + COLUMN_WIDTH/2 - 30, 240);
				detailsUp = true;

				string s = BriefDetails(item);
				itemPreviewObject.GetComponent<ItemRenderer>().item = item;
				itemPreviewObject.GetComponent<ItemRenderer>().RemakeMesh();
				GUI.Box(detailsRect, s);
				GUI.DrawTexture(new Rect(detailsRect.center.x, detailsRect.y, detailsRect.width/2, detailsRect.height), itemPreview);
			}
		}

		/*EQUPIED INVENTORY*/
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;
		Rect[] equipedRects = new Rect[weaponManager.equiped.Count];
		for(int i = 0; i < weaponManager.equiped.Count; i++) {
			Rect rect = new Rect(WINDOW_WIDTH/2 - (COLUMN_WIDTH/2), 50 + (ITEM_HEIGHT + 2) * i, COLUMN_WIDTH, ITEM_HEIGHT);
			Item item = weaponManager.equiped[i];

			string boxStyle = "Box";
			if(!selectedInInventory && selected == i)//this is the selected item
				boxStyle = "BoxSelected";
			if(GUI.Button(rect, UseRarityColor(item.rarity, item.title), skin.GetStyle(boxStyle))) {
				if(selected == -1) {
					selected = i;
					selectedInInventory = false;
				} else {
					if(!selectedInInventory || (selectedInInventory && inventory[selected].itemType == ItemType.Gun))
						SwapItems(i, false);
					//WHY DOES IT NOT REACH THIS LINE?! oh its cause the prefab is null and breaks shit in item renderer after switch, dont worry bout it it's vince's fault
					selected = -1;
				}
				AudioSource.PlayClipAtPoint(Resources.Load("_sounds/Selection_Weapon_Select_02") as AudioClip, transform.position);
			}
			equipedRects[i] = rect;
		}

		/*EQUIPED DETAILS*/
		GUI.skin.box.alignment = TextAnchor.MiddleLeft;
		for(int i = 0; i < equipedRects.Length; i++) {
			Rect rect = equipedRects[i];
			Item item = weaponManager.equiped[i];
			if(rect.Contains(Event.current.mousePosition)) {
				detailsUp = true;
				Rect detailsRect = new Rect(WINDOW_WIDTH/2 - (COLUMN_WIDTH/2), WINDOW_HEIGHT - 250, WINDOW_WIDTH/2 + COLUMN_WIDTH/2 - 30, 240);
				if(item.itemType == ItemType.Gun) {
					string s = BriefDetails(item);
					itemPreviewObject.GetComponent<ItemRenderer>().item = item;
					itemPreviewObject.GetComponent<ItemRenderer>().RemakeMesh();
					GUI.Box(detailsRect, s);
					GUI.DrawTexture(new Rect(detailsRect.center.x, detailsRect.y, detailsRect.width/2, detailsRect.height), itemPreview);				
				}
			}
		}

		/*INFO STUFF*/
		GUI.skin.box.alignment = TextAnchor.MiddleRight;
		Rect infoRect = new Rect(WINDOW_WIDTH - 240, 50, COLUMN_WIDTH, 200);
		string infoString = "";
		infoString += "Duct tape: " + ductTape.ToString();
		infoString += "\nPistol Ammo: " + ammo[0].amount;
		infoString += "\nSMG Ammo: " + ammo[1].amount;
		infoString += "\nAssult Rifle Ammo: " + ammo[2].amount;
		infoString += "\nShotgun Ammo: " + ammo[3].amount;
		infoString += "\nSniper Ammo: " + ammo[4].amount;
		infoString += "\nLauncher Ammo: " + ammo[5].amount;
		GUI.Box(infoRect, infoString);
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;

		/*MENU BUTTONS ETC (only visible while !details)*/
		if(!detailsUp) {
			//buttons: save and quit to menu, mute sounds, mute music, 
			GUI.Toggle(new Rect(WINDOW_WIDTH - 240, WINDOW_HEIGHT - 120, COLUMN_WIDTH, ITEM_HEIGHT), false, "Mute Sounds");
			GUI.Toggle(new Rect(WINDOW_WIDTH - 240, WINDOW_HEIGHT - 90, COLUMN_WIDTH, ITEM_HEIGHT), false, "Mute Music");
			if(GUI.Button(new Rect(WINDOW_WIDTH - 240, WINDOW_HEIGHT - 60, COLUMN_WIDTH, ITEM_HEIGHT), "Save and Quit")) {
				PhotonNetwork.LeaveRoom();
				PhotonNetwork.LoadLevel("MainMenuScene");
			}
		}
	}

	public static string UseRarityColor(Rarity r, string text) {
		string s = "";
		if(r == Rarity.common) {
			s += "<color=white>";
		}else if(r == Rarity.uncommon) {
			s += "<color=lime>";
		}else if(r == Rarity.valuable) {
			s += "<color=blue>";
		}else if(r == Rarity.rare) {
			s += "<color=magenta>";
		}else if(r == Rarity.epic) {
			s += "<color=orange>";
		}else if(r == Rarity.unique) {
			s += "<color=cyan>";
		}
		s += text;
		s += "</color>";
		return s;
	}

	public static string BriefDetails(Item item) {//no more than 8ish lines, reserve 2 for description and 2 for special effects
		string s = "";
		//level + name
		s += UseRarityColor(item.rarity, "Level " + item.level + " " + item.title + "\n");
		//description
		if(item.desc.Length > 0)
			s += item.desc + "\n";
		//damage
		if((item.itemType == ItemType.Gun || item.itemType == ItemType.GunPart) && item.damage > 0) {
			s += "Damage: " + item.damage;
			if(item.pellets > 0) {
				s += "x" + item.pellets;
			}
			s += "\n";
		}
		//fireRate
		if((item.itemType == ItemType.Gun || item.itemType == ItemType.GunPart) && item.firerate > 0)
			s += "Fire Rate: " + item.firerate + "\n";

		//for non-guns show as much of normal stats as you can but no special effects
		if(item.itemType != ItemType.Gun && item.accuracy > 0)
			s += "Accuracy: " + item.accuracy + "\n";
		if(item.itemType != ItemType.Gun && item.recoil > 0)
			s += "Recoil: " + item.recoil + "\n";
		if(item.itemType != ItemType.Gun && item.accuracy > 0)
			s += "Accuracy: " + item.accuracy + "\n";
		if(item.itemType != ItemType.Gun && item.reload > 0)
			s += "Reload Speed: " + item.reload + "\n";
		if(item.itemType != ItemType.Gun && item.magsize > 0)
			s += "Mag Size: " + item.magsize + "\n";

		//dps if gun
		if(item.itemType == ItemType.Gun)
			s += "DPS: " + (item.damage * (item.firerate/10f)) + "\n";
		//specialEffects if gun or attachment
		if((item.itemType == ItemType.Gun || item.itemType == ItemType.Attachment) && item.specialEffects.Count > 0) {
			//only show first two
			s += SpecialEffectDesc(item.specialEffects[0]) + "\n";
			if(item.specialEffects.Count > 1) {
				s += SpecialEffectDesc(item.specialEffects[1]);
				if(item.specialEffects.Count > 2)
					s += "\t+more";
			}
		}
		return s;
	}

	public static string InDepthDetails(Item item) {//say pretty much everything there is to know
		string s = "this is where \nin depth details \nwould go";


		return s;
	}

	public static string SpecialEffectDesc(SpecialEffects s) {
		if(s == SpecialEffects.InfiniteAmmo)
			return "Why stop?";
		if(s == SpecialEffects.NoReload)
			return "\"Reloading takes too long!\"";
		else
			return "Probably does something cool";
	}

	public void SwapItems(int i, bool inInventory) {
		Item a;
		if(selectedInInventory) {
			a = inventory[selected];
			if(inInventory) {
				inventory[selected] = inventory[i];
				inventory[i] = a;
			} else {
				inventory[selected] = weaponManager.equiped[i];
				weaponManager.equiped[i] = a;
			}
		} else {
			a = weaponManager.equiped[selected];
			if(inInventory) {
				weaponManager.equiped[selected] = inventory[i];
				inventory[i] = a;
			} else {
				weaponManager.equiped[selected] = weaponManager.equiped[i];
				weaponManager.equiped[i] = a;
			}
		}
		selected = -1;
		weaponManager.UpdateGun();
	}
	
	IEnumerator Roll() {
		rolling = true;
		Vector3 originalPos = transform.position;
		yield return new WaitForSeconds(0.1f);
		Vector3 newPos = transform.position;
		gameObject.GetComponent<PlayerMoveController>().rollDelta = newPos - originalPos;
		gameObject.GetComponent<PlayerMoveController>().rolling = true;
		yield return new WaitForSeconds(0.5f);
		rolling = false;
		gameObject.GetComponent<PlayerMoveController>().rolling = false;
		//play animation, lock movemnt direction, ignore input, move quickly a set amount in that direction, temp invincibility
	}

	void OnDeath() {
		Debug.Log("player died!");
	}

	public void AddItem(Item o) {
		inventory.Add(o);
	}

	public void RemoveItem(Item o) {
		inventory.Remove(o);
	}

	public void RemoveItem(int index) {
		inventory.RemoveAt(index);
	}
	
	public void DropItem(int index) {
		Item item;
		item = inventory[index];
		for(int i = index; i < inventory.Count-1; i++) {
			inventory[i] = inventory[i + 1];
		}
		inventory.RemoveAt(inventory.Count - 1);

		photonView.RPC("MakeItem", PhotonNetwork.masterClient, Item.SerializeObject<Item>(item));
	}

	[RPC]
	void MakeItem(byte[] dataStream) {
		GameObject g = PhotonNetwork.InstantiateSceneObject("_prefabs/DroppedItem", transform.position + new Vector3(0, 0, 0), Quaternion.identity, 0, null) as GameObject;
		Vector3 randVel = new Vector3(UnityEngine.Random.Range(1f, 10f), UnityEngine.Random.Range(5f, 10f), UnityEngine.Random.Range(-1f, 1f));
		photonView.RPC("InitItemDropped", PhotonTargets.All, PhotonView.Get(g).viewID, dataStream, randVel);
	}
	
	[RPC]
	void InitItemDropped(int n, byte[] dataStream, Vector3 v) {
		GameObject g = PhotonView.Find(n).gameObject;
		
		MemoryStream stream = new MemoryStream(dataStream);
		stream.Position = 0;
		BinaryFormatter bf = new BinaryFormatter();
		bf.Binder = new VersionFixer();
		Item item = (Item)bf.Deserialize(stream);
		
		g.GetComponent<Rigidbody>().velocity = v;
		g.GetComponent<ItemRenderer>().item = item;
		g.GetComponentInChildren<PickupAble>().item = item;
	}

//	public IEnumerator OtherWindowClose() { //used so that esc doesnt open inv immediatly after closing other window
//		yield return new WaitForEndOfFrame();
//		windowOpen = Window.None;
//	}
	
	public Item CraftGun(Item clip, Item body, Item barrel, Item[] attachments = null) {
		//calculate how much duct tape needed
		int ductTapeNeeded = 10;
		
		if(ductTape < ductTapeNeeded)
			return null;
		else {
			Item[] parts = {clip, body, barrel};
			Item craftedGun = new Item();
			craftedGun.InitGun(parts, attachments);
			return craftedGun;
		}
	}
}

public enum Window {
	None,
	Inventory,
	Details,
	Other//handled by a different script
};
