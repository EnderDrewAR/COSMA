using UnityEngine;

public class SunSensor : MonoBehaviour
{
    [SerializeField] private Transform _sun;          // Ссылка на объект Солнца
    public float GetValue()
    {
        if (_sun == null) return 0f;
        
        Vector3 sensorDirection = transform.forward;
        
        Vector3 toSun = -(_sun.position - transform.position).normalized;
        
        float cosAngle = Vector3.Dot(sensorDirection, toSun);
        
        return Mathf.Clamp01(cosAngle);
    }
    
    private void Update()
    {
        float value = GetValue();
        Debug.Log($"Солнечный датчик: {value:F3} ({value * 100:F1}%)");
    }
}
