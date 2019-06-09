using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UI;

public class MainUI : Singleton<MainUI>
{
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
        yield return SceneManager.LoadSceneAsync(Level.Instance.NextScene, LoadSceneMode.Single);
        yield return BlackScreen.Hide(.5f);
        //yield return new WaitForSeconds(2);
        //yield return Fail();
        /*
        yield return new WaitForSeconds(4);
        yield return Pass();*/
    }

    public void Fail()
    {
    }

    public IEnumerator FailProcess()
    {
        yield return GameFail.Show(.5f);
        yield return new WaitForSeconds(1.5f);

        yield return BlackScreen.Show(.5f);
        //yield return new WaitForSeconds(.5f);
        yield return GameFail.Hide(.5f);

        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return new WaitForSeconds(1);
        yield return SceneManager.LoadSceneAsync(Level.Instance.NextScene, LoadSceneMode.Single);
        yield return BlackScreen.Hide(.5f);
    }

    public void Pass()
    {
        if (!start)
            return;
        start = false;
        StartCoroutine(PassProcess());
    }

    public IEnumerator PassProcess()
    {
        yield return GamePass.Show(.5f);
        yield return new WaitForSeconds(1.5f);

        yield return WhiteScreen.Show(.5f);
        //yield return new WaitForSeconds(.5f);
        yield return GamePass.Hide(.5f);

        //yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return new WaitForSeconds(1);
        yield return SceneManager.LoadSceneAsync(Level.Instance.NextScene, LoadSceneMode.Single);
        yield return WhiteScreen.Hide(.5f);
        start = true;
    }
}
