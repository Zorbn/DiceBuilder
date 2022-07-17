using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Core core;
    public RectTransform healthRect;
    
    void Update()
    {
        Vector2 scale = healthRect.localScale;
        scale.x = (core.Health - core.DamageTaken) / (float)core.Health;
        healthRect.localScale = scale;
    }
}
