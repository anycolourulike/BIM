using UnityEngine;
using UnityEngine.AI;
using System.Collections;


    public class Mover : MonoBehaviour
    {  
               
        private NavMeshAgent navMeshAgent;
        private Vector3 destination;
        public float maxSpeed;
        Animator anim;
        Rigidbody rb;

        public void Start()
        {
            navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {           
            UpdateAnimator();  
        }      

        public float MaxSpeed()
        {
            return maxSpeed;
        }     

        public void RotateTowards(Transform target)
        {
            int rotSpeed = 160;
            var targetToLook = Quaternion.LookRotation(target.transform.position - this.transform.position);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetToLook, rotSpeed * Time.deltaTime);            
        }       

        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            MoveTo(destination, speedFraction);            
        }

        public void MoveTo(Vector3 destination, float speedFraction)
        {   
            navMeshAgent.isStopped = false;      
            navMeshAgent.destination = destination; 
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
        }

        public void CancelNav()
        {            
            navMeshAgent.isStopped = true;
            rb.velocity = Vector3.zero;
        }
               
        private void UpdateAnimator()
        {
            anim.SetFloat("Locomotion", maxSpeed);         
        }


        IEnumerator NavMeshDelay()
        {
            yield return new WaitForSeconds(1f);
            navMeshAgent.enabled = true;
        }        
    }
