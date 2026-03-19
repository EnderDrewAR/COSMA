using UnityEngine;

public class CameraFollows : MonoBehaviour
{
    [SerializeField] private Transform _targetLookAt;
    [SerializeField] private Transform _targetPosition;
    
    private void Start()
    {

    }

    private void LateUpdate()
    {
        transform.position = _targetPosition.position;
        transform.LookAt(_targetLookAt);
    }
}