using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    private const float BreakSpeed = 0.75f;

    public GameObject outline;
    public int height = 2;
    
    private float speed = 3f;
    private float breakTimer;
    
    private Transform targetTransform;
    private Transform eTransform;
    private Rigidbody rb;
    private Core core;
    
    private static readonly Vector3 BreakOffset = new(0, 0.5f, 0.5f);
    private World world;
    private int breakOffsetCounter;

    private int health = 100;
    private int damage = 5;
    private float attackDelay = 1f;
    private float attackTimer;

    private float hitOutlineDelay = 0.2f;
    private float hitOutlineTimer;

    private const float LookDelay = 1f;
    private float lookTimer = LookDelay;

    private void Start()
    { 
        if (height is not (2 or 1)) 
            throw new ArgumentException("Only enemy height or 1 and 2 are supported!");
        
        eTransform = transform;
        targetTransform = GameObject.FindWithTag("Beacon").transform;
        core = GameObject.FindWithTag("Core").GetComponent<Core>();
        rb = GetComponent<Rigidbody>();
        world = GameObject.FindWithTag("World").GetComponent<World>();

        health += (int)(world.BuffData.EnemyHealth * 0.1f);
        attackDelay -= world.BuffData.EnemyAttackSpeed * 0.05f;
    }

    private void Update()
    {
        lookTimer += Time.deltaTime;

        if (lookTimer > LookDelay)
        {
            lookTimer = 0;
            LookAtTarget();
        }

        attackTimer += Time.deltaTime;
        
        if (Move() && attackTimer > attackDelay)
        {
            attackTimer = 0;
            core.TakeDamage(damage);
        }

        breakTimer += Time.deltaTime;

        if (breakTimer > BreakSpeed)
        {
            breakTimer = 0;
            BreakBlocks();
        }

        if (hitOutlineTimer < hitOutlineDelay)
        {
            hitOutlineTimer += Time.deltaTime;
        }
        else if (outline.activeSelf)
        {
            outline.SetActive(false);
        }
    }

    public void TakeDamage(int damageTaken)
    {
        health -= damageTaken;
        outline.SetActive(true);
        hitOutlineTimer = 0;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    private void BreakBlocks()
    {
        int offZ = breakOffsetCounter % 3;
        int offY = height == 1 ? 0: breakOffsetCounter / 3;
        Vector3 offset = new(0, (offY - 1) * BreakOffset.y, (offZ - 1) * BreakOffset.z);

        (Block.Id blockId, Vector3Int blockPos, Vector3Int _) =
            VoxelRay.Cast(world, eTransform.position + offset, eTransform.forward, 2f);

        if (blockId != Block.Id.Air) world.SetBlock(Block.Id.Air, blockPos);

        breakOffsetCounter = (breakOffsetCounter + 1) % 9;
    }

    private bool Move()
    {
        if (Vector3.Distance(eTransform.position, targetTransform.position) < 3f) return true;

        Vector3 newVelocity = eTransform.forward * speed;
        newVelocity.y = rb.velocity.y;
        rb.velocity = newVelocity;
        return false;
    }

    private void LookAtTarget()
    {
        Vector3 position = transform.position;
        Vector3 lookPos = targetTransform.position - position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = rotation;
    }
}
