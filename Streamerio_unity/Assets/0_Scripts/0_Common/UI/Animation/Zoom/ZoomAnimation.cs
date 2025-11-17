using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    public class ZoomAnimation: SequenceAnimationBase
    {
        private readonly Camera _camera;
        private readonly ZoomAnimationParamSO _param;
    
        public ZoomAnimation(Camera camera, ZoomAnimationParamSO param)
        {
            _camera = camera;
            _param = param;

            SetSequence();
        }

        public override async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
        {
            if (useInitial)
            {
                _camera.orthographicSize = _param.InitialCameraSize;
                _camera.transform.position = _param.InitialPosition;
            }
        
            await base.PlayAsync(ct, useInitial);
        }

        public override void PlayImmediate()
        {
            _camera.orthographicSize = _param.CameraSize;
            _camera.transform.position = _param.Position;
        }

        private void SetSequence()
        {
            Sequence.Append(_camera
                .DOOrthoSize(_param.CameraSize, _param.DurationSec)
                .SetEase(_param.Ease));
        
            Sequence.Join(_camera.transform
                .DOMove(_param.Position, _param.DurationSec)
                .SetEase(_param.Ease));
        }
    }
}