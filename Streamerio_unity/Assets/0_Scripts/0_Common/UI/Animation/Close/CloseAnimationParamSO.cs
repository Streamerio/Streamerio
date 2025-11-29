using UnityEngine;

namespace Common.UI.Animation
{
    [CreateAssetMenu(fileName = "CloseAnimationParamSO", menuName = "SO/UI/Animation/Close")]
    public class CloseAnimationParamSO: UIAnimationComponentParamSO
    {
        public Vector2 InitialAnchoredPosition;
        public Vector2 AnchoredPosition;

        public Vector2 InitialScale;
        public Vector2 Scale;
    }
}