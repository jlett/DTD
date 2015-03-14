using UnityEngine;
using System.Collections;
using RAIN.Core;

public class Health : MonoBehaviour {
	
	public int maxHealth = 1;
	public int curHealth = 1;
	public int armor = 0;
	public float curShield = 0;
	public int maxShield = 0;
	public float shieldRegenRate = 0;

	private AIRig aiRig = null;

	void Awake() {
		aiRig = gameObject.GetComponentInChildren<AIRig>();
	}

	void Update() {
		if(aiRig != null)
			aiRig.AI.WorkingMemory.SetItem("health", curHealth);


		//shield regen
		if(shieldRegenRate == 0 || (shieldRegenRate < 0 && curShield <= 0) || (shieldRegenRate > 0 && curShield >= maxShield)) {
			return;
		}
		curShield += shieldRegenRate * Time.deltaTime;
	}

	public void Damage(int d, bool damageText = true) {
		bool tookHealthDamage = false;
		if(curShield >= d) {
			curShield -= d;
		} else if(curShield > 0) {
			curShield = 0;
			curHealth -= (d - (int)curShield);
			tookHealthDamage = true;
		} else {
			curHealth -= d;
			tookHealthDamage = true;
		}

		//make damage text
		if(damageText && gameObject.GetComponent<MakesDamageText>() != null) {
			gameObject.GetComponent<MakesDamageText>().TookDamage(d, tookHealthDamage);
		}
		if(curHealth <= 0) {
			gameObject.SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Heal(int h) {
		if(curHealth == maxHealth) {
			return;
		}
		curHealth += h;
		if(curHealth > maxHealth) {
			curHealth = maxHealth;
		}
	}

	public void HealShield(int h) {
		if(curShield == maxShield) {
			return;
		}
		curShield += h;
		if(curShield > maxShield) {
			curShield = maxShield;
		}
	}
}
