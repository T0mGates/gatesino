using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Symbol : MonoBehaviour
{
    public GameObject twoMulti, fiveMulti, tenMulti, twentyFiveMulti;
    public int multiplier;
    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("GameManager");
        bool bonus = player.GetComponent<GameManager>().fruitMadnessBonus;
        if (bonus)
        {
            int randNum = Random.Range(1, 101);
            if (randNum > 75 && randNum < 86)
            {
                twoMulti.SetActive(true);
                multiplier = 2;
            }
            else if (randNum > 85 && randNum < 93)
            {
                fiveMulti.SetActive(true);
                multiplier = 5;
            }
            else if (randNum > 92 && randNum < 98)
            {
                tenMulti.SetActive(true);
                multiplier = 10;
            }
            else if (randNum > 97 && randNum < 101)
            {
                twentyFiveMulti.SetActive(true);
                multiplier = 25;
            }
            else
            {
                multiplier = 0;
            }
        }
        
    }

}
