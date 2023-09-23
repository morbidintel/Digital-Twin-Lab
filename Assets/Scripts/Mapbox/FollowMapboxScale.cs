using Gamelogic.Extensions;
using Mapbox.Unity.Map;
using UnityEngine.Assertions;
using UnityEngine;

namespace GeorgeChew.HiverlabAssessment.Mapbox
{
    /// <summary>
    /// Updates a GameObject's scale to scale according to a <see cref="AbstractMap"/>
    /// </summary>
    public class FollowMapboxScale : MonoBehaviour
    {
        [SerializeField] AbstractMap map;

        Vector3 originalScale;

        private void Awake()
        {
            Assert.IsNotNull(map);
        }

        void Start()
        {
            originalScale = transform.localScale;
        }

        void Update()
        {
            transform.localScale = originalScale.HadamardMul(map.transform.localScale);
        }
    }
}