using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Item {
	
	public Rarity rarity;
	public string title, desc;
	public ItemType itemType = ItemType.Item;
	
	//gun/gunpart/attachment variables
	public int damage, firerate, reload, accuracy, range, recoil, magsize, level, bulletSpeed = 50;
	public GunCompany company;
	public PartType partType;
	public GunType gunType;
	
	//gun variables
	public float bullets;//treated as an int in most cases, only used as float when reloading
	public Item gunClip, gunBody, gunBarrel;
	public float bulletSpawnX, bulletSpawnY, bulletSpawnZ;
	public Aura aura;
	public int pellets;//shotgun
	public float spread;//shotgun
	public int explosionRadius;//launcher
	public int projectileSize = 1;
	
	public List<SpecialEffects> specialEffects = new List<SpecialEffects>();
	
	//ammo variables
	public AmmoType ammoType;
	
	//ductTape/ammo variables
	public int amount;
	
	public Item(string n = "", Rarity r = Rarity.common, string d = "") {
		rarity = r;
		title = n;
		desc = d;
	}
	
	//duct tape constructor
	public void InitDuctTape(int a) {
		rarity = Rarity.common;
		title = "Duct Tape";
		desc = "Useful in many situations";
		
		itemType = ItemType.DuctTape;
		amount = a;
	}
	
	//ammo constructor
	public void InitAmmo(int a, AmmoType t) {
		rarity = Rarity.common;
		title = "Ammo";
		desc = "Used to shoot things";
		
		itemType = ItemType.Ammo;
		ammoType = t;
		amount = a;
	}
	
	public void InitGun(int level) {
		Item[] parts = new Item[3];
		for(int i = 0; i < parts.Length; i++) {
			Item p = new Item();
			p.InitGunPart(level);
			parts[i] = p;
		}
		//TODO add possibility for attachments here
		InitGun(parts, null);
	}
	
	public void InitGun(Item[] parts, Item[] attachments = null) {
		gunBody = parts[0];
		gunClip = parts[1];
		gunBarrel = parts[2];
		itemType = ItemType.Gun;
		for(int i = 0; i < 3; i++) {//TODO this will change probably
			damage += parts[i].damage;
			firerate += parts[i].firerate;
			reload += parts[i].reload;
			accuracy += parts[i].accuracy;
			recoil += parts[i].recoil;
			range += parts[i].range;
			magsize += parts[i].magsize;
			level += parts[i].level;
		}
		level = (int)(level/3);//avg level
		company = parts[0].company;//company is same as body company, not that it really matters in foreseeable future
		ammoType = (AmmoType)((int)gunClip.gunType);
		//make actually good l8r


		//------------------------------FINALIZE GUN, ALL CREATION LEADS HERE EVENTUALLY----------------------------------
		title = JsonReader.PartName(gunClip.company, gunClip.gunType, PartType.Clip) + JsonReader.PartName(gunBody.company, gunBody.gunType, PartType.Body) + JsonReader.PartName(gunBarrel.company, gunBarrel.gunType, PartType.Barrel);
		bullets = magsize;
	}
	
	//gunpart constructor
	public void InitGunPart(int level, int damage, int firerate, int reload, int accuracy, int recoil, int range, int magsize, GunCompany company, GunType type, PartType partType) {
		itemType = ItemType.GunPart;
		this.level = level;
		this.damage = damage;
		this.firerate = firerate;
		this.reload = reload;
		this.accuracy = accuracy;
		this.recoil = recoil;
		this.range = range;
		this.magsize = magsize;
		this.company = company;
		this.gunType = type;
		this.partType = partType;
	}
	
	public void InitGunPart(int level) {
		itemType = ItemType.GunPart;
		//create random gun part based on level
		gunType = GetRandomEnum<GunType>();
		partType = GetRandomEnum<PartType>();
		this.level = level;
		
		float r = UnityEngine.Random.value;
		if(r < .2)
			company = GunCompany.Isoid;
		else if(r < .4)
			company = GunCompany.Nomina;
		else if(r < .6)
			company = GunCompany.Rogue;
		else if(r < .75)
			company = GunCompany.Sterns;
		else if(r < .9)
			company = GunCompany.Mystica;
		else if(r < .95)
			company = GunCompany.JCorp;
		else
			company = GunCompany.VTech;
		
		damage = (int)(UnityEngine.Random.value * level);
		firerate = (int)(UnityEngine.Random.value * level);
		reload = (int)(UnityEngine.Random.value * level);
		accuracy = (int)(UnityEngine.Random.value * level);
		recoil = (int)(UnityEngine.Random.value * level);
		range = (int)(UnityEngine.Random.value * level);
		magsize = (int)(UnityEngine.Random.value * level);
	}
	
	public void InitGunPartOfType(int level, int type = -1, int gt = -1) {
		itemType = ItemType.GunPart;
		//create random gun part based on level
		if(gt < 0)
			gunType = GetRandomEnum<GunType>();
		else
			gunType = (GunType)gt;
		if(type < 0)
			partType = GetRandomEnum<PartType>();
		else
			partType = (PartType)type;
		this.level = level;
		
		float r = UnityEngine.Random.value;
		if(r < .2)
			company = GunCompany.Isoid;
		else if(r < .4)
			company = GunCompany.Nomina;
		else if(r < .6)
			company = GunCompany.Rogue;
		else if(r < .75)
			company = GunCompany.Sterns;
		else if(r < .9)
			company = GunCompany.Mystica;
		else if(r < .95)
			company = GunCompany.JCorp;
		else
			company = GunCompany.VTech;
		
		damage = (int)(UnityEngine.Random.value * level);
		firerate = (int)(UnityEngine.Random.value * level);
		reload = (int)(UnityEngine.Random.value * level);
		accuracy = (int)(UnityEngine.Random.value * level);
		recoil = (int)(UnityEngine.Random.value * level);
		range = (int)(UnityEngine.Random.value * level);
		magsize = (int)(UnityEngine.Random.value * level);
	}
	
	public void InitGunPartOfCompany(int level, int c = -1, int type = -1, int gt = -1) {
		itemType = ItemType.GunPart;
		//create random gun part based on level
		if(gt < 0)
			gunType = GetRandomEnum<GunType>();
		else
			gunType = (GunType)gt;
		if(c < 0)
			company = GetRandomEnum<GunCompany>();
		else
			company = (GunCompany)c;
		
		if(type < 0)
			partType = GetRandomEnum<PartType>();
		else
			partType = (PartType)type;
		
		this.level = level;

		damage = (int)(UnityEngine.Random.value * level);
		firerate = (int)(UnityEngine.Random.value * level);
		reload = (int)(UnityEngine.Random.value * level);
		accuracy = (int)(UnityEngine.Random.value * level);
		recoil = (int)(UnityEngine.Random.value * level);
		range = (int)(UnityEngine.Random.value * level);
		magsize = (int)(UnityEngine.Random.value * level);
	}
	
	//attachment constructor
	public void InitAttachment(int level, int damage, int firerate, int reload, int accuracy, int recoil, int range, int magsize, GunCompany company) {
		itemType = ItemType.Attachment;
		this.level = level;
		this.damage = damage;
		this.firerate = firerate;
		this.reload = reload;
		this.accuracy = accuracy;
		this.recoil = recoil;
		this.range = range;
		this.magsize = magsize;
		this.company = company;
	}
	
	//helper functions:
	
	public static byte[] SerializeObject<T>(T objectToSerialize) {
		BinaryFormatter bf = new BinaryFormatter();
		MemoryStream memStr = new MemoryStream();

		bf.Serialize(memStr, objectToSerialize);
		memStr.Position = 0;

		//return "";
		return memStr.ToArray();
	}
	
	static T GetRandomEnum<T>() {
		System.Array A = System.Enum.GetValues(typeof(T));
		T V = (T)A.GetValue(UnityEngine.Random.Range(0, A.Length));
		return V;
	}
	
	public static T DeepClone<T>(T obj) {
		using(var ms = new MemoryStream()) {
			var formatter = new BinaryFormatter();
			formatter.Serialize(ms, obj);
			ms.Position = 0;

			return (T)formatter.Deserialize(ms);
		}
	}
}

public enum ItemType {
	Item,
	GunPart,
	Gun,
	Ammo,
	DuctTape,
	Attachment
}

public enum Rarity {
	common,
	uncommon,
	valuable,
	rare,
	epic,
	unique
};

public enum GunType {
	Pistol,
	Revolver,
	SMG,
	AssultRifle,
	Shotgun,
	Sniper,
	Launcher
};

public enum GunCompany {
	Isoid,
	Nomina,
	Rogue,
	Sterns,
	Mystica,
	JCorp,
	VTech
};

public enum AmmoType {
	Pistol,
	Revolver,
	Smg,
	AssultRifle,
	Shotgun,
	Sniper,
	Launcher
};

public enum PartType {
	Clip,
	Body,
	Barrel
};

public enum Aura {
	None,
	Dark,//effect: think enderman particles - Dot and slow
	Light,//opposite of ^ - blind (slow and take additional damage from all sources while active)
	Fire,//it is on fire - heavy DoT
	Electric,// - chance to chain damage, higher chain depending on rarity
	Ice,//slow icy snowfall around it - severe slow
	Poison//dripping green - slow DoT but very high amount
};

public enum SpecialEffects {
	InfiniteAmmo,
	NoReload
};

sealed class VersionFixer : SerializationBinder {
	public override Type BindToType(string assemblyName, string typeName) {
		Type typeToDeserialize = null;

		// For each assemblyName/typeName that you want to deserialize to
		// a different type, set typeToDeserialize to the desired type.
		String assemVer1 = Assembly.GetExecutingAssembly().FullName;

		if(assemblyName != assemVer1) {
			// To use a type from a different assembly version, 
			// change the version number.
			// To do this, uncomment the following line of code.
			assemblyName = assemVer1;

			// To use a different type from the same assembly, 
			// change the type name.
		}

		// The following line of code returns the type.
		typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));

		return typeToDeserialize;
	}
}