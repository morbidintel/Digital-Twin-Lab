using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.UI
{
    using Events = EventMessaging.Registry.UI;

    /// <summary>
    /// Handles animation of left panel tabs
    /// </summary>
    public class LeftPanelMovement : MonoBehaviour
    {
        private readonly float tweenDuration = 0.5f;

        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private List<LeftPanelTab> tabs;

        private RectTransform rectTransform;
        private float panelWidth;

        private LeftPanelTab currentlyOpenedTab;

        private void Awake()
        {
            Assert.IsNotNull(toggleGroup);
            Assert.IsTrue(tabs.Count > 0);
        }

        // Start is called before the first frame update
        private void Start()
        {
            rectTransform = transform as RectTransform;
            panelWidth = GetComponent<RectTransform>().sizeDelta.x;

            foreach (var tab in tabs)
            {
                tab.Initialize(toggleGroup);
            }

            currentlyOpenedTab = tabs.Find(t => t.IsVisible);

            Events.OnAnyLeftPanelTabToggle += OnAnyLeftPanelTabToggle;
        }

        private void OnAnyLeftPanelTabToggle(object obj)
        {
            (LeftPanelTab tab, bool on) = ((LeftPanelTab, bool))obj;

            if (tab == null)
            {
                Debug.LogError("[LeftPanelMovement] Tab is null!");
                return;
            }

            if (on)
            {
                OpenPanel(tab);
            }
            // don't animate if there's already a tab open
            else if (!toggleGroup.AnyTogglesOn())
            {
                ClosePanel(tab);
            }
        }

        public void OpenPanel(LeftPanelTab tab)
        {
            // close the current tab if it's open
            if (currentlyOpenedTab != null)
            {
                currentlyOpenedTab.Panel.SetActive(false);
            }

            // show and animate the new tab
            currentlyOpenedTab = tab;
            currentlyOpenedTab.Panel.SetActive(true);
            rectTransform
                .DOAnchorPosX(panelWidth, tweenDuration)
                .SetEase(Ease.InOutExpo);
        }

        public void ClosePanel(LeftPanelTab tab)
        {
            currentlyOpenedTab = null;

            rectTransform
                .DOAnchorPosX(0, tweenDuration)
                .SetEase(Ease.InOutExpo)
                .onComplete = () => tab.Panel.SetActive(false);
        }
    }
}