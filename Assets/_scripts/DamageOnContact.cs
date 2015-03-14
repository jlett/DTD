using UnityEngine;
using System.Collections;

public class DamageOnContact : MonoBehaviour {
	
	public int damage = 0;
	public bool active = false;

	void OnTriggerEnter(Collider c) {
		if(active && c.tag == "MyPlayer") {
			c.gameObject.GetComponent<Health>().Damage(transform.parent.GetComponent<EnemyStats>().damage);
		}
	}
}
