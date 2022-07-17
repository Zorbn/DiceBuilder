using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Player : MonoBehaviour
{
    public const float Reach = 10;

    public World world;
    public Core core;
    public Camera cam;
    public float lookSpeed = 3;
    public GameObject attackingCrosshair;
    public ParticleSystem attackParticles;
    public GameObject coreHealthBar;
    public GameObject startText;
    
    private Transform camTransform;
    private CapsuleCollider capsuleCollider;
    
    private Vector2 rotation = Vector2.zero;
    private Rigidbody rb;
    private float speed = 5f;
    private float jumpForce = 6f;
    private Transform pTransform;

    private Vector3 feetOffset;
    private LayerMask playerMask;
    private LayerMask beaconMask;

    private bool inAttackMode;

    private int baseDamage = 20;
    private const float baseAttackDelay = 0.5f;
    private float attackDelay = baseAttackDelay;
    private float attackTimer;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        pTransform = transform;
        camTransform = cam.transform;

        feetOffset = new Vector3(0, -capsuleCollider.height / 2f - 0.1f, 0) - CubeMesh.offset;
        playerMask = LayerMask.GetMask("Player");
        beaconMask = LayerMask.GetMask("Beacon");
    }

    private void Update()
    {
        attackDelay = baseAttackDelay - world.BuffData.AttackSpeed * 0.05f;
        attackTimer += Time.deltaTime;
        
        Look();
        Move();

        UpdateMode();
        
        bool leftClick = Input.GetButtonDown("Fire1"); // Break block or attack.
        bool rightClick = Input.GetButtonDown("Fire2"); // Place block.
        
        if (inAttackMode)
        {
            Attack(leftClick, rightClick);
        }
        else
        {
            Interact(leftClick, rightClick);
        }

        UpdateGameState();
    }

    private void UpdateGameState()
    {
        if (Input.GetButtonDown("Submit"))
        {
            world.StartSpawning();
            startText.SetActive(false);
            coreHealthBar.SetActive(true);
        }
    }

    private void UpdateMode()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            inAttackMode = !inAttackMode;
        }
        
        if (Input.GetButtonDown("EnterBuildMode"))
        {
            inAttackMode = false;
        }
        
        if (Input.GetButtonDown("EnterAttackMode"))
        {
            inAttackMode = true;
        }
    }

    private void Attack(bool leftClick, bool rightClick)
    {
        attackingCrosshair.SetActive(true);

        if (leftClick && attackTimer > attackDelay)
        {
            attackTimer = 0;
            
            attackParticles.Play();
            
            RaycastHit hit;
            Physics.Raycast(camTransform.position, camTransform.forward, out hit, Reach, ~playerMask);

            Collider hitCollider = hit.collider;

            if (!hitCollider) return;
            
            GameObject hitGo = hit.collider.gameObject;

            if (hitGo && hitGo.CompareTag("Enemy"))
            {
                Enemy enemy = hitGo.GetComponent<Enemy>();
                enemy.TakeDamage(baseDamage + (int)(baseDamage * world.BuffData.Damage * 0.1f));
            }
        }
    }

    private void Interact(bool leftClick, bool rightClick)
    {
        attackingCrosshair.SetActive(false);
        
        if (!leftClick && !rightClick) return;
        
        (Block.Id blockId, Vector3Int blockPos, Vector3Int hitNormal) =
            VoxelRay.Cast(world, camTransform.position, camTransform.forward, Reach);

        if (blockId == Block.Id.Air) return;

        if (leftClick)
        {
            world.SetBlock(Block.Id.Air, blockPos);
        }
        else
        {
            Vector3Int placePos = blockPos + hitNormal;

            Random random = SharedRandom.Default;

            Block.Id placeBlockId = Block.Id.Die;

            if (random.Next(6) == 0)
            {
                switch (random.Next(5))
                {
                    case 0:
                        world.BuffData.CoreHealth++;
                        placeBlockId = Block.Id.PositiveDie;
                        break;
                    case 1:
                        world.BuffData.Damage++;
                        placeBlockId = Block.Id.PositiveDie;
                        break;
                    case 2:
                        world.BuffData.AttackSpeed++;
                        placeBlockId = Block.Id.PositiveDie;
                        break;
                    case 3:
                        world.BuffData.EnemyHealth++;
                        placeBlockId = Block.Id.NegativeDie;
                        break;
                    case 4:
                        world.BuffData.EnemyAttackSpeed++;
                        placeBlockId = Block.Id.NegativeDie;
                        break;
                }
            }

            if (!world.IsBlockPhysicallyOccupied(placePos)) world.SetBlock(placeBlockId, placePos);
        }
    }

    private void Move()
    {
        Vector3 movement = Vector3.zero;
        Vector2 input = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input.Normalize();
        movement += speed * input.x * pTransform.right;
        movement += speed * input.y * pTransform.forward;
        movement.y = rb.velocity.y;

        if (Input.GetButton("Jump"))
        {
            Vector3 feetPos = pTransform.position + feetOffset;

            if (world.IsUnsnappedBlockPhysicallyOccupied(feetPos, ~(playerMask | beaconMask), -0.11f))
            {
                movement.y = jumpForce;
            }
        }

        rb.velocity = movement;
    }

    private void Look()
    {
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -89f, 89f);
        transform.eulerAngles = new Vector2(0, rotation.y);
        cam.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
    }
}
