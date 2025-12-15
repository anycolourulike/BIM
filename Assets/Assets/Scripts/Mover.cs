using UnityEngine;
using UnityEngine.AI;
using System.Collections;


    public class Mover : MonoBehaviour
    {  
               
        private NavMeshAgent navMeshAgent;
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
            RotateTowardsVelocity();
        }  

        public void RotateTowards(Transform target)
        {
            int rotSpeed = 160;
            var targetToLook = Quaternion.LookRotation(target.transform.position - this.transform.position);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetToLook, rotSpeed * Time.deltaTime);            
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
            navMeshAgent.isStopped = false;      
            navMeshAgent.destination = destination; 
            //navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
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
