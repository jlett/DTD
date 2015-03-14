using UnityEngine;
using System.Collections;

public class particleController : MonoBehaviour {
	//this class is used to say how the particle system will be used, controls: colors used, self-destruction timer, size, direction, shape
	public float duration;
	public Texture2D colors;

	void Start() {
		transform.rotation = Quaternion.Euler(270, 0, 0);
	}

	public void Init(float d = -1, Texture2D t = null) {
		duration = d;
		colors = t;

		if(colors != null) {
			//todo
		}
	}

	void Update() {
		if(duration == -1)
			return;
		else if(duration > 0) {
			duration -= Time.deltaTime;
		} else if(transform.GetComponent<ParticleSystem>().emissionRate != 0) {
			transform.GetComponent<ParticleSystem>().emissionRate = 0f;
			duration += transform.GetComponent<ParticleSystem>().startLifetime;
		} else {
			Destroy(gameObject);
		}
	}
}