using UnityEngine;
using System.Collections;
using Holoville.HOTween;
public class TextFade : MonoBehaviour {

	public float timeToFade = 0.5f;
	public float verticalRise = 4.0f;
	public TextMesh textMesh;
	
	// Use this for initialization
	void Start () {
		HOTween.To(transform, timeToFade, 
			new TweenParms().Prop("position", transform.position + new Vector3(Random.Range(-1.0f, 1.0f), verticalRise + Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f))).OnComplete(onCompletion));
	}
	
	public void onCompletion() {
		Destroy(gameObject);
	}
}
