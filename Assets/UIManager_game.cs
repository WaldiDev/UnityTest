using UnityEngine;
using System.Collections;

public class UIManager_game : MonoBehaviour {

    GameObject canvas;

    float time = 0.0f;
    float inputDelay = 0.5f;
    float lastInput = 0.0f;

    // Use this for initialization
    void Start () {
        canvas = GameObject.Find("Menu");
        canvas.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;
        if (time - lastInput > inputDelay)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                lastInput = time;
                Time.timeScale = 0;
                canvas.SetActive(true);
            }
        }
    }

    public void Resume()
    {
        Time.timeScale = 1;
        canvas.SetActive(false);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("mainmenu");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
