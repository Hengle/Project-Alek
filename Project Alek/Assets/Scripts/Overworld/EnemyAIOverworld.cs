using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Overworld
{
    public class EnemyAIOverworld : Spawnable
    {
        private NavMeshAgent enemy;
        private Animator anim;
        private SpriteRenderer spriteRenderer;

        private Vector3 walkPoint;
        private Vector3 DistanceToWalkPoint => transform.position - walkPoint;

        [SerializeField] private Transform player;
        [SerializeField] private LayerMask whatIsGround, whatIsPlayer;

        [SerializeField] private float normalSpeed;
        [SerializeField] private float chaseSpeed;
        [SerializeField] private float walkPointRange;
        [SerializeField] private float range;
        [SerializeField] private float waitTime;

        private float tempWaitTime;

        [SerializeField] [ReadOnly] 
        private bool playerInRange;
        [SerializeField] [ReadOnly]
        private bool canChase;
        [SerializeField] [ReadOnly]
        private bool wait;
        [SerializeField] [ReadOnly]
        private bool walkPointSet;

        private bool PlayerIsTooFarAway => Math.Abs(transform.position.x - player.position.x) > 30f;
        private bool PlayerOnRightSide => transform.position.x - player.position.x <= 0;
        private bool PlayerOnLeftSide => transform.position.x - player.position.x >= 0;
        private bool WalkPointReached => DistanceToWalkPoint.magnitude < 1f;
        private bool IsFacingAwayFromPlayer =>
            PlayerOnRightSide && spriteRenderer.flipX || PlayerOnLeftSide && !spriteRenderer.flipX;

        private bool PlayerInRange {
            get {
                playerInRange = Physics.CheckSphere(transform.position, range, whatIsPlayer);
                if (!playerInRange) canChase = false;
                return playerInRange;
            }
        }

        private bool InRangeAndFacingTarget => PlayerInRange && !IsFacingAwayFromPlayer;

        private bool DestinationIsOnWalkableGround =>
            Physics.Raycast(walkPoint, -transform.up, 5f, whatIsGround);

        private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
        private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
        private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
        private static readonly int AnimState = Animator.StringToHash("AnimState");
        
        private void Awake()
        {
            enemy = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            player = GameObject.FindWithTag("Player").transform;
            
            anim.SetInteger(AnimState, 0);
            anim.SetBool(IsWalkingHash, true);
        }

        private void OnDrawGizmosSelected() => Gizmos.DrawWireSphere(transform.position, range);

        private void Update()
        {
            if (PlayerIsTooFarAway) Destroy(gameObject);
            if (InRangeAndFacingTarget) canChase = true;
            
            anim.SetFloat(HorizontalHash, enemy.velocity.x);
            
            if (!enemy.isOnNavMesh) return;
            if (canChase) ChasePlayer();
            else Patrolling();
        }

        private void Patrolling()
        {
            if (wait && tempWaitTime > 0) { tempWaitTime -= Time.deltaTime; return; }
            wait = false;

            anim.SetBool(IsWalkingHash, true);
            enemy.speed = normalSpeed;
            
            if (!walkPointSet) SearchWalkPoint();
            if (walkPointSet) enemy.SetDestination(walkPoint);
            if (!WalkPointReached) return;
            
            walkPointSet = false;
            wait = true;
            tempWaitTime = waitTime;
            anim.SetBool(IsWalkingHash, false);
        }

        private void SearchWalkPoint()
        {
            var randomZ = Random.Range(-walkPointRange, walkPointRange);
            var randomX = Random.Range(-walkPointRange, walkPointRange);
            var position = transform.position;
            
            walkPoint = new Vector3(position.x + randomX, position.y, position.z + randomZ);
            walkPointSet = DestinationIsOnWalkableGround;
        }

        private void ChasePlayer()
        {
            anim.SetBool(IsWalkingHash, true);
            enemy.speed = chaseSpeed;
            enemy.SetDestination(player.position);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.transform.CompareTag("Player"))
            {
                LoadSceneManager.Instance.LoadBattle();
            }
        }
    }
}
