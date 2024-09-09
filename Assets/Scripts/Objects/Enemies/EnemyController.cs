using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Objects.Enemies.StateMachine;
using Objects.PlayerScripts;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

namespace Objects.Enemies
{
    public class EnemyController : MonoBehaviourPunCallbacks, ICanBeKilled, IEnemyController
    {
        private StateMachine.StateMachine stateMachine;
        private List<PhotonView> photonViewsOfPlayers;
        private NavMeshAgent agent;
        private GameObject currentTarget;
        private float attackTimer;
        private EnemyModel model;
        private bool isDead = false;

        [Header("Params")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int damage = 10;
        [SerializeField] private float chaseRange = 5f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackInterval = 1f;
        [SerializeField] private float moveDistance = 10f;
        
        [Header("MVC")] 
        [SerializeField] private EnemyView view;

        [Header("Animations")]
        [SerializeField] private Animator animator;
        [SerializeField] private CapsuleCollider mainCollider;

        private void Start()
        {
            model = gameObject.AddComponent<EnemyModel>();
            agent = GetComponent<NavMeshAgent>();
            model.Initialize(maxHealth, damage, attackRange, attackInterval, view);

            if (PhotonNetwork.IsMasterClient)
            {
                InitializeStateMachine();
                photonViewsOfPlayers = new List<PhotonView>();
            }
        }

        private void InitializeStateMachine()
        {
            stateMachine = new StateMachine.StateMachine();

            stateMachine.AddState(new PatrolState(this));
            stateMachine.AddState(new ChaseState(this));
            stateMachine.AddState(new AttackState(this));

            stateMachine.ChangeState<PatrolState>();
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient || isDead) return;

            stateMachine.Update();
            UpdatePlayerTargets();
            UpdateState();
        }

        private void UpdatePlayerTargets()
        {
            photonViewsOfPlayers.Clear();
            foreach (var view in FindObjectsOfType<PhotonView>())
            {
                if (view.GetComponent<PlayerControllerWithCC>() != null)
                {
                    photonViewsOfPlayers.Add(view);
                }
            }
        }

        private void UpdateState()
        {
            currentTarget = FindNearestPlayer();
            if (currentTarget == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distanceToPlayer <= chaseRange && distanceToPlayer > attackRange)
            {
                if (!(stateMachine.CurrentState is ChaseState))
                {
                    stateMachine.ChangeState<ChaseState>();
                }
            }
            else if (distanceToPlayer <= attackRange)
            {
                if (!(stateMachine.CurrentState is AttackState))
                {
                    stateMachine.ChangeState<AttackState>();
                }
            }
            else
            {
                if (!(stateMachine.CurrentState is PatrolState))
                {
                    stateMachine.ChangeState<PatrolState>();
                }
            }
        }

        private GameObject FindNearestPlayer()
        {
            GameObject nearestPlayer = null;
            float nearestDistance = Mathf.Infinity;

            foreach (var player in photonViewsOfPlayers)
            {
                if (player == null) continue;

                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPlayer = player.gameObject;
                }
            }
            return nearestPlayer;
        }
        
        public Vector3 GetRandomPositionInArea(float distance) 
        {
            Vector3 randomDirection = Random.insideUnitSphere * distance + transform.position;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, distance, -1);
            return navHit.position;
        }

        public void Patrol()
        {
            agent.SetDestination(GetRandomPositionInArea(moveDistance));
        }
        
        public void Chase()
        {
            if (currentTarget != null)
            {
                agent.SetDestination(currentTarget.transform.position);
            }
        }

        public void AttackCurrentTarget()
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                if (currentTarget != null)
                {
                    PhotonView targetPhotonView = currentTarget.GetComponent<PhotonView>();

                    if (targetPhotonView != null)
                    {
                        targetPhotonView.RPC("TakeDamage", targetPhotonView.Owner, model.Damage);
                        attackTimer = attackInterval;
                    }
                }
            }
        }

        public bool HasReachedDestination()
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Die()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                isDead = true;
                photonView.RPC("RPC_EnableRagdoll", RpcTarget.All);
                StartCoroutine(DelayedDestroy(60f));
            }
        }

        private IEnumerator DelayedDestroy(float delay)
        {
            yield return new WaitForSeconds(delay);
            PhotonNetwork.Destroy(gameObject);
        }

        [PunRPC]
        private void RPC_EnableRagdoll()
        {
            agent.enabled = false;
            animator.enabled = false;
            view.SetTextVisibility(false);

            foreach (var rb in GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = false;
            }

            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = true;
            }
            mainCollider.enabled = false;
        }
    }
}