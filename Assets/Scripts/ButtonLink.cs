using UnityEngine;
using UnityEngine.SceneManagement; // for scene management

public class Cardscanner : MonoBehaviour
{
    public string sceneToLoad; // assign the scene name in the inspector
    public void LoadScene()
    {
        Debug.Log("Loading scene: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}