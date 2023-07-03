using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{	
	public static GameManager Instance { set; get; }

	public int playerLives = 0;
	public int levelsCompleted = 0;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		Instance = this;
	}
}
