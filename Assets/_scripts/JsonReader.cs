using SimpleJSON;
using UnityEngine;

public class JsonReader {
	static JSONNode N;
	public JsonReader() {
		TextAsset t = (TextAsset)Resources.Load("gunData", typeof(TextAsset));
		N = JSON.Parse(t.text);
	}
	
	public static string PartName(GunCompany c, GunType t, PartType pt) {
		int i = N["gunParts"][(int)t][t.ToString().ToLower()][(int)c][pt.ToString().ToLower()]["name"].Count;
		if(pt == PartType.Clip && Random.Range(0, 2) == 0) //clip applys prefix only half the time
			return "";
		else 
			return N["gunParts"][(int)t][t.ToString().ToLower()][(int)c][pt.ToString().ToLower()]["name"][Random.Range(0, i)] + (pt==PartType.Clip ? " " : "");
	}
}