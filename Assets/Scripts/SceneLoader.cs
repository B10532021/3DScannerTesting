using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private int currentScene;

    static Stack<int> sceneStack = new Stack<int>();

    void Awake()
    {
        currentScene = SceneManager.GetActiveScene().buildIndex;
    }

    void Update()
    {
        BackButtonPressed();
    }
    
    void BackButtonPressed()
    {
        if (Input.GetKey(KeyCode.Escape) && sceneStack.Count != 0)
        {
            LoadLastScene();
        }
    }

    public void LoadNewScene(int sceneToLoad)
    {
        currentScene = SceneManager.GetActiveScene().buildIndex;
        sceneStack.Push(currentScene);
        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoadNewScene(string sceneToLoad)
    {
        currentScene = SceneManager.GetActiveScene().buildIndex;
        sceneStack.Push(currentScene);
        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoadLastScene()
    {
        var lastScene = sceneStack.Pop();
        SceneManager.LoadScene(lastScene);
    }
}
