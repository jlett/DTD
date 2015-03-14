using UnityEngine;
using System.Collections;

public class ConsoleManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		ConsoleCommandsRepository repo = ConsoleCommandsRepository.Instance;
		repo.RegisterCommand("help", Help);
		repo.RegisterCommand("ping", Ping);
		repo.RegisterCommand("spawnGun", SpawnGun);
	}

	public string Help(params string[] args) {
		return "fuck you, figure it out on your own";
	}

	public string Ping(params string[] args) {
		return "pong";
	}

	public string SpawnGun(params string[] args) {
		//args[0] = level
		//args[1] = type
		//args[2] = company
		//args[3] = name?

		//args value of -1 or unknown = random
		Item gun = new Item();
		int level = 0;
		int gt = -1;
		int c = -1;
		if(args.Length > 0) {
			level = int.Parse(args[0]) > 0 ? int.Parse(args[0]) : Random.Range(1, 100);
			if(args.Length > 1) {
				if(args[1].ToLower().Equals("pistol")) gt = 0;
				else if(args[1].ToLower().Equals("revolver")) gt = 1;
				else if(args[1].ToLower().Equals("smg")) gt = 2;
				else if(args[1].ToLower().Equals("assaultrifle")) gt = 3;
				else if(args[1].ToLower().Equals("shotgun")) gt = 4;
				else if(args[1].ToLower().Equals("sniper")) gt = 5;
				else if(args[1].ToLower().Equals("launcher")) gt = 6;
				if(args.Length > 2) {
					if(args[2].ToLower().Equals("isoid")) c = 0;
					else if(args[2].ToLower().Equals("nomina")) c = 1;
					else if(args[2].ToLower().Equals("rogue")) c = 2;
					else if(args[2].ToLower().Equals("sterns")) c = 3;
					else if(args[2].ToLower().Equals("mystica")) c = 4;
					else if(args[2].ToLower().Equals("jcorp")) c = 5;
					else if(args[2].ToLower().Equals("vtech")) c = 6;
					if(args.Length > 3) {

					} else {
						Item[] parts = new Item[3];
						for(int i = 0; i < 3; i++) {
							Item it = new Item();
							it.InitGunPartOfCompany(level, c, i, gt);
							parts[i] = it;
						}
						gun.InitGun(parts);
					}
				} else {
					Item[] parts = new Item[3];
					for(int i = 0; i < 3; i++) {
						Item it = new Item();
						it.InitGunPartOfType(level, i, gt);
						parts[i] = it;
					}
					gun.InitGun(parts);
				}
			} else {
				gun.InitGun(level);
			}
		} else {
			gun.InitGun(Random.Range(1, 100));
		}
		GameObject.FindGameObjectsWithTag("MyPlayer")[0].GetComponent<PlayerStats>().AddItem(gun);
		return "spawned gun";
	}
}
