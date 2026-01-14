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
               
        private void UpdateAnimator()
        {
            float speed = navMeshAgent.velocity.magnitude;
            anim.SetFloat("Locomotion", speed);  
        }       
    }
