using UnityEngine;
using UnityEngine.AI;
using System.Collections;


    public class Mover : MonoBehaviour
    {  
               
        private NavMeshAgent navMeshAgent;
        private float baseSpeed;
        Animator anim;

        public void Start()
        {
            navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            baseSpeed = navMeshAgent.speed;
            navMeshAgent.updateRotation = false;
            anim = GetComponent<Animator>();
        }

        void Update()
        {     
            UpdateAnimator();
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

            if (!state.IsTag("Locomotion"))
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.velocity = Vector3.zero;
                return;
            }
            
            navMeshAgent.isStopped = false; 
            RotateTowardsVelocity();
        }  
        
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
            navMeshAgent.isStopped = false;      
            navMeshAgent.destination = destination; 
            navMeshAgent.speed = baseSpeed * Mathf.Clamp01(speedFraction);
        }

        public void CancelNav()
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }

        // Full stop: park the agent, then disable it and this component.
        // Used on death so a corpse's agent can't wedge living enemies in tight spaces.
        public void Deactivate()
        {
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
            float speed = navMeshAgent.velocity.magnitude;
            anim.SetFloat("Locomotion", speed);  
        }       
    }
