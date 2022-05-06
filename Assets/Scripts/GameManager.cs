using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private List<GameObject> usedObjects = new List<GameObject>();
    [SerializeField] private GameObject[] symbols;
    [SerializeField] private int[] intervalSpawnChance;
    public float[] payMultipliers;
    public string[] symbolNames;
    [SerializeField] private Transform[] spawnLocations;
    [SerializeField] private float[] colTimes;
    [SerializeField] private bool[] canSpawn;
    private bool rolling = false;
    private List<float> spawnTimers = new List<float>();
    [SerializeField] private List<float> timeToNextSpawn = new List<float>();
    [SerializeField] private float timeToNextSpawnBase;
    [SerializeField] private float initialSpeedPerFrame = -0.02f;
    private List<GameObject> currentObjects = new List<GameObject>();
    private List<float> colTimesStop = new List<float>();
    private List<float> colSpeeds = new List<float>();
    private bool timerActivated = false;
    private List<List<GameObject>> slotMatrix = new List<List<GameObject>>();
    private List<List<Vector3>> slotLocations = new List<List<Vector3>>();
    public List<Vector3> slotLocationOne = new List<Vector3>();
    public List<Vector3> slotLocationThree = new List<Vector3>();
    public List<Vector3> slotLocationTwo = new List<Vector3>();
    public List<Vector3> slotLocationFour = new List<Vector3>();
    public List<Vector3> slotLocationFive = new List<Vector3>();
    private float distance = 0;
    private float smallestDistance = 0;
    public int rows, cols;
    private bool moveObjects = false;
    public int coinBetSize;
    public string wildObjectName, scatterObjectName;

    private PlayerManager playerStats;
    public Button rollButton;
    public Button decreaseBetButton;
    public Button increaseBetButton;
    public Text coinText;
    public Text betSizeText;
    public Text lastWinText;
    public Text bonusText;
    public Text bonusSpinText;
    public Text currentBonusPayoutText;

    public bool[] canMoveIndexCol;
    public bool fruitMadnessBonus = false;
    [SerializeField] private int baseBonusSpins;
    private int currentBonusSpin, currentMaxBonusSpins;
    public int requiredScattersForBonus;
    private int coinsWonTracker = 0;
    public float spawnDecreaseInterval, speedDecreaseInterval, moveObjectsInterval;
    public float lightningSpawnDecreaseInterval, lightningSpeedDecreaseInterval, lightningMoveObjectsInterval, lightningInitialSpeedPerFrame, lightningTimeToNextSpawnBase;
    private bool lightning = false;
    public Toggle lightningToggle;

    void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
        if (slotLocationOne[0] != new Vector3(-1, 0, 0))
        {
            slotLocations.Add(slotLocationOne);
        }
        if (slotLocationTwo[0] != new Vector3(-1, 0, 0))
        {
            slotLocations.Add(slotLocationTwo);
        }
        if (slotLocationThree[0] != new Vector3(-1, 0, 0))
        {
            slotLocations.Add(slotLocationThree);
        }
        if (slotLocationFour[0] != new Vector3(-1, 0, 0))
        {
            slotLocations.Add(slotLocationFour);
        }
        if (slotLocationFive[0] != new Vector3(-1, 0, 0))
        {
            slotLocations.Add(slotLocationFive);
        }
    }

    void Update()
    {
        if (rolling)
        {
            if (timerActivated)
            {
                for (int i = 0; i < colTimes.Length; i++)
                {
                    if (colTimesStop[i] - Time.time <= 1.5f && colTimesStop[i] - Time.time > 0)
                    {
                        if (colSpeeds[i] != 0)
                        {
                            if (!lightning)
                            {
                                colSpeeds[i] += speedDecreaseInterval * Time.deltaTime;
                                timeToNextSpawn[i] += spawnDecreaseInterval * Time.deltaTime;
                            }
                            else
                            {
                                colSpeeds[i] += lightningSpeedDecreaseInterval * Time.deltaTime;
                                timeToNextSpawn[i] += lightningSpawnDecreaseInterval * Time.deltaTime;
                            }
                        }
                    }
                    else if (colTimesStop[i] - Time.time <= 0)
                    {
                        colSpeeds[i] = 0;
                        canSpawn[i] = false;
                        colTimesStop[i] += 1000;
                        ArrangeCol(i);
                    }
                }
                bool stoppedSlot = true;
                for(int i = 0; i < colSpeeds.Count; i++)
                {
                    if(colSpeeds[i] != 0)
                    {
                        stoppedSlot = false;
                    }
                }
                if (stoppedSlot)
                {
                    rolling = false;
                    SlotResults();
                }
            }

            //first iteration, adding speeds/knowing when to stop
            else
            {
                for (int i = 0; i < colTimes.Length; i++)
                {
                    if (lightning)
                    {
                        colTimesStop.Add(Time.time + (colTimes[i] / 2));
                        colSpeeds.Add(lightningInitialSpeedPerFrame);
                    }
                    else
                    {
                        colTimesStop.Add(Time.time + colTimes[i]);
                        colSpeeds.Add(initialSpeedPerFrame);
                    }
                    timerActivated = true;
                }
                
                for (int i = 0; i < timeToNextSpawn.Count; i++)
                {
                    spawnTimers.Add(Time.time + timeToNextSpawn[i]);
                    
                }
            }

            //spawning
            for (int i = 0; i < timeToNextSpawn.Count; i++)
            {
                if (spawnTimers[i] < Time.time && canSpawn[i])
                {
                    spawnTimers[i] = Time.time + timeToNextSpawn[i];
                    Spawn(spawnLocations[i]);
                }
            }

            //moving objects down stopped slots
            foreach (GameObject symbol in currentObjects)
            {
                if (symbol.transform.position.x == spawnLocations[0].position.x)
                {
                    symbol.transform.position = new Vector3(symbol.transform.position.x, symbol.transform.position.y + colSpeeds[0] * Time.deltaTime, symbol.transform.position.z);
                    //if outside of screen delete
                }
                else if (symbol.transform.position.x == spawnLocations[1].position.x)
                {
                    symbol.transform.position = new Vector3(symbol.transform.position.x, symbol.transform.position.y + colSpeeds[1] * Time.deltaTime, symbol.transform.position.z);
                    //if outside of screen delete
                }
                else if (symbol.transform.position.x == spawnLocations[2].position.x)
                {
                    symbol.transform.position = new Vector3(symbol.transform.position.x, symbol.transform.position.y + colSpeeds[2] * Time.deltaTime, symbol.transform.position.z);
                    //if outside of screen delete
                }
            }
        }
        
        if (moveObjects)
        {
            moveObjects = false;
            for(int i = 0; i < canMoveIndexCol.Length; i++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (canMoveIndexCol[i])
                    {
                        if (slotMatrix[row][i].transform.position != slotLocations[row][i])
                        {
                            if (!lightning)
                            {
                                slotMatrix[row][i].transform.position = Vector3.MoveTowards(slotMatrix[row][i].transform.position, slotLocations[row][i], moveObjectsInterval * Time.deltaTime);
                            }
                            else
                            {
                                slotMatrix[row][i].transform.position = Vector3.MoveTowards(slotMatrix[row][i].transform.position, slotLocations[row][i], lightningMoveObjectsInterval * Time.deltaTime);
                            }
                            moveObjects = true;
                        }
                    }
                }
            }
        }
    }


    int ReturnCoins()
    {
        return(playerStats.GetComponent<PlayerManager>().coins);
    }

    void AddCoins(int coinsToAdd)
    {
        playerStats.GetComponent<PlayerManager>().coins += coinsToAdd;
        coinText.text = "Coins: " + playerStats.GetComponent<PlayerManager>().coins;
    }

    //moves object towards given target location, recursive function until it is at target location (detect if y is greater or lower to know to move up or down)
    void ArrangeCol(int colToArrange)
    {
        List<GameObject> symbolsInCol = new List<GameObject>(); 
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Symbol");
        for (int index = 0; index < allObjects.Length; index++)
        {
            if(slotLocations[0][colToArrange].x == allObjects[index].transform.position.x)
            {
                symbolsInCol.Add(allObjects[index]);
            }
        }
        
        GameObject currentObject = null;
        for(int row = 0; row < rows; row++)
        {
            smallestDistance = 10000;
            distance = 0;
            foreach (GameObject symb in symbolsInCol)
            {
                distance = Mathf.Abs(slotLocations[row][colToArrange].y - symb.transform.position.y);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    currentObject = symb;
                }

            }
            slotMatrix[row].Add(currentObject);
            usedObjects.Add(currentObject);
        }

        List<GameObject> toRemove = new List<GameObject>();
        foreach (GameObject symb in symbolsInCol)
        {
            if (usedObjects.Contains(symb))
            {}
            else
            {
                toRemove.Add(symb);
            }
        }
            for(int i = 0; i < toRemove.Count; i++)
            {
                currentObjects.Remove(toRemove[i]);
                Destroy(toRemove[i]);
            }
        canMoveIndexCol[colToArrange] = true;
        moveObjects = true;
    }

    public void Roll()
    {
        if(ReturnCoins() >= coinBetSize)
        {
            rollButton.enabled = false;
            decreaseBetButton.enabled = false;
            increaseBetButton.enabled = false;
            lightningToggle.enabled = false;

            colTimesStop.Clear();
            colSpeeds.Clear();
            slotMatrix.Clear();
            usedObjects.Clear();
            spawnTimers.Clear();
            foreach (GameObject objects in currentObjects)
            {
                Destroy(objects);
            }
            currentObjects.Clear();
            timerActivated = false;

            for (int i = 0; i < canSpawn.Length; i++)
            {
                canSpawn[i] = true;
            }
            for (int i = 0; i < rows; i++)
            {
                slotMatrix.Add(new List<GameObject>());
            }
            for (int i = 0; i < timeToNextSpawn.Count; i++)
            {
                if (!lightning)
                {
                    timeToNextSpawn[i] = timeToNextSpawnBase;
                }
                else
                {
                    timeToNextSpawn[i] = lightningTimeToNextSpawnBase;
                }
            }
            for(int i = 0; i < canMoveIndexCol.Length; i++)
            {
                canMoveIndexCol[i] = false;
            }

            AddCoins(coinBetSize * -1);

            rolling = true;
        }
        else
        {
            Debug.Log("Not Enough Coins!");
        }
        
    }

    void SlotResults()
    {
        float payResult = 0;
        int scatterCounter = 0;
        for(int nameIndex = 0; nameIndex < symbolNames.Length; nameIndex++)
        {
            int counter = 0;
            int multiplier = 1;
            List<int> rowList = new List<int>();
            List<int> colList = new List<int>();
            float tempResult = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (slotMatrix[row][col].gameObject.name == symbolNames[nameIndex] + "(Clone)" || slotMatrix[row][col].gameObject.name ==  wildObjectName + "(Clone)") //need to change wild name
                    {
                        rowList.Add(row);
                        colList.Add(col);
                        if (fruitMadnessBonus)
                        {
                            multiplier += slotMatrix[row][col].gameObject.GetComponent<Symbol>().multiplier;
                        }
                        //condition check -> passed = counter +=1
                    }
                }
            }
            //check for if index will be out of list
            for(int i = 0; i < rowList.Count; i++)
            {
                for(int iTwo = i + 1; iTwo < rowList.Count; iTwo++)
                {
                    if((rowList[i] + 1 == rowList[iTwo] || rowList[i] - 1 == rowList[iTwo] || rowList[i] == rowList[iTwo]) && (colList[i] + 1 == colList[iTwo] || colList[i] - 1 == colList[iTwo] || colList[i] == colList[iTwo]))
                    {
                        counter++;
                    }
                }
            }

            tempResult += (payMultipliers[nameIndex] * coinBetSize);
            if(multiplier > 1)
            {
                multiplier -= 1;
            }

            //might need re-doing...
            if (counter > 3)
            {
                tempResult *= (counter * (counter * 0.3f));
                tempResult *= multiplier;
            }
            else if (counter == 3)
            {
                tempResult *= (counter * (counter * 0.2f));
                tempResult *= multiplier;
            }
            else if(counter == 2)
            {
                tempResult *= (counter * (counter * 0.1f));
                tempResult *= multiplier;
            }
            else if(counter == 1)
            {
                tempResult = 0;
            }
            else
            {
                tempResult *= counter;
            }
            payResult += tempResult;
        }
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (slotMatrix[row][col].gameObject.name == scatterObjectName + "(Clone)")
                {
                    scatterCounter += 1;
                }
            }
        }
        
        if(scatterCounter >= requiredScattersForBonus)
        {
            lastWinText.text = "Last Win: " + (int)Mathf.Round(payResult) + " coins";
            currentMaxBonusSpins += baseBonusSpins + scatterCounter;
            fruitMadnessBonus = true;
            bonusText.text = "You Have Won " + (baseBonusSpins + scatterCounter) + " Free Spins!";
            bonusText.gameObject.SetActive(true);
            currentBonusPayoutText.text = "Total Bonus Round Payout: " + coinsWonTracker + " coins";
            currentBonusPayoutText.gameObject.SetActive(true);
        }
        else
        {
            if (!fruitMadnessBonus)
            {
                AddCoins((int)Mathf.Round(payResult));
                lastWinText.text = "Last Win: " + (int)Mathf.Round(payResult) + " coins";
                rollButton.enabled = true;
                decreaseBetButton.enabled = true;
                increaseBetButton.enabled = true;
                lightningToggle.enabled = true;
            }
        }
        if (fruitMadnessBonus)
        {
            coinsWonTracker += (int)Mathf.Round(payResult);
            currentBonusPayoutText.text = "Total Bonus Round Payout: " + coinsWonTracker + " coins";
            lastWinText.text = "Last Win: " + (int)Mathf.Round(payResult) + " coins";
            currentBonusSpin += 1;
            if (currentBonusSpin > currentMaxBonusSpins)
            {
                AddCoins((int)Mathf.Round(coinsWonTracker));
                fruitMadnessBonus = false;
                currentMaxBonusSpins = 0;
                currentBonusSpin = 0;
                coinsWonTracker = 0;
                rollButton.enabled = true;
                decreaseBetButton.enabled = true;
                increaseBetButton.enabled = true;
                lightningToggle.enabled = true;
                bonusSpinText.gameObject.SetActive(false);
                currentBonusPayoutText.gameObject.SetActive(false);
            }
            else
            {
                StartCoroutine(BonusStall());
            }
        }

    }

    private IEnumerator BonusStall()
    {
        yield return new WaitForSeconds(2.5f);
        FruitMadnessSpins();
    }

    void FruitMadnessSpins()
    {
        bonusText.gameObject.SetActive(false);
        bonusSpinText.gameObject.SetActive(true);
        bonusSpinText.text = "Bonus Spin: " + currentBonusSpin + " / " + currentMaxBonusSpins;
        rollButton.enabled = false;
        decreaseBetButton.enabled = false;
        increaseBetButton.enabled = false;
        lightningToggle.enabled = false;

        colTimesStop.Clear();
        colSpeeds.Clear();
        slotMatrix.Clear();
        usedObjects.Clear();
        spawnTimers.Clear();
        foreach (GameObject objects in currentObjects)
        {
            Destroy(objects);
        }
        currentObjects.Clear();
        timerActivated = false;

        for (int i = 0; i < canSpawn.Length; i++)
        {
            canSpawn[i] = true;
        }
        for (int i = 0; i < rows; i++)
        {
            slotMatrix.Add(new List<GameObject>());
        }
        for (int i = 0; i < timeToNextSpawn.Count; i++)
        {
            if (!lightning)
            {
                timeToNextSpawn[i] = timeToNextSpawnBase;
            }
            else
            {
                timeToNextSpawn[i] = lightningTimeToNextSpawnBase;
            }
        }
        for (int i = 0; i < canMoveIndexCol.Length; i++)
        {
            canMoveIndexCol[i] = false;
        }
        rolling = true;
    }

        void Spawn(Transform location)
    {
        int randNum = Random.Range(1, 101);
        bool found = false;
        for(int i = 0; i < intervalSpawnChance.Length; i++)
        {
            if (!found)
            {
                if (i == 0)
                {
                    if (randNum > 0 && randNum <= intervalSpawnChance[0])
                    {
                        currentObjects.Add(Instantiate(symbols[i], location.position, Quaternion.identity));
                        found = true;
                    }
                }
                else
                {
                    if (randNum > intervalSpawnChance[i - 1] && randNum <= intervalSpawnChance[i])
                    {
                        currentObjects.Add(Instantiate(symbols[i], location.position, Quaternion.identity));
                        found = true;
                    }
                }
            }
        }
    }

    public void IncreaseBetSize(int betSize)
    {
        coinBetSize += betSize;
        betSizeText.text = "Bet Size: " + coinBetSize + " coins";
    }

    public void DecreaseBetSize(int betSize)
    {
        if(coinBetSize - betSize >= 10)
        {
            coinBetSize -= betSize;
        }
        betSizeText.text = "Bet Size: " + coinBetSize + " coins";
    }

    public void LightningSpinChange(bool newBool)
    {
        lightning = newBool;
    }

}
