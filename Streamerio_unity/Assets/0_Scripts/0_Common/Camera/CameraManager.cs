using UnityEngine;

namespace Common.Camera
{
    public interface ICameraManager
    {
        void Move(Vector3 position);
        void SetSize(float size);
    }
    
    public class CameraManager: ICameraManager
    {
        private readonly UnityEngine.Camera _mainCamera;
        private readonly UnityEngine.Camera[] _overlayCamera;
        
        public CameraManager(UnityEngine.Camera mainCamera, params UnityEngine.Camera[] overlayCamera)
        {
            _mainCamera = mainCamera;
            _overlayCamera = overlayCamera;
        }

        public void Move(Vector3 position)
        {
            _mainCamera.transform.position = position;
        }

        public void SetSize(float size)
        {
            var sizeDiff = size - _mainCamera.orthographicSize;
            _mainCamera.orthographicSize = size;
            
            foreach (var camera in _overlayCamera)
            {
                camera.fieldOfView += sizeDiff;
            }
        }
    }
}