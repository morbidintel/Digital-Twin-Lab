using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.UI
{
    using Events = EventMessaging.Registry.UI;

    /// <summary>
    /// Allows a panel tab to trigger the opening/closing its panel
    /// </summary>
    public class LeftPanelTab : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private GameObject panel;

        internal GameObject Panel => panel;
        internal bool IsVisible => toggle.isOn;

        private void Awake()
        {
            Assert.IsNotNull(toggle);
            Assert.IsNotNull(panel);
        }

        // Start is called before the first frame update
        private void Start()
        {
            toggle.onValueChanged.AddListener(value => OnToggleValueChanged(value));
        }

        internal void Initialize(ToggleGroup group)
        {
            toggle.group = group;
        }

        private void OnToggleValueChanged(bool value)
        {
            // don't close panel if there other panels are opened
            Events.OnAnyLeftPanelTabToggle.Publish((this, value));

            //if (value)
            //{
            //    LeftPanelMovement.Instance.OpenPanel(panel);
            //}
            //else if (!toggle.group.AnyTogglesOn())
            //{
            //    LeftPanelMovement.Instance.ClosePanel(panel);
            //}
        }
    }
}