using Unity.Cinemachine;
using UnityEngine;

namespace Game.Contents.Player
{
    public class PlayerFollowCameraController : MonoBehaviour
    {
        [SerializeField] private GameObject _player;

        public void SetPlayer(GameObject player)
        {
            _player = player;

            if (_player)
            {
                if (gameObject.TryGetComponent<CinemachineCamera>(out var cinemachineCamera))
                {
                    cinemachineCamera.Target.TrackingTarget = _player.transform;
                }
            }
        }
    }
}