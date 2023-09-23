using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GeorgeChew.HiverlabAssessment.UI
{
    using CityJsonEvents = EventMessaging.Registry.CityJson;

    /// <summary>
    /// Animates the loading icon, and deactivates itself when <see cref="LoadCityJson"/> 
    /// has finished loading
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LoadingIcon : MonoBehaviour
    {
        Image icon;

        private void Awake()
        {
            icon = GetComponent<Image>();
        }

        void Start()
        {
            icon.transform
                .DORotate(new Vector3(0, 0, -360), 1)
                .SetLoops(-1)
                .SetEase(Ease.Linear)
                .SetRelative(true);

            CityJsonEvents.OnLoadedAllData += OnLoadedCityJson;
        }

        void OnLoadedCityJson(object _)
        {
            gameObject.SetActive(false);
            icon.transform.DOKill();
        }
    }
}