using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Objects.Enemies.StateMachine;
using Objects.PlayerScripts;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Objects.Enemies
{
    public abstract class BaseEnemy : MonoBehaviourPunCallbacks, ICanBeKilled, IEnemyController
    {
        protected StateMachine.StateMachine stateMachine;
        protected List<PhotonView> photonViewsOfPlayers;
        protected NavMeshAgent agent;
        protected GameObject currentTarget;
        protected float attackTimer;
        protected EnemyModel model;
        protected bool isDead = false;
        
        [Header("Params")]
        [SerializeField] protected int maxHealth = 100;
        [SerializeField] protected int damage = 10;
        [SerializeField] protected float chaseRange = 5f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float attackInterval = 1f;
        [SerializeField] protected float moveDistance = 10f;
        
        [Header("MVC")] 
        [SerializeField] protected EnemyView view;

        [Header("Animations")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected CapsuleCollider mainCollider;

        protected virtual void Start()
        {
            model = gameObject.AddComponent<EnemyModel>();
            model.Initialize(maxHealth, damage, attackRange, attackInterval, view);

            if (PhotonNetwork.IsMasterClient)
            {
                agent = GetComponent<NavMeshAgent>();
                InitializeStateMachine();
                photonViewsOfPlayers = new List<PhotonView>();
            }
        }

        protected virtual void InitializeStateMachine()
        {
            stateMachine = new StateMachine.StateMachine();

            stateMachine.AddState(new PatrolState(this));
            stateMachine.AddState(new ChaseState(this));
            stateMachine.AddState(new AttackState(this));

            stateMachine.ChangeState<PatrolState>();
        }
        
        protected virtual void Update()
        {
            if (!PhotonNetwork.IsMasterClient || isDead) return;

            stateMachine.Update();
            UpdatePlayerTargets();
            UpdateState();
        }
        
        protected void UpdatePlayerTargets()
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
        
        protected void UpdateState()
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
        
        protected GameObject FindNearestPlayer()
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
        
        public virtual Vector3 GetRandomPositionInArea(float distance) 
        {
            Vector3 randomDirection = Random.insideUnitSphere * distance + transform.position;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, distance, -1);
            return navHit.position;
        }
        
        public virtual void Patrol()
        {
            agent.SetDestination(GetRandomPositionInArea(moveDistance));
        }
        
        public virtual void Chase()
        {
            if (currentTarget != null)
            {
                agent.SetDestination(currentTarget.transform.position);
            }
        }
        
        public virtual void AttackCurrentTarget()
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

        public virtual bool HasReachedDestination()
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
        
        public virtual void Die()
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
        protected virtual void RPC_EnableRagdoll()
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
