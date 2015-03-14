using UnityEngine;
using System.Collections;

public class ItemPreviewRenderer : MonoBehaviour {

	public float speed = 10f;
	new public GameObject camera;

	// Use this for initialization
	void Start () {
		transform.position.Set(0, -1000, 0);
	}

	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(0, -1000, 0) - GetComponent<Rigidbody>().centerOfMass;
		camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, camera.transform.rotation * Quaternion.Euler(new Vector3(0, 10, 0)), speed * Time.deltaTime);
	}
}
