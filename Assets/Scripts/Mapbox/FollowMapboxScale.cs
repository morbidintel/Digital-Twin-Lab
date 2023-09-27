using UnityEngine;
using UnityEngine.Assertions;
using Mapbox.Unity.Map;
using Gamelogic.Extensions;

namespace GeorgeChew.UnityAssessment.Mapbox
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