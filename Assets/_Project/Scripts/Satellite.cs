using UnityEngine;

public class Satellite : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Vector3 _defaultPosition;
    private Quaternion _defaultRotation;

    private void Awake()
    {
        _rigidbody =  GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _defaultPosition = transform.position;
        _defaultRotation = transform.rotation;
    }

    public void Rotate()
    {
        _rigidbody.AddRelativeTorque(Vector3.up * 100 * Time.deltaTime * 10);
    }

    public void Reset()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        
        transform.position = _defaultPosition;
        transform.rotation = _defaultRotation; 
    }
}
