using UnityEngine;

namespace Sample
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private GameObject player;

        private Vector3 _offset;

        private void Start()
        {
            if (player) _offset = this.transform.position - player.transform.position;
        }

        private void LateUpdate()
        {
            if (player) this.transform.position = player.transform.position + _offset;
        }

        public void SetPlayer(GameObject p)
        {
            player = p;
        }
    }
}