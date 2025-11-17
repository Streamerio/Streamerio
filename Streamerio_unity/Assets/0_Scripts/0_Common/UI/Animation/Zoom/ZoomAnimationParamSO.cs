using UnityEngine;

namespace Common.UI.Animation
{
    [CreateAssetMenu(fileName = "ZoomAnimationParamSO", menuName = "SO/UI/Animation/Zoom")]
    public class ZoomAnimationParamSO: UIAnimationComponentParamSO
    {
        public float InitialCameraSize;
        public Vector2 InitialPosition;
        
        public float CameraSize;
        public Vector2 Position;
    }
}