using System.Collections;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel<T> : Singleton<T> where T : UIPanel<T>
    {
        public bool Visible = false;
        System.Guid lockID;
        Coroutine coroutineShow;
        Coroutine coroutineHide;
        protected virtual void Update()
        {
        }

        public virtual IEnumerator Show(float time = .1f)
        {
            if (Visible)
                yield break;
            if (coroutineHide != null)
                StopCoroutine(coroutineHide);
            coroutineHide = null;
            Visible = true;

            GetComponent<CanvasGroup>().alpha = 0;
            yield return Utility.ShowUI(GetComponent<CanvasGroup>(), time);
        }


        public virtual IEnumerator Hide(float time = .2f)
        {
            if (!Visible)
                yield break;
            if (coroutineShow != null)
                StopCoroutine(coroutineShow);
            coroutineShow = null;
            Visible = false;

            if (gameObject.activeInHierarchy)
            {
                yield return Utility.HideUI(GetComponent<CanvasGroup>(), time);
            }
        }
        public void HideAsync(float time = .2f)
        {
            StartCoroutine(Hide(time));
        }

        public void ShowAsync(float time = .1f)
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
            StartCoroutine(Show(time));
        }
    }
}