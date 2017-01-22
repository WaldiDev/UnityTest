using UnityEngine;
using System.Collections;

public class TileTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    
    void OnTriggerEnter(Collider col)
    {
        GameObject.Find("SceneManager").SendMessage("TileTriggered");
    }
}
