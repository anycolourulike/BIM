using UnityEngine;
using UnityEngine.AI;
using System.Collections;


    public class Mover : MonoBehaviour
    {  
               
        private NavMeshAgent navMeshAgent;
        public bool canMove = true;
        Animator anim;
        Rigidbody rb;

        public void Start()
        {
            navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            navMeshAgent.updateRotation = false;
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        void Update()
        {           
            UpdateAnimator();

            if (!canMove)
            {
                navMeshAgent.velocity = Vector3.zero;
                navMeshAgent.isStopped = true;
                return;
            }
            RotateTowardsVelocity();
        }  
        
        public void RotateTowardsVelocity()
        {
            Vector3 dir = navMeshAgent.desiredVelocity;
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
            if (!canMove) return;
            navMeshAgent.isStopped = false;      
            navMeshAgent.destination = destination; 
            navMeshAgent.speed = navMeshAgent.speed * Mathf.Clamp01(speedFraction);
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
            canMove = speed > 0.05f;  //Ensures Sliding stops when the idle animation is playing
        }       
    }
