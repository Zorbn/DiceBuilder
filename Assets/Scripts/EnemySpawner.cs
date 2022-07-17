using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public World world;

    private const float BaseSpawnDelay = 15f;
    private const float MinSpawnDelay = 1.5f;
    private float spawnSpeedIncrease = 0.1f;
    private float spawnDelay = BaseSpawnDelay;
    private float spawnTimer = BaseSpawnDelay;

    private const float RateIncreaseDelay = 15f;
    private int rateIncrease;
    private float rateIncreaseTimer;

    private void Update()
    {
        if (!world.HasSpawningStarted) return;

        spawnTimer += Time.deltaTime;
        rateIncreaseTimer += Time.deltaTime;

        if (rateIncreaseTimer > RateIncreaseDelay)
        {
            rateIncrease++;
            rateIncreaseTimer = 0;
        }

        if (spawnTimer > spawnDelay)
        {
            spawnTimer = 0;
            spawnDelay = Mathf.Max(MinSpawnDelay, BaseSpawnDelay - rateIncrease * spawnSpeedIncrease);

            Instantiate(enemy, transform.position, Quaternion.identity);
        }
    }
}
