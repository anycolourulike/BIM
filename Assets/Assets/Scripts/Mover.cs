using UnityEngine;
using UnityEngine.AI;
using System.Collections;


    public class Mover : MonoBehaviour
    {  
               
        private NavMeshAgent navMeshAgent;
        private float baseSpeed;
        Animator anim;

        // True between a MoveTo and the matching CancelNav. While set, the AI
        // (or player) is actively pathing this agent and the animator-state
        // gate below is bypassed - otherwise an enemy that enters combat with
        // zero velocity sits in a non-Locomotion idle state and Update() keeps
        // hard-stopping the agent before it can ever start moving.
        private bool _commandedMove;

        public void Start()
        {
            navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            baseSpeed = navMeshAgent.speed;
            navMeshAgent.updateRotation = false;
            anim = GetComponent<Animator>();

            // If the agent spawned just off the mesh ("Failed to create agent
            // because it is not close enough to the NavMesh"), pull it on now so
            // it can move at all. Only reaches ~2 units - a bigger gap is a
            // scene placement / bake problem, not something code should paper over.
            if (navMeshAgent.isActiveAndEnabled && !navMeshAgent.isOnNavMesh)
                TryRecoverToNavMesh();
        }

        void Update()
        {
            UpdateAnimator();

            if (!AgentReady) return;

            // Only plant the agent for animation when nothing is commanding a
            // move. A commanded move (patrol, combat pursuit, player) always
            // wins so pursuit still works while the base layer is mid-blend.
            if (!_commandedMove)
            {
                AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
                if (!state.IsTag("Locomotion"))
                {
                    navMeshAgent.isStopped = true;
                    navMeshAgent.velocity = Vector3.zero;
                    return;
                }
            }

            navMeshAgent.isStopped = false;
            RotateTowardsVelocity();
        }

        // The agent can only be commanded when it is enabled and actually
        // sitting on the NavMesh. Setting isStopped / destination otherwise
        // throws "Resume"/"SetDestination ... can only be called on an active
        // agent that has been placed on a NavMesh".
        private bool AgentReady =>
            navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh;
        
        public void RotateTowardsVelocity()
        {
            Vector3 dir = navMeshAgent.velocity;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.01f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                10f * Time.deltaTime);
        }

        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            MoveTo(destination, speedFraction);            
        }

        public void MoveTo(Vector3 destination, float speedFraction)
        {
            // Recover an agent that has drifted just off the mesh (spawn point,
            // ramp edge, base-offset mismatch) before giving up on the command.
            if (!AgentReady && !TryRecoverToNavMesh()) return;

            _commandedMove = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.destination = destination;
            navMeshAgent.speed = baseSpeed * Mathf.Clamp01(speedFraction);
        }

        public void CancelNav()
        {
            _commandedMove = false;
            if (!AgentReady) return;
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }

        // Warp the agent onto the nearest NavMesh point if it is enabled but
        // not currently on the mesh. Returns true once it is usable.
        private bool TryRecoverToNavMesh()
        {
            if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled) return false;
            if (navMeshAgent.isOnNavMesh) return true;

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit.position);
                return navMeshAgent.isOnNavMesh;
            }
            return false;
        }

        // Full stop: park the agent, then disable it and this component.
        // Used on death so a corpse's agent can't wedge living enemies in tight spaces.
        public void Deactivate()
        {
            _commandedMove = false;

            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
            }

            if (navMeshAgent != null)
                navMeshAgent.enabled = false;

            enabled = false;
        }
               
        private void UpdateAnimator()
        {
            float speed = (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
                ? navMeshAgent.velocity.magnitude
                : 0f;
            anim.SetFloat("Locomotion", speed);
        }
    }
