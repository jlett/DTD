using UnityEngine;
using System.Collections;

public class TextFaceCamera : MonoBehaviour {
	
	public bool yRotOnly = true; //should rotate in all axis or just Y?
	new public Camera camera;
	
	void Start() {
		if(camera == null) {
			camera = Camera.main;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		Vector3 lookat = camera.transform.rotation * Vector3.forward;
		Quaternion lookRot = Quaternion.LookRotation(lookat, camera.transform.rotation * Vector3.up);
		
		if(yRotOnly) {
			lookRot.x = transform.rotation.x;
			lookRot.z = transform.rotation.z;
		}
		
		transform.rotation = lookRot;
	}
}
