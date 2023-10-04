using UnityEngine;

namespace Universal
{
    [RequireComponent(typeof(Camera))]
    public class CameraAdapt : MonoBehaviour
    {
        [Header ("Min Screen Width")]
        public int minWidth = 1940;
        [Header ("Min Screen Height")]
        public int minHeight = 1080;

        private Camera cameraComponent;
        private float initialSize;

        private void Awake()
        {
            cameraComponent = GetComponent<Camera>();
            initialSize = cameraComponent.orthographicSize; 
            Rebuild();
        }
        private void Rebuild()
        {
            float proportions = ((float)Screen.width / Screen.height);
            float propBase = minWidth*1f / minHeight;
            if (proportions < propBase)
            {
                cameraComponent.orthographicSize = initialSize * (propBase / proportions);
            }
        }
    }
}
