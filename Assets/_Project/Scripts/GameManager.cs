using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _satellite;
    [SerializeField] private UILayoutBuilder _uiBuilder;
    [SerializeField] private UITheme _theme;

    private void Start()
    {
        if (_uiBuilder != null && _theme != null)
            _uiBuilder.Initialize(_theme);
    }
}
