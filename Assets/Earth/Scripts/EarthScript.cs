using UnityEngine;
using System.Collections.Generic;

namespace DigitalRuby.Earth
{
    [RequireComponent(typeof(MeshRenderer))]
    public class EarthScript : SphereScript
    {
        [Range(-1000.0f, 1000.0f)]
        [Tooltip("Rotation speed around axis")]
        public float RotationSpeed = 1.0f;

        [Tooltip("Planet axis in world vector, defaults to start up vector")]
        public Vector3 Axis;

        [Tooltip("The sun, defaults to first light in scene")]
        public Light Sun;

        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock materialBlock;

        private void OnEnable()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            materialBlock = new MaterialPropertyBlock();
            
            // Безопасный поиск любого света (если не назначен в инспекторе)
            if (Sun == null)
            {
                Light[] allLights = FindObjectsOfType<Light>();
                
                if (allLights.Length > 0)
                {
                    // Берём первый свет в сцене
                    Sun = allLights[0];
                    Debug.Log($"Найден свет: {Sun.name} (тип: {Sun.type})");
                }
                else
                {
                    // Создаём свет программно
                    GameObject sunGO = new GameObject("Sun");
                    Sun = sunGO.AddComponent<Light>();
                    Sun.type = LightType.Spot;
                    Sun.color = Color.yellow;
                    Sun.intensity = 1.5f;
                    Sun.range = 100f;
                    Sun.transform.position = new Vector3(0, 50, 0);
                    
                    Debug.LogWarning("⚠️ На сцене не найден свет. Создан SpotLight 'Sun'.");
                }
            }
            
            // Настройка оси вращения
            if (Axis == Vector3.zero)
            {
                Axis = transform.up;
            }
            else
            {
                Axis = Axis.normalized;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (materialBlock != null && Sun != null && meshRenderer != null)
            {
                meshRenderer.GetPropertyBlock(materialBlock);
                
                // Для любого типа света: направление от Земли к Солнцу
                Vector3 sunDirection = (Sun.transform.position - transform.position).normalized;
                materialBlock.SetVector("_SunDir", sunDirection);
                
                // Цвет и интенсивность
                materialBlock.SetVector("_SunColor", new Vector4(Sun.color.r, Sun.color.g, Sun.color.b, Sun.intensity));
                
                meshRenderer.SetPropertyBlock(materialBlock);
            }

#if UNITY_EDITOR

            if (Application.isPlaying)
            {

#endif

                transform.Rotate(Axis, RotationSpeed * Time.deltaTime);

#if UNITY_EDITOR

            }

#endif

        }
    }
}