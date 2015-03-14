using UnityEngine;
using System.Collections;

public class MakesDamageText : MonoBehaviour {
	
	public Vector3 textOffset = Vector3.zero;
	
	public void TookDamage(int d, bool tookHealthDamage) {
		string color = "<color=cyan>";
		if(tookHealthDamage) {
			color = "<color=red>";
		}
		PhotonView.Get(this).RPC("MakeText", PhotonTargets.All, color + d.ToString() + "</color>");
	}
	
	[RPC]
	void MakeText(string s) {
		GameObject g = Instantiate(Resources.Load("_prefabs/3dText"), transform.position + textOffset, Quaternion.identity) as GameObject;
		g.GetComponent<TextMesh>().text = s;
	}
}
