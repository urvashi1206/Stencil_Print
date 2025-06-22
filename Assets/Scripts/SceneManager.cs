using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneResetter : MonoBehaviour
{
    public string labScene;
    void Start()
    {
        SceneManager.LoadScene(labScene, LoadSceneMode.Additive);
    }

    void Update()
    {
        // Press Alt & R to restart the current scene
        if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftAlt))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
