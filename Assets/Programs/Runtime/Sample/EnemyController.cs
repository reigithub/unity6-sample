using Game.Contents.Player;
using UnityEngine;
using UnityEngine.AI;

namespace Sample
{
    /// <summary>
    /// 簡易的なエネミー追尾システム
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private GameObject _player;

        // private Rigidbody _playerRigidbody;
        // private SDUnityChanPlayerController _playerController;
        private NavMeshAgent _navMeshAgent;
        private Vector3 _originPosition;

        private float _distance;
        private float _viewAngle;
        private NavMeshHit _navMeshHit;

        public void SetPlayer(GameObject player)
        {
            _player = player;
            // _player.TryGetComponent<Rigidbody>(out _playerRigidbody);
            // _player.TryGetComponent<SDUnityChanPlayerController>(out _playerController);
        }

        private void Start()
        {
            if (TryGetComponent<NavMeshAgent>(out var navMeshAgent))
            {
                _navMeshAgent = navMeshAgent;
                // _navMeshAgent.speed = 5f; // 速度値もマスタから入れる
                _originPosition = transform.position; // 最初の位置はマスタから入れる？
            }
        }

        private void Update()
        {
            if (!_player) return;

            DetectPlayerOrReturn();
        }

        private void DetectPlayerOrReturn()
        {
            _distance = Vector3.Distance(_player.transform.position, transform.position);

            // プレイヤー探索
            if (!TryDetectPlayerByVision() && !TryDetectPlayerByRandom())
            {
                // オリジナル位置に戻る
                TrySetDestination(_originPosition);
            }
        }

        private bool TryDetectPlayerByVision()
        {
            //視覚で感知
            // if (_distance > 25f)
            if (_distance > 5f)
                return false;

            Vector3 distance = transform.position - _player.transform.position;
            Vector3 cross = Vector3.Cross(transform.forward, distance);
            _viewAngle = Vector3.Angle(transform.forward, distance) * (cross.y < 0f ? -1f : 1f);
            _viewAngle += 180f;

            if (_viewAngle <= 45f || _viewAngle >= 315f)
            {
                //Rayを飛ばしてプレイヤーとの間に障害物がないか確認する
                Vector3 diff = _player.transform.position - transform.position;
                float maxDistance = diff.magnitude;
                Vector3 direction = diff.normalized;
                Vector3 eyePosition = transform.position + new Vector3(0f, 0.5f, 0f);

                // Debug.DrawRay(eyePosition, transform.forward + Quaternion.Euler(0, _viewAngle, 0) * transform.forward * 5f, Color.yellow);
                // Debug.DrawRay(eyePosition, transform.forward + Quaternion.Euler(0, -_viewAngle, 0) * transform.forward * 5f, Color.yellow);

                // int layerMask = ~(1 << 13);
                // RaycastHit[] raycastHits = Physics.RaycastAll(eyeHeightPos, direction, distance, layerMask);

                RaycastHit[] raycastHitResults = new RaycastHit[1];
                var raycastHitCount = Physics.RaycastNonAlloc(new Ray(eyePosition, direction), raycastHitResults, maxDistance);

                // Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), direction * distance, Color.red);

                if (raycastHitCount > 0)
                {
                    // Debug.Log($"raycastHitCount: {raycastHitCount}");

                    if (raycastHitResults[0].transform.gameObject.CompareTag("Player"))
                    {
                        if (NavMesh.SamplePosition(_player.transform.position, out _navMeshHit, 1f, 1))
                        {
                            return TrySetDestination(_navMeshHit.position);
                        }
                    }
                }
            }

            return false;
        }

        private bool TryDetectPlayerByRandom()
        {
            if (_distance > 3f)
                return false;

            // if (_playerRigidbody.linearVelocity.magnitude > 0.1f)
            // if (_playerController.IsMoving())
            {
                float x = _player.transform.position.x + Random.Range(-2f, 2f);
                float z = _player.transform.position.z + Random.Range(-2f, 2f);

                if (NavMesh.SamplePosition(new Vector3(x, 0f, z), out _navMeshHit, 1f, 1))
                {
                    var forward = _player.transform.position - transform.position;
                    forward.y = 0f;
                    var lookRotation = Quaternion.LookRotation(forward);
                    var slerp = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.fixedDeltaTime);
                    transform.rotation = slerp;

                    // if (_playerController.IsMoving())
                    {
                        return TrySetDestination(_navMeshHit.position);
                    }
                }
            }

            return false;
        }

        private bool TrySetDestination(Vector3 position)
        {
            if (_navMeshAgent && _navMeshAgent.pathStatus != NavMeshPathStatus.PathInvalid)
            {
                _navMeshAgent.SetDestination(position);
                return true;
            }

            return false;
        }
    }
}