using UnityEngine;

public class GlowingCube : MonoBehaviour
{
    public float speedX = 0.25f;
    public float speedY = 0.25f;
    public float speedZ = 0.25f;
    private float speedMultiplier;
    private float speedFalloff = 0.1f;
    private float time;

    private void Update()
    {
        time += Time.deltaTime;

        Vector3 rotation = transform.rotation.eulerAngles;
        if (speedY != 0) rotation.y = Mathf.Sin(time * speedY * speedMultiplier) * 360f;
        if (speedX != 0) rotation.x = Mathf.Cos(time * speedX * speedMultiplier) * 360f;
        if (speedZ != 0) rotation.z = Mathf.Cos(time * speedZ * speedMultiplier) * 360f;
        transform.rotation = Quaternion.Euler(rotation);
        
        speedMultiplier -= Time.deltaTime * speedFalloff;
        if (speedMultiplier < 1) speedMultiplier = 1;
    }

    public void Accelerate()
    {
        speedMultiplier = 1.1f;
    }
}
