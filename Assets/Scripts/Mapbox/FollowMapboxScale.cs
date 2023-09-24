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
        [SerializeField] private AbstractMap map;

        private Vector3 originalScale;

        private void Awake()
        {
            Assert.IsNotNull(map);
        }

        private void Start()
        {
            originalScale = transform.localScale;
        }

        private void LateUpdate()
        {
            transform.localScale = originalScale.HadamardMul(map.transform.localScale);
        }
    }
}