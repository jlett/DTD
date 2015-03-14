using UnityEngine;
using System.Collections;

public class ShockwaveController : MonoBehaviour {

	public float damage;
	public float range;//end radius in unity units
	public float speed;// 1/_ seconds to grow by 1 unit
	public AnimationCurve damageCurve;

	private float curPercent = 0f;
	private float startingAlpha;

	void Start() {
		startingAlpha = GetComponent<Renderer>().material.GetColor("_TintColor").a;
		object[] data = PhotonView.Get(gameObject).instantiationData;
		Init((float)data[0], (float)data[1], (float)data[2], (AnimationCurve)data[3]);
	}
	
	public void Init(float d = 0f, float r = 1f, float s = 1f, AnimationCurve c = null) {
		damage = d;
		range = r;
		speed = s;
		if(c != null)
			damageCurve = c;
	}

	void Update () {
		curPercent += Time.deltaTime*speed/range;
		if(curPercent > 1)
			Destroy(gameObject);
		transform.localScale = new Vector3(curPercent*range*2f, .01f, curPercent*range*2f);
		if(curPercent > .5) {
			Color c = GetComponent<Renderer>().material.GetColor("_TintColor");
			c.a = startingAlpha - ((curPercent - .5f)*2f*startingAlpha);
			GetComponent<Renderer>().material.SetColor("_TintColor", c);
		}
	}

	void OnTriggerEnter(Collider other) {
		if(other.gameObject.tag.Equals("Enemy") && other.gameObject.GetComponent<Health>() != null) {
			other.gameObject.GetComponent<Health>().Damage((int)(damage*(1f - damageCurve.Evaluate(curPercent))));
		}
	}
}
