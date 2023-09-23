using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine;

namespace GeorgeChew.HiverlabAssessment.UI
{
    using Events = EventMessaging.Registry.UI;

    /// <summary>
    /// Handles moving the left panel in and out of the screen
    /// </summary>
    public class LeftPanelMovement : MonoBehaviour
    {
        private readonly float tweenDuration = 0.3f;

        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private List<LeftPanelTab> tabs;

        private float panelWidth;

        private LeftPanelTab currentlyOpenedPanel;

        private void Awake()
        {
            Assert.IsNotNull(toggleGroup);
            Assert.IsTrue(tabs.Count > 0);
        }

        // Start is called before the first frame update
        private void Start()
        {
            panelWidth = GetComponent<RectTransform>().sizeDelta.x;

            foreach (var tab in tabs)
            {
                tab.Initialize(toggleGroup);
            }

            currentlyOpenedPanel = tabs.Find(t => t.IsVisible);

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
            else if (!toggleGroup.AnyTogglesOn())
            {
                ClosePanel(tab);
            }
        }

        public void OpenPanel(LeftPanelTab panel)
        {
            // we don't wanna move if a panel's already opened
            if (currentlyOpenedPanel == null)
            {
                transform
                    .DOMoveX(panelWidth, tweenDuration)
                    .SetRelative();
            }
            else
            {
                currentlyOpenedPanel.Panel.SetActive(false);
            }

            currentlyOpenedPanel = panel;
            panel.gameObject.SetActive(true);
        }

        public void ClosePanel(LeftPanelTab panel)
        {
            currentlyOpenedPanel = null;

            transform
                .DOMoveX(-panelWidth, tweenDuration)
                .SetRelative()
                .onComplete = () => panel.gameObject.SetActive(false);
        }
    }
}