using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager_mainmenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void StartGame()
    {
        InputField f = GameObject.Find("SeedInput").GetComponent<InputField>();
        StaticObjectScript.seed = int.Parse(f.text);
        UnityEngine.SceneManagement.SceneManager.LoadScene("game");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
