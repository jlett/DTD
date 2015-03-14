using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShopController : MonoBehaviour {

	bool showMessage = false;
	bool open = false;
	bool openForMe = false;
	public List<Item> shop = new List<Item>();
	public GUISkin skin;
	private PhotonView photonView;
	PlayerStats p;
	private int selected = -1; //index of selected item in... (-1 if nothing selected)
	private bool selectedInInventory = true; //inventory or shop

	void Start() {
		photonView = PhotonView.Get(gameObject);
		Init(6, 30);
	}

	void Init(int size, int level) {//size: how many items in shop shop.	level: max level available from shop, lowest = level-2

		for(int i = 0; i < size; i++) {
			float r = UnityEngine.Random.value;
			if(r < .7) {
				//gun part
				Item item = new Item();
				item.InitGunPart(level);
				shop.Add(item);
			} else{
				//full random gun
				Item item = new Item();
				item.InitGun(level);
				shop.Add(item);
			}
		}

		//debug stuff
		shop.Add(new Item("Item1", Rarity.common, "An uninteresting item"));
		
		Item _gunPart = new Item("GunPart", Rarity.common, "Meh, it's okay i guess");
		_gunPart.InitGunPart(10, 0, 0, 69, 0, 0, 10, 21, GunCompany.Isoid, GunType.Pistol, PartType.Body);
		shop.Add(_gunPart);
		
		Item _gunItem = new Item("Cool Gun", Rarity.rare, "this is a description");
		_gunItem.InitGun(99);
		shop.Add(_gunItem);
		Item _gunItem2 = new Item("Awesome Gun", Rarity.rare, "this is a description again");
		_gunItem2.InitGun(10);
		shop.Add(_gunItem2);
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
			open = true;
			openForMe = true;
			GameObject.FindGameObjectWithTag("MyPlayer").GetComponent<PlayerStats>().windowOpen = Window.Other;
			showMessage = false;
			photonView.RPC("ShopOpened", PhotonTargets.OthersBuffered);
		}else if(openForMe && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))) {
			open = false;
			openForMe = false;
			//StartCoroutine(GameObject.FindGameObjectWithTag("MyPlayer").GetComponent<PlayerStats>().OtherWindowClose());
			photonView.RPC("ShopClosed", PhotonTargets.OthersBuffered);
		}

		if(invItemClicked)
			invItemClicked = false;
	}

	void OnGUI() {
		if(skin != null)
			GUI.skin = skin;
		if(showMessage)
			GUI.Label(new Rect(500, 500, 200, 30), "Press E to Shop");
		else if(openForMe) {
			Rect r = new Rect(0, 0, 800, 600);
			r.center = new Vector2(Screen.width/2, Screen.height/2);
			p = GameObject.FindGameObjectWithTag("MyPlayer").GetComponent<PlayerStats>();
			GUI.Window(1, r, DoShopWindow, "Shop");
			GUI.FocusWindow(1);
		}
	}

	bool invItemClicked = false;
	Vector2 scrollPosition = Vector2.zero;
	Vector2 scrollPosition2 = Vector2.zero;
	void DoShopWindow(int windowId) {
		int WINDOW_WIDTH = 800;
		int WINDOW_HEIGHT = 600;
		int COLUMN_WIDTH = 220;
		int ITEM_HEIGHT = 40;
		bool detailsUp = false;
		
		/*REGULAR INVENTORY*/
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;
		Rect[] itemRects = new Rect[p.inventory.Count];
		scrollPosition = GUI.BeginScrollView(new Rect(30, 50, COLUMN_WIDTH+13, WINDOW_HEIGHT-70), scrollPosition, new Rect(0, 0, COLUMN_WIDTH, (ITEM_HEIGHT+2)*p.inventory.Count));
		for(int i = 0; i < p.inventory.Count; i++) {
			Rect rect = new Rect(0, (ITEM_HEIGHT + 2) * i, COLUMN_WIDTH, ITEM_HEIGHT);
			Item item = p.inventory[i];
			string boxStyle = "Box";
			if(selectedInInventory && selected == i)//this is the selected item
				boxStyle = "BoxSelected";
			if(GUI.Button(rect, PlayerStats.UseRarityColor(item.rarity, item.title), skin.GetStyle(boxStyle))) {
				selected = i;
				selectedInInventory = true;
				AudioSource.PlayClipAtPoint(Resources.Load("_sounds/Selection_Weapon_Select_02") as AudioClip, transform.position);
			}
			itemRects[i] = rect;
		}
		GUI.EndScrollView();
		
		/*REGULAR INVENTORY DETAILS*/
		GUI.skin.box.alignment = TextAnchor.MiddleLeft;
		for(int i = 0; i < p.inventory.Count; i++) {
			Rect rect = new Rect(30, 50	 - scrollPosition.y + itemRects[i].y, COLUMN_WIDTH, itemRects[i].height);
			Item item = p.inventory[i];
			if(rect.Contains(Event.current.mousePosition)) {
				
				Rect detailsRect = new Rect(WINDOW_WIDTH/2 - (COLUMN_WIDTH/2), WINDOW_HEIGHT - 250, WINDOW_WIDTH/2 + COLUMN_WIDTH/2 - 30, 240);
				detailsUp = true;
				
				string s = PlayerStats.BriefDetails(item);
				p.itemPreviewObject.GetComponent<ItemRenderer>().item = item;
				p.itemPreviewObject.GetComponent<ItemRenderer>().RemakeMesh();
				GUI.Box(detailsRect, s);
				GUI.DrawTexture(new Rect(detailsRect.center.x, detailsRect.y, detailsRect.width/2, detailsRect.height), p.itemPreview);
			}
		}
		
		/*Shop INVENTORY*/
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;
		Rect[] shopRects = new Rect[shop.Count];
		scrollPosition2 = GUI.BeginScrollView(new Rect(WINDOW_WIDTH/2 - (COLUMN_WIDTH/2), 50, COLUMN_WIDTH+13, WINDOW_HEIGHT-300), scrollPosition2, new Rect(0, 0, COLUMN_WIDTH, (ITEM_HEIGHT+2)*shop.Count));
		for(int i = 0; i < shop.Count; i++) {
			Rect rect = new Rect(0, (ITEM_HEIGHT + 2) * i, COLUMN_WIDTH, ITEM_HEIGHT);
			Item item = shop[i];
			
			string boxStyle = "Box";
			if(!selectedInInventory && selected == i)//this is the selected item
				boxStyle = "BoxSelected";
			if(GUI.Button(rect, PlayerStats.UseRarityColor(item.rarity, item.title), skin.GetStyle(boxStyle))) {
				selected = i;
				selectedInInventory = false;
				AudioSource.PlayClipAtPoint(Resources.Load("_sounds/Selection_Weapon_Select_02") as AudioClip, transform.position);
			}
			shopRects[i] = rect;
		}
		GUI.EndScrollView();
		
		/*shop DETAILS*/
		GUI.skin.box.alignment = TextAnchor.MiddleLeft;
		for(int i = 0; i < shopRects.Length; i++) {
			Rect rect = new Rect(WINDOW_WIDTH/2 - (COLUMN_WIDTH/2), 50	 - scrollPosition2.y + shopRects[i].y, COLUMN_WIDTH, shopRects[i].height);
			Item item = shop[i];
			if(rect.Contains(Event.current.mousePosition)) {
				detailsUp = true;
				Rect detailsRect = new Rect(WINDOW_WIDTH/2 - (COLUMN_WIDTH/2), WINDOW_HEIGHT - 250, WINDOW_WIDTH/2 + COLUMN_WIDTH/2 - 30, 240);
				if(item.itemType == ItemType.Gun) {
					string s = PlayerStats.BriefDetails(item);
					p.itemPreviewObject.GetComponent<ItemRenderer>().item = item;
					p.itemPreviewObject.GetComponent<ItemRenderer>().RemakeMesh();
					GUI.Box(detailsRect, s);
					GUI.DrawTexture(new Rect(detailsRect.center.x, detailsRect.y, detailsRect.width/2, detailsRect.height), p.itemPreview);				
				}
			}
		}
		
		/*INFO STUFF*/

		GUI.skin.box.alignment = TextAnchor.MiddleRight;
		Rect infoRect = new Rect(WINDOW_WIDTH - 240, 50, COLUMN_WIDTH, 200);
		string infoString = "";
		infoString += "Duct tape: " + p.ductTape.ToString();
		infoString += "\nPistol Ammo: " + p.ammo[0].amount;
		infoString += "\nSMG Ammo: " + p.ammo[1].amount;
		infoString += "\nAssult Rifle Ammo: " + p.ammo[2].amount;
		infoString += "\nShotgun Ammo: " + p.ammo[3].amount;
		infoString += "\nSniper Ammo: " + p.ammo[4].amount;
		infoString += "\nLauncher Ammo: " + p.ammo[5].amount;
		GUI.Box(infoRect, infoString);
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;
		
		/*SHOP BUTTONS ETC (only visible while !details)*/
		if(!detailsUp) {
			//buttons: buy selected, sell selected (disabled when no selected), buy for each ammo
			if(selectedInInventory) {
				if(GUI.Button(new Rect(WINDOW_WIDTH - 240, WINDOW_HEIGHT - 60, COLUMN_WIDTH, ITEM_HEIGHT), "Sell Selected")) {
					SellItem(selected);
				}
			} else {
				if(GUI.Button(new Rect(WINDOW_WIDTH - 240, WINDOW_HEIGHT - 60, COLUMN_WIDTH, ITEM_HEIGHT), "Buy Selected")) {
					BuyItem(selected);
				}
			}
		}
	}

	public int PriceOfItem(Item item) {
		return 10;
	}

	void SellItem(int index) {
		p.ductTape += PriceOfItem(p.inventory[index]);
		shop.Add(p.inventory[index]);
		p.RemoveItem(index);
	}

	void BuyItem(int index) {
		p.ductTape -= PriceOfItem(shop[index]);
		p.AddItem(shop[index]);
		shop.RemoveAt(index);
	}

	[RPC]
	void ShopOpened() {
		open = true;
		showMessage = false;
	}

	[RPC]
	void ShopClosed() {
		open = false;
	}
}