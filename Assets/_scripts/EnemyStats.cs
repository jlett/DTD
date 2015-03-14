using UnityEngine;
using System.Collections;
using RAIN.Core;

public class EnemyStats : MonoBehaviour {

	public int damage = 0;

	PhotonView photonView;
	private AIRig aiRig = null;
	private RAIN.Memory.RAINMemory memory;
	/*memory for all ai stores:
	 * reference to each player gameobject
	 * distance to player (for each player)
	 * reference to closest player
	 * distance to closest player
	 * reference to target player
	 * distance to target player
	 * 
	 * self-health (handled in health component)
	*/
	
	void Awake() {
		aiRig = gameObject.GetComponentInChildren<AIRig>();
		memory = aiRig.AI.WorkingMemory;
	}

	void Start () {
		photonView = PhotonView.Get(gameObject);
		if(photonView.instantiationData != null) {
			object[] data = photonView.instantiationData;
			damage = (int)data[0];
		}
		memory.SetItem("test", 42);
	}
}
