using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOFView : MonoBehaviour
{
   public float radius;
   [Range(0,360)]
   public float angle;
    
   [SerializeField]
   public List<GameObject> enemies = new();

   float currentTime = 0;
   private GameObject player;
   public float viewRadius = 10f; 
   public Vector3 lastKnownPlayerPos;
   public Transform playerTransform;
   public LayerMask obstructionMask; 
   public LayerMask playerMask;
   public bool canSeePlayer;
   EnemyAI enemyAI;

   private void OnEnable()
   {     
     player = GameObject.FindGameObjectWithTag("Player");
     if (player == null)
     {
       Debug.LogWarning("No player found");
     }
     enemyAI = GetComponent<EnemyAI>();
     FindColliders();
   }      

   void FindColliders()
   {
     enemies.Clear();
     // If THIS object is an enemy, then find all other enemies
     if (CompareTag("Enemy"))
     {
       enemies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));

       // Remove itself from the list
       enemies.Remove(this.gameObject);
     }
   }
   
   public bool CanSeePlayer(Transform target)
   {
     if (target == null) return false;

     Vector3 directionToTarget = (target.position - transform.position);
     float distanceToTarget = directionToTarget.magnitude;

     // Check if target is within view radius
     if (distanceToTarget > viewRadius)
       return false;

     // Check if there is an obstacle in the way
     if (Physics.Raycast(transform.position, directionToTarget.normalized, distanceToTarget, 
           obstructionMask))
       return false;

     // Optional: check if target is within a field-of-view angle
     // float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
     // if (angleToTarget > viewAngle / 2) return false;

     return true; // No obstacle, within range
   }

    public void LateUpdate()
    {
      currentTime += Time.deltaTime;

      if (currentTime >= 0.2f)
      {
        canSeePlayer = false;
        FieldOfViewCheck();
        currentTime = 0f;
      }
    }

    private void FieldOfViewCheck()
    {
      if (player == null)
        return;
      
      lastKnownPlayerPos = playerTransform.position;

      Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

      // Check if player is inside radius
      float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
      if (distanceToPlayer > radius)
      {
        canSeePlayer = false;
        return;
      }

      // Check if inside cone angle
      if (Vector3.Angle(transform.forward, directionToPlayer) > angle / 2)
      {
        canSeePlayer = false;
        return;
      }

      // Check if obstructed
      if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstructionMask))
      {
        canSeePlayer = false;
        return;
      }

      // Player is visible
      canSeePlayer = true;
    }

}
