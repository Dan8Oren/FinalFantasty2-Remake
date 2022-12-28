using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    public CharacterScriptableObject[] heroes;
    public int FightLevel
    {
        get { return curLevel; } 
        set{curLevel = value;}
        
    }
    [SerializeField] private float coins;
    [SerializeField] private int curLevel;
    private void Awake()
    {
        //singleton pattern the prevent two scene managers
        if ( Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject); 
        }

        FightLevel = 0;

    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
