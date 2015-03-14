using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ShakeManager : MonoBehaviour {
	//the shake should be directional so that it goes in direction of recoil

	bool shaking = false;
	float q = 0;

	void Update() {
		q = GameObject.FindGameObjectWithTag("MyPlayer").transform.rotation.eulerAngles.y;
		q = 360-q;//b/c it needs to go counter clockwise
		//60 = facing right, 150 = down, etc
		q -= 300;
		if(q < 0) {
			q = 360 + q;
		}
		q *= Mathf.Deg2Rad;
	}
	
	public void StartShake(float intensity) {
		if(shaking)
			return;
		else
			shaking = true;

		Hashtable ht = new Hashtable();
		ht.Add("amount", new Vector3(1,1,0) * intensity/4);
		ht.Add("islocal", true);
		ht.Add("time", 1000000);
		iTween.ShakePosition(gameObject, ht);
	}

	public void StartPunch(float intensity) {
		Hashtable ht = new Hashtable();
		ht.Add("amount", new Vector3(Mathf.Cos(q),Mathf.Sin(q),0) * intensity);
		ht.Add("islocal", true);
		ht.Add("time", .5f);
		iTween.PunchPosition(gameObject, ht);
	}

	public IEnumerator StopShake() {
		if(!shaking)
			yield break;

		yield return new WaitForSeconds(.3f);
		iTween.Stop(gameObject);
		shaking = false;
		transform.localPosition = Vector3.zero;
	}
}