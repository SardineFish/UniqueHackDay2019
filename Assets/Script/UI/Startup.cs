using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Startup : MonoBehaviour
{
    public string SceneToLoad;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(InputUtils.GetMainKey())
        {
            SceneManager.LoadScene(SceneToLoad, LoadSceneMode.Single);
        }
    }
}
