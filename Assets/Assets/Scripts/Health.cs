using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Health : MonoBehaviour
{
        //public LazyValue<float> healthPoints; 
        //LazyValue<float> HealthPoints { get{return healthPoints;} set{healthPoints = value;}}
        [SerializeField] TextMeshProUGUI displayLives;
        
        [SerializeField] GameObject RigLayer;
        [SerializeField] GameObject headFX;
        [SerializeField] GameObject legFX; 
        [SerializeField] GameObject armFX;
        [SerializeField] GameObject shield;
        [SerializeField] GameObject deathSplashScreen;
        [SerializeField] GameObject gameOverSplashScreen;
        [SerializeField] Mover mover;
        
        public delegate void TargetDeath();
        public static event TargetDeath targetDeath;
        public delegate void PlayerDied(GameObject obj);
        public static event PlayerDied playerDeath;
        public delegate void CompanionDied(GameObject obj);
        public static event CompanionDied companionDeath;
        public delegate void AIHit();
        public static event AIHit aIHit;
        UnityEngine.AI.NavMeshAgent agent;
        public float healthPts;
        bool isActivePlayer;
        
        
        GameObject hitScreenFX;
        CapsuleCollider capCol;
        
        float isDeadTimer = 0f;
        
        public bool isDead;
       
        FieldOFView FOV;        
        Fighter fighter;
        float damage;        
        Animator anim;
        int dieRanNum;        
        Rigidbody rb;
        int lives;


        void Awake() 
        {
            //healthPoints = new LazyValue<float>(GetInitialHealth);
            anim = GetComponent<Animator>();
        }

        void Start() 
        {
            //wrapper = GetComponent<SavingWrapper>();
            FOV = GetComponent<FieldOFView>();
            //combatTarget = GetComponent<CombatTarget>();
            //enemySpawner = FindObjectOfType<EnemySpawner>();
            rb = GetComponent<Rigidbody>(); 
            fighter = GetComponent<Fighter>(); 
            capCol = GetComponent<CapsuleCollider>();  
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (this.gameObject.CompareTag("Player"))
            {
                hitScreenFX = GameObject.Find(name: "/PlayerCore/HUD/DamageScreen");
                hitScreenFX.SetActive(false);
                //vitals = GetComponent<PlayerVitals>();
                LivesUpdate();
            }
        }       

        void Update()
        {
            isDeadTimer += Time.deltaTime;
           // healthPts = healthPoints.value;
        }

        public bool IsDead()
        {
            return isDead;
        } 

        public void TakeDamage(float damage)
        {  
            if (isDead == true) return;
            if (this.gameObject.CompareTag("Enemy") || (this.gameObject.CompareTag("Companion")))
            {
                aIHit?.Invoke();
            }            

            if (this.gameObject.CompareTag("Player")) // && (vitals != null))
            {
                StartCoroutine("HitFX");
                //vitals.TakeDamage(damage);
                if (isDead == true) return;
                anim.SetTrigger("HitAnim");
            }          

           // healthPoints.value = Mathf.Max(healthPoints.value - damage, 0);
            mover.CancelNav();
            //HitAnim();
           // HealthCheck();
        }

        // public void HealthCheck()
        // {
        //     //Debug.Log("HealthCheckCalled" + " " + gameObject.name + healthPoints.value);
        //     if (healthPoints.value <= 0)
        //     {
        //         if (gameObject.name == ("SuicideDrone"))
        //         {
        //             //var dronCon = GetComponent<DroneCon>();
        //             //dronCon.DroneDeath();
        //             return;
        //         }
        //         //enemySpawner.RemoveEnemy(gameObject);
        //         healthPoints.value = 0f;
        //         if (isDeadTimer <= 1f)
        //         { 
        //             Destroy(gameObject);
        //         }
        //         else
        //         {
        //             Die();
        //         }
        //     }            
        // } 
        
        public void RestoreHealth()
        {
            //healthPoints = new LazyValue<float>(GetInitialHealth);
            //vitals.Restore();
        }
 
        public void Die()
        {
            isDead = true;
            if (this.CompareTag("Enemy"))
            {
                //var aiCon = GetComponent<AIController>();
                //aiCon.isDead = true;
                Transform selectionUI = transform.Find("SelectionIcon");
                var selObj = selectionUI.gameObject;
                selObj.SetActive(false);
            }

            //var rigBuilder = GetComponent<RigBuilder>();
            //rigBuilder.enabled = false;
            //var capCol = GetComponent<CapsuleCollider>();
            capCol.enabled = false;
            fighter.enabled = false;

            anim.SetBool("HitAnim", false);
            dieRanNum = Random.Range(1, 4);
            if (this.gameObject.CompareTag("Companion"))
            {
                shield.SetActive(false);
                companionDeath?.Invoke(this.gameObject);
                //update AI to change target
                
                //check length of available players
            }
            else if (this.gameObject.CompareTag("Player"))
            {
                shield.SetActive(false);               
                playerDeath?.Invoke(this.gameObject);
                //check if infiniteLives
                //lives--;
                //LevelManager.Instance.lifeWasLost = true;
                //displayLives.text = lives.ToString();
                //SavingWrapper wrapper = FindObjectOfType<SavingWrapper>();
                //healthPoints = new LazyValue<float>(GetInitialHealth);
                //wrapper.Save();
            }
            
            transform.position += new Vector3(0,0,-2f) * 10f * Time.deltaTime;
            if(dieRanNum == 1)
            {
                anim.speed = 2.5f;
                anim.SetTrigger("Die1");
                armFX.transform.SetParent(null);
                armFX.SetActive(true);
                if(this.CompareTag("Enemy"))
                {                  
                  //AudioManager.PlayHumanSound(AudioManager.HumanSound.Death1, this.transform.position);
                  this.tag = "Dead";                  
                  targetDeath?.Invoke();
                }
                else
                {
                   //PlayerDeath(); 
                }                              
            }
            else if (dieRanNum == 2)
            {
                anim.speed = 2.5f;
                anim.SetTrigger("Die2");
                headFX.SetActive(true);
                if(this.CompareTag("Enemy"))
                {                  
                  //AudioManager.PlayHumanSound(AudioManager.HumanSound.Death2, this.transform.position); 
                  this.tag = "Dead";
                  targetDeath?.Invoke();                  
                } 
                else
                {
                   //PlayerDeath(); 
                }                                            
            }
            else if (dieRanNum == 3)
            {
                anim.speed = 2.5f;
                anim.SetTrigger("Die3");
                legFX.SetActive(true); 
                if(this.CompareTag("Enemy"))
                {
                  //AudioManager.PlayHumanSound(AudioManager.HumanSound.Death3, this.transform.position); 
                  this.tag = "Dead";
                  targetDeath?.Invoke();
                }
                else
                {
                   //PlayerDeath(); 
                }
            }                     
        }

        public void HitTheFloor() 
        {
            //AudioManager.PlayHumanSound(AudioManager.HumanSound.HumanHitGroundDeath, this.transform.position);
        }  

        public object CaptureState()
        {
            //Debug.Log(this.gameObject.name + " " + HealthPoints.value);
            Dictionary<string, object> data = new Dictionary<string, object>();
            //data["healthPoints.value"] = healthPoints.value;
            if (this.gameObject.name == "Rambler")
            {
                data["lives"] = lives;
            }
            return data;
        }

        public void AdWatched()
        {
            lives += 3;
            displayLives.text = lives.ToString();
        }

        public void RestoreState(object state)
        {
            Dictionary<string, object> data = (Dictionary<string, object>)state;
            //healthPoints.value = (float)data["healthPoints.value"];
            if ((this.gameObject.CompareTag("Player")) || (this.gameObject.CompareTag("Companion")))
            {
                //check if lives are infinite
                lives = (int)data["lives"];
                LivesUpdate();
            }            
            //HealthCheck();
        }

        public void DeathSpeedNormal()
        {
            anim.speed = 1f;
        }

        public void StartingLives()
        {
            //check if lives are infinite
            lives = 5;
            displayLives.text = lives.ToString();
        }

        public void AddOneLife()
        {
            lives++;
        }
       
        public void LivesUpdate()
        {
            displayLives.text = lives.ToString();
            //if infinite set UI to disabled
        }

        // float GetInitialHealth()
        // {
        //     //return GetComponent<BaseStats>().GetStat(Stat.Health);
        // }

        // void HitAnim()
        // {  
        //     if(isDead) return;
        //     if(gameObject.name == ("SuicideDrone"))
        //     {
        //         //var droneCon = GetComponent<DroneCon>();
        //         //droneCon.DroneDeath();
        //         return;
        //     }
        //     anim.SetTrigger("HitAnim");
        //     if(gameObject.CompareTag("Player")) return; 
        //     //FOV.radius = 40f;
        //     //AudioManager.PlayHumanSound(AudioManager.HumanSound.Hit1, position: this.transform.position); 
        // }
        
        //void OnParticleCollision(GameObject particleProj)
       // {           
            //var proj = particleProj.GetComponent<Projectile>();
            //damage = proj.GetDamage();
            //TakeDamage(damage);

           // if (proj.HitEffect() != null)
          //  {                    
             // var cloneProjectile = Instantiate(proj.HitEffect(), proj.GetAimLocation(), particleProj.transform.rotation);                      
              
              //Destroy(proj.gameObject);
               //MF_AutoPool.Despawn(gameObject);     
           // }
           // else
            //{
              //Destroy(proj.gameObject);
               //MF_AutoPool.Despawn(gameObject);
           // }       
       // }  

        // void PlayerDeath() 
        // {
        //     this.tag = "Dead";
        //     fighter.enabled = false;
        //     if (lives > 0) //check if lives are infinite
        //     {
        //         deathSplashScreen.SetActive(true);
        //     }
        //     else
        //     {
        //         gameOverSplashScreen.SetActive(true);
        //     }
        //         //AudioManager.PlayHumanSound(AudioManager.HumanSound.Death4, transform.position); 
        //         //AudioManager.PlayAmbientSound(AudioManager.AmbientSound.DeathScreen);            
        //     //if(this.gameObject.name == "Rambler") {LevelManager.Instance.PlayerWeaponCheck();}
        // }

        // IEnumerator HitFX()
        // {
        //     //shakeCamera
        //     hitScreenFX.SetActive(true);
        //     yield return new WaitForSeconds(0.3f);
        //     hitScreenFX.SetActive(false);
        // }
    }

