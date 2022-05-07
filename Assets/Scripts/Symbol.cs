using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Symbol : MonoBehaviour
{
    public GameObject twoMulti, threeMulti, fiveMulti, tenMulti, twentyFiveMulti;
    public int multiplier;
    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("GameManager");
        bool bonus = player.GetComponent<GameManager>().IsInFruitMadnessBonus();
        if (bonus)
        {
            int randNum = Random.Range(1, 101);
            if (randNum > 73 && randNum < 83)
            {
                twoMulti.SetActive(true);
                multiplier = 2;
            }
            else if (randNum > 82 && randNum < 90)
            {
                threeMulti.SetActive(true);
                multiplier = 3;
            }
            else if (randNum > 89 && randNum < 96)
            {
                fiveMulti.SetActive(true);
                multiplier = 5;
            }
            else if (randNum > 95 && randNum < 99)
            {
                tenMulti.SetActive(true);
                multiplier = 10;
            }
            else if (randNum > 98 && randNum < 101)
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
