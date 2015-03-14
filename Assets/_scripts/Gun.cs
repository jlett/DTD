using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Gun : MonoBehaviour {
	
	public Item gun;
	public bool canShoot = true;
	private float cooldownTimer = 0, recoilMod = 1;
	public float reloadTimer = 0, reloadPer = 1;//public so that playerstats can make reload bar animation
	public LayerMask layerMask = -1;
	
	void Start() {
		gun = transform.parent.GetComponent<WeaponManager>().equiped[0];
		if(gun == null || gun.itemType != ItemType.Gun) {
			gun = new Item("Error", Rarity.unique, "Congrats, you broke something");
			gun.InitGun(1);
		}
		PhotonView.Get(this).RPC("InitGun", PhotonTargets.OthersBuffered, Item.SerializeObject<Item>(gun));
		GetComponentInChildren<ItemRenderer>().item = gun;
	}
	
	void Update() {
		//cant shoot situations
		if(cooldownTimer > 0) {
			canShoot = false;
			cooldownTimer -= Time.deltaTime;//goes down by one per second
		} else if(reloadTimer > 0 && gun.bullets == 0) {
			canShoot = false;
			reloadTimer -= Time.deltaTime;//goes down by one per second
			if(reloadTimer <= 0) {
				gun.bullets = gun.magsize;
			} else {
				float maxReload = 50.0f / gun.reload;
				reloadPer = 1 - (reloadTimer/maxReload);//visual "filling up" status bar
			} 
		} else if(!canShoot) {
			canShoot = true;
		}

		 

		if(recoilMod > 1) 
			recoilMod -= Time.deltaTime * recoilMod * .4f;
		else
			recoilMod = 1;
	}
	
	public void Shoot() {
		Vector3 bulletSpawnPoint = transform.TransformPoint(new Vector3(gun.bulletSpawnX, gun.bulletSpawnY, gun.bulletSpawnZ) * .1f);
		string projectileName = "_prefabs/projectiles/";
		if(gun.ammoType == AmmoType.Launcher) {
			projectileName += "LauncherProjectile";
		} else {
			projectileName += "Projectile";
		}
		if(canShoot && gun.bullets > 0 && !Physics.Linecast(transform.parent.parent.position, bulletSpawnPoint, layerMask)) {
			Quaternion randomRotation = Quaternion.Euler( 0, Random.Range(100f/gun.accuracy + recoilMod/10f, -(100f/gun.accuracy) - recoilMod/10f), 0);
			if(gun.pellets > 0) {
				for(int i = 0; i < gun.pellets; i++) {
					Quaternion randomRotationSpread = Quaternion.Euler(0, randomRotation.eulerAngles.y + Random.Range(-(gun.spread/2), gun.spread/2), 0);
					GameObject projectile = PhotonNetwork.Instantiate(projectileName, bulletSpawnPoint, transform.rotation * randomRotationSpread, 0);
					projectile.GetComponent<Projectile>().damage = gun.damage;
					projectile.GetComponent<Projectile>().speed = gun.bulletSpeed;
					if(gun.explosionRadius > 0) {
						projectile.GetComponent<Projectile>().explosive = true;
						projectile.GetComponent<Projectile>().explosionRadius = gun.explosionRadius;
					}
				}
			} else {
				GameObject projectile = PhotonNetwork.Instantiate(projectileName, bulletSpawnPoint, transform.rotation * randomRotation, 0);
				projectile.GetComponent<Projectile>().damage = gun.damage;
				projectile.GetComponent<Projectile>().speed = gun.bulletSpeed;
				if(gun.explosionRadius > 0) {
					projectile.GetComponent<Projectile>().explosive = true;
					projectile.GetComponent<Projectile>().explosionRadius = gun.explosionRadius;
				}
			}
			gun.bullets--;
			if(gun.bullets <= 0) {
				reloadTimer = 50.0f / gun.reload;
				reloadPer = 0;
				StartCoroutine(Camera.main.GetComponent<ShakeManager>().StopShake());
			} else {
				cooldownTimer = 10.0f / gun.firerate;
			}
			canShoot = false;

			//play sound TODO - make sound different depending on gun
			AudioSource.PlayClipAtPoint(Resources.Load("_sounds/MechWeapons_Pulse_Quick_01") as AudioClip, transform.position);

			//shake screen TODO - make intensity/duration different depending on gun
			if(gun.firerate > 15)
				Camera.main.GetComponent<ShakeManager>().StartShake(gun.projectileSize);
			else
				Camera.main.GetComponent<ShakeManager>().StartPunch(gun.projectileSize);
		}
		if(recoilMod < gun.recoil)
			recoilMod += gun.recoil * .01f;
	}
	
	public void SwapItem(Item item) {
		gun = item;
		GetComponentInChildren<ItemRenderer>().item = gun;
		GetComponentInChildren<ItemRenderer>().RemakeMesh();
		if(gun.bullets <= 0) {
			reloadTimer = 50.0f / gun.reload;
			reloadPer = 0f;
			StartCoroutine(Camera.main.GetComponent<ShakeManager>().StopShake());
		} else {
			reloadTimer = reloadPer = 0f;
		}
	}
	
	[RPC]
	void InitGun(byte[] dataStream) {
		MemoryStream stream = new MemoryStream(dataStream);
		stream.Position = 0;
		BinaryFormatter bf = new BinaryFormatter();
		bf.Binder = new VersionFixer();
		Item item = (Item)bf.Deserialize(stream);
		
		gun = item;
		GetComponentInChildren<ItemRenderer>().item = gun;
		GetComponentInChildren<ItemRenderer>().RemakeMesh();
	}
}