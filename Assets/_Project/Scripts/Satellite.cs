using UnityEngine;

public class Satellite : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private Transform _orbitCenter;
    [SerializeField] private float _orbitSpeed = 5f;
    [SerializeField] private float _orbitRadius = 50f;
    [SerializeField] private float _orbitInclination = 25f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float _rotationForce = 1f;
    [SerializeField] private SunSensor _sunSensor;
    
    private Rigidbody _rigidbody;
    private Vector3 _torqueInput = Vector3.zero;
    private Vector3 _defaultPosition;
    private Quaternion _defaultRotation;
    private float _orbitAngle = 0f;

    private void Awake()
    {
        _rigidbody =  GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _defaultPosition = transform.position;
        _defaultRotation = transform.rotation;
        
        if (_orbitCenter != null)
        {
            Vector3 direction = transform.position - _orbitCenter.position;
            _orbitAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        } 
        if (_orbitCenter != null)
        {
            // Вычисляем начальный угол орбиты
            Vector3 direction = transform.position - _orbitCenter.position;
            _orbitAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            
            // Устанавливаем правильное расстояние от центра
            _orbitRadius = direction.magnitude;
        } 
        // _orbitCenter.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(Vector3.up * 110);
    }
    private void Update()
    {
        if (_orbitCenter != null)
        {
            _orbitAngle += _orbitSpeed * Time.deltaTime;
            
            // 1. Вычисляем позицию на базовой орбите (XZ плоскость)
            float x = Mathf.Sin(_orbitAngle * Mathf.Deg2Rad) * _orbitRadius;
            float z = Mathf.Cos(_orbitAngle * Mathf.Deg2Rad) * _orbitRadius;
            float y = 0f;
            
            Vector3 orbitPosition = new Vector3(x, y, z);
            
            // 2. 🆕 Поворачиваем орбиту на угол наклонения (вокруг оси X)
            Quaternion inclinationRotation = Quaternion.Euler(_orbitInclination, 0f, 0f);
            orbitPosition = inclinationRotation * orbitPosition;
            
            // 3. Применяем позицию относительно центра орбиты
            transform.position = _orbitCenter.position + orbitPosition;
        }
        
        if (_sunSensor != null)
        {
            float sunValue = _sunSensor.GetValue();
        
            if (sunValue > 0.8f)
            {
                // Debug.Log("Зарядка от Солнца!");
            }
        
            if (sunValue < 0.3f)
            {
                // RotateRight(); // ваш метод поворота
            }
        }
        
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

    // Read-only status properties for UI
    public Vector3 CurrentAngularVelocity => _rigidbody != null ? _rigidbody.angularVelocity : Vector3.zero;
    public float CurrentOrbitAngle => _orbitAngle;
    public bool IsStable => _rigidbody != null && _rigidbody.angularVelocity.magnitude < 0.1f;
}
