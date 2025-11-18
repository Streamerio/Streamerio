using System.Threading;
using Common.Camera;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    public class ZoomAnimation: SequenceAnimationBase
    {
        private readonly ICameraManager _camera;
        private readonly ZoomAnimationParamSO _param;
    
        public ZoomAnimation(ICameraManager camera, ZoomAnimationParamSO param)
        {
            _camera = camera;
            _param = param;

            SetSequence();
        }

        public override async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
        {
            if (useInitial)
            {
                _camera.Move(_param.InitialPosition);
                _camera.SetSize(_param.InitialSize);
            }
        
            await base.PlayAsync(ct, useInitial);
        }

        public override void PlayImmediate()
        {
            _camera.Move(_param.Position);
            _camera.SetSize(_param.CameraSize);
        }

        private void SetSequence()
        {
            Sequence.Append(DOTween.To(() => _param.InitialSize,
                    size => _camera.SetSize(size),
                    _param.CameraSize,
                    _param.DurationSec))
                .SetEase(_param.Ease);

            Sequence.Join(DOTween.To(() => _param.InitialPosition,
                    position => _camera.Move(position),
                    _param.Position,
                    _param.DurationSec))
                .SetEase(_param.Ease);
        }
    }
}