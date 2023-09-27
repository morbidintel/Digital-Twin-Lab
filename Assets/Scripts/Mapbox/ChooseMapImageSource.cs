using Mapbox.Unity.Map;
using System.Linq;
using System;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine;

namespace GeorgeChew.UnityAssessment.MapBox
{
    /// <summary>
    /// Facilitates changing the Mapbox Image Source with a Dropdown
    /// </summary>
    public class ChooseMapImageSource : MonoBehaviour
    {
        [SerializeField] private AbstractMap map;
        [SerializeField] private Dropdown dropdown;

        private void Awake()
        {
            Assert.IsNotNull(map);
            Assert.IsNotNull(dropdown);
        }

        private void Start()
        {
            PopulateDropdown();

            dropdown.onValueChanged.AddListener(value => OnDropdownValueChanged(value));
        }

        private void PopulateDropdown()
        {
            dropdown.options.Clear();
            dropdown.options
                .AddRange(Enum.GetNames(typeof(ImagerySourceType))
                .Take(6) // skip 'Custom' and 'None'
                .Select(n => new Dropdown.OptionData() { text = n }));
            dropdown.value = (int)map.ImageLayer.LayerSource;
        }

        public void OnDropdownValueChanged(int value)
        {
            map.ImageLayer.SetLayerSource((ImagerySourceType)value);
        }
    }
}