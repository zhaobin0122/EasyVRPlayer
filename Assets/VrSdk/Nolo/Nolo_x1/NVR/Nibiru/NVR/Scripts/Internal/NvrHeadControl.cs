using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Nvr.Internal
{
    public class NvrHeadControl : MonoBehaviour
    {
        [NonSerialized]
        public static GameObject eventGameObject;
        [NonSerialized]
        public static BaseEventData baseEventData;

        private float duration = 2f;
        [SerializeField] private Image selection;

        private Coroutine fillcoroutine;
        private bool isselect;
        private bool isfilled;


        public float Duration { get { return duration; } }

        private void Start()
        {
            selection.fillAmount = 0f;
            duration = NvrViewer.Instance.Duration;
        }


        public void Show()
        {
            selection.gameObject.SetActive(true);
            isselect = true;
        }


        public void Hide()
        {
            selection.gameObject.SetActive(false);
            isselect = false;

            // This effectively resets the radial for when it's shown again.
            selection.fillAmount = 0f;
        }
 
        private IEnumerator FillSelectionRadial()
        {
            isfilled = false;

            float timer = 0f;
            selection.fillAmount = 0f;
            while (timer < duration)
            {
                selection.fillAmount = timer / duration;
                timer += Time.deltaTime;
                yield return null;
            }
            selection.fillAmount = 1f;
            isselect = false;
            isfilled = true;
            if (eventGameObject != null)
            {
                ExecuteEvents.ExecuteHierarchy(eventGameObject, baseEventData, ExecuteEvents.pointerClickHandler);
            }
        }


        public IEnumerator WaitForSelectionRadialToFill()
        {
            isfilled = false;
            Show();
            while (!isfilled)
            {
                yield return null;
            }
            Hide();
        }


        public void HandleDown()
        {
            if (isselect)
            {
                fillcoroutine = StartCoroutine(FillSelectionRadial());
            }
        }


        public void HandleUp()
        {
            if (fillcoroutine != null)
                StopCoroutine(fillcoroutine);
            selection.fillAmount = 0f;
        }
    }
}
