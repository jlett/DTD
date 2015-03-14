using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
	
	public float damage = 10f, speed = 500f, explosionRadius = 20f, explosionSpeed = 40f;
	public bool explosive = false;
	public AnimationCurve rangeDamageMultipliers;
	public GunType ammoType = GunType.Pistol;
	
	void Start() {
		if(PhotonView.Get(gameObject).isMine)
			StartCoroutine(destroyTimer());
		else
			GetComponent<BoxCollider>().enabled = false;
	}
	
	IEnumerator destroyTimer() {
		yield return new WaitForSeconds(7);
		PhotonNetwork.Destroy(PhotonView.Get(gameObject));
	}
	
	// Update is called once per frame
	void Update() {
		if(PhotonView.Get(this).isMine)
			transform.position += transform.forward * speed * Time.deltaTime;
	}
	
	void OnCollisionEnter(Collision collision) {
		if(PhotonView.Get(gameObject).isMine) {
			if(explosive)
				PhotonView.Get(gameObject).RPC("MakeShockwave", PhotonTargets.MasterClient);
			else if(collision.gameObject.GetComponent<Health>() != null)
				collision.gameObject.GetComponent<Health>().Damage((int)damage);
			PhotonNetwork.RemoveRPCs(gameObject.GetComponent<PhotonView>().owner);
			PhotonNetwork.Destroy(PhotonView.Get(gameObject));
		}
	}
	[RPC]
	void MakeShockwave() {
		object[] data = new object[4];
		data[0] = damage;
		data[1] = explosionRadius;
		data[2] = explosionSpeed;
		data[3] = null; //can specifically set damage falloff curve if needed, null gives default inspector-set one.
		PhotonNetwork.InstantiateSceneObject("_prefabs/Shockwave", gameObject.transform.position, Quaternion.identity, 0, data);
	}
}
