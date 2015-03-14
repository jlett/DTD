using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ObjectLabelFade : MonoBehaviour {
	
	public float timeToFade = 2.0f;
	public float verticalRise = 20.0f;
	public GUIText textLabel;
	public ObjectLabel objectLabel;
	
	// Use this for initialization
	void Start () {
		HOTween.To(objectLabel, timeToFade, 
			new TweenParms().Prop("offset", objectLabel.offset + new Vector3(Random.Range(-5.0f, 5.0f), verticalRise + Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f))).OnComplete(onCompletion));
	}
	
	public void onCompletion() {
		Destroy(gameObject);
	}
}
