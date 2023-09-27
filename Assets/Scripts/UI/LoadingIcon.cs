using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.UI
{
    using CityJsonEvents = EventMessaging.Registry.CityJson;

    /// <summary>
    /// Animates the loading icon, and deactivates itself when <see cref="LoadCityJson"/>
    /// has finished loading
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LoadingIcon : MonoBehaviour
    {
        private Image icon;

        private void Awake()
        {
            icon = GetComponent<Image>();
        }

        private void Start()
        {
            icon.transform
                .DORotate(new Vector3(0, 0, -360), 1)
                .SetLoops(-1)
                .SetEase(Ease.Linear)
                .SetRelative(true);

            CityJsonEvents.OnLoadedAllHdbBlocks += OnLoadedCityJson;
        }

        private void OnLoadedCityJson(object _)
        {
            gameObject.SetActive(false);
            icon.transform.DOKill();
        }
    }
}