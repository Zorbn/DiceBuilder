using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GlowingCube))]
public class Core : MonoBehaviour
{
    public Transform gfx;
    public Transform beaconGfx;
    public World world;

    private GlowingCube glowingCube;

    private const int BaseHealth = 100;
    public int Health { get; private set; } = BaseHealth;
    public int DamageTaken { get; private set; }
    
    private bool isDestroyed;
    private bool isBeaconHidden, isGfxHidden;

    private void Start()
    {
        glowingCube = GetComponent<GlowingCube>();
    }

    private void Update()
    {
        if (isDestroyed)
        {
            if (!isBeaconHidden)
            {
                Vector3 beaconScale = beaconGfx.localScale;
                beaconScale.y = Mathf.Lerp(beaconScale.y, 0, Time.deltaTime);
                beaconGfx.localScale = beaconScale;

                if (beaconScale.y < 0.1f)
                {
                    beaconGfx.gameObject.SetActive(false);
                    isBeaconHidden = true;
                }
            }

            if (!isGfxHidden)
            {
                Vector3 gfxScale = gfx.localScale;
                float newGfxScale = Mathf.Lerp(gfxScale.x, 0, Time.deltaTime);
                gfxScale.x = newGfxScale;
                gfxScale.y = newGfxScale;
                gfxScale.z = newGfxScale;
                gfx.localScale = gfxScale;

                if (gfxScale.x < 0.1f)
                {
                    gfx.gameObject.SetActive(false);
                    isGfxHidden = true;
                }
            }

            if (isBeaconHidden && isGfxHidden)
            {
                SceneManager.LoadScene("DeathMenu");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDestroyed) return;
        
        DamageTaken += damage;
        glowingCube.Accelerate();

        Health = BaseHealth + (int)(world.BuffData.CoreHealth * 0.1f);
        
        if (DamageTaken >= Health)
        {
            isDestroyed = true;
        }
    }
}
