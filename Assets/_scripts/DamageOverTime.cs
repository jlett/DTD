using UnityEngine;
using System.Collections;

public class DamageOverTime : MonoBehaviour {

	public float damage = 0f;
	public float time = 1f;//in seconds

	void Start() {
		//update the damage every quarter second
		StartCoroutine("DoDamage");
	}

	public IEnumerator DoDamage() {
		for(int i = 0; i < time*4; i++) {
			float damageThisTick = damage/(time*4);
			gameObject.GetComponent<Health>().Damage((int)damageThisTick);

			yield return new WaitForSeconds(.25f);
		}
	}
}
