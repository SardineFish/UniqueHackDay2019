using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UI;

public class MainUI : Singleton<MainUI>
{
    public string SceneToLoad;
    public string Credit;
    public CoveredUI StartupUI;
    public CoveredUI BlackScreen;
    public CoveredUI WhiteScreen;
    public CoveredUI GamePass;
    public CoveredUI GameFail;
    bool start = false;
    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (InputUtils.GetMainKey())
        {
            if (start)
                return;
            start = true;
            StartCoroutine(StartProcess());
        }
    }

    IEnumerator StartProcess()
    {
        yield return BlackScreen.Show(.5f);
        yield return new WaitForSeconds(.5f);
        StartupUI.gameObject.SetActive(false);
        yield return SceneManager.LoadSceneAsync(SceneToLoad, LoadSceneMode.Single);
        yield return BlackScreen.Hide(.5f);
        yield return new WaitForSeconds(2);
        yield return Fail();
        yield return new WaitForSeconds(4);
        yield return Pass();
    }

    public IEnumerator Fail()
    {
        yield return GameFail.Show(.5f);
        yield return new WaitForSeconds(1.5f);

        yield return BlackScreen.Show(.5f);
        //yield return new WaitForSeconds(.5f);
        yield return GameFail.Hide(.5f);

        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return new WaitForSeconds(1);
        yield return SceneManager.LoadSceneAsync(SceneToLoad, LoadSceneMode.Single);
        yield return BlackScreen.Hide(.5f);
    }

    public IEnumerator Pass()
    {
        yield return GamePass.Show(.5f);
        yield return new WaitForSeconds(1.5f);

        yield return WhiteScreen.Show(.5f);
        //yield return new WaitForSeconds(.5f);
        yield return GamePass.Hide(.5f);

        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return new WaitForSeconds(1);
        yield return SceneManager.LoadSceneAsync(Credit, LoadSceneMode.Single);
        yield return BlackScreen.Hide(.5f);
    }
}
