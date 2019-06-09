using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace UI
{
    public class Startup : MonoBehaviour
    {
        public string SceneToLoad;
        public CoveredUI StartupUI;
        public CoveredUI Mask;
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
            yield return Mask.Show(.5f);
            yield return new WaitForSeconds(.5f);
            StartupUI.gameObject.SetActive(false);
            yield return SceneManager.LoadSceneAsync(SceneToLoad, LoadSceneMode.Single);
            yield return Mask.Hide(.5f);
        }
    }

}
