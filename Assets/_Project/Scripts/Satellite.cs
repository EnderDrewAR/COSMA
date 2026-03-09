using UnityEngine;

public class Satellite : MonoBehaviour
{
    [SerializeField] private float _rotationForce = 10f;
    
    private Rigidbody _rigidbody;
    private Vector3 _torqueInput = Vector3.zero;
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

    private void FixedUpdate()
    {
        if (_torqueInput != Vector3.zero)
        {
            _rigidbody.AddRelativeTorque(_torqueInput * _rotationForce);
            _torqueInput = Vector3.zero;
        }
    }

    public void RotateRight() => _torqueInput = Vector3.up;
    public void RotateLeft() => _torqueInput = Vector3.down;
    public void RotateUp() => _torqueInput = Vector3.right;
    public void RotateDown() => _torqueInput = Vector3.left;
    
    public void Reset()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        
        transform.position = _defaultPosition;
        transform.rotation = _defaultRotation; 
    }
}
