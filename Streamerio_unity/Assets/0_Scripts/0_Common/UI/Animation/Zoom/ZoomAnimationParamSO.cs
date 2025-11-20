using UnityEngine;

namespace Common.UI.Animation
{
    [CreateAssetMenu(fileName = "ZoomAnimationParamSO", menuName = "SO/UI/Animation/Zoom")]
    public class ZoomAnimationParamSO: UIAnimationComponentParamSO
    {
        public float InitialSize;
        public Vector3 InitialPosition;
        
        public float CameraSize;
        public Vector3 Position;
    }
}