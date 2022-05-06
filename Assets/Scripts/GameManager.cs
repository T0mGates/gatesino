using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum SlotBonus
    {
        FruitMadness,
        None
    }
    private SlotBonus currentBonus = SlotBonus.None;

    [System.Serializable]
    public class Row
    {
        public Vector3[] slotLocation;
    }
    public Row[] rows;

    [System.Serializable]
    public class SymbolObject
    {
        public GameObject symbol;
        public int intervalSpawnChance;
        public float payMultiplier;
        public string symbolName;
    }
    public SymbolObject[] symbols;

    [System.Serializable]
    public class Column
    {
        public Vector3 spawnLocation;
        public float timeToEndSpin;
        public bool canSpawnSymbol;
        public float timeToNextSpawn;
        public float timeToStop;
        public float speed;
        public float spawnTimer;
        public bool canMove;
    }
    public Column[] columns;

    private List<GameObject> usedObjects = new List<GameObject>();

    private bool rolling = false;

    [SerializeField] private float timeToNextSpawnBase;
    [SerializeField] private float initialSpeedPerFrame = -0.02f;
    private List<GameObject> currentObjects = new List<GameObject>();
    private bool timerActivated = false;
    private List<List<GameObject>> slotMatrix = new List<List<GameObject>>();
    private float distance = 0;
    private float smallestDistance = 0;
    private bool moveObjects = false;
    private int coinBetSize = 10;
    public float spawnDecreaseInterval, speedDecreaseInterval, moveObjectsInterval;
    public string wildObjectName, scatterObjectName;
    public int requiredScattersForBonus;
    [SerializeField] private int baseBonusSpins;
    public SlotBonus slotBonus;

    private PlayerManager playerStats;
    public Button rollButton;
    public Button decreaseBetButton;
    public Button increaseBetButton, increaseBetButtonTen, decreaseBetButtonTen;
    public Text coinText;
    public Text betSizeText;
    public Text lastWinText;
    public Text bonusText;
    public Text bonusSpinText;
    public Text currentBonusPayoutText;

    private int currentBonusSpin, currentMaxBonusSpins;
    private int coinsWonTracker = 0;
    private float lightningSpawnDecreaseInterval, lightningSpeedDecreaseInterval, lightningMoveObjectsInterval, lightningInitialSpeedPerFrame, lightningTimeToNextSpawnBase;
    private bool lightning = false;
    public Toggle lightningToggle;


    void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
        lightningSpawnDecreaseInterval = spawnDecreaseInterval;
        lightningSpeedDecreaseInterval = speedDecreaseInterval * 4;
        lightningMoveObjectsInterval = moveObjectsInterval * 6;
        lightningInitialSpeedPerFrame = initialSpeedPerFrame * 3f;
        lightningTimeToNextSpawnBase = timeToNextSpawnBase / 3f;
    }

    void Update()
    {
        if (rolling)
        {
            if (timerActivated)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    if (columns[i].timeToStop - Time.time <= 1.5f && columns[i].timeToStop - Time.time > 0)
                    {
                        if (columns[i].speed != 0)
                        {
                            if (!lightning || currentBonus != SlotBonus.None)
                            {
                                columns[i].speed += speedDecreaseInterval * Time.deltaTime;
                                columns[i].timeToNextSpawn += spawnDecreaseInterval * Time.deltaTime;
                            }
                            else
                            {
                                columns[i].speed += lightningSpeedDecreaseInterval * Time.deltaTime;
                                columns[i].timeToNextSpawn += lightningSpawnDecreaseInterval * Time.deltaTime;
                            }
                        }
                    }
                    else if (columns[i].timeToStop - Time.time <= 0)
                    {
                        columns[i].speed = 0;
                        columns[i].canSpawnSymbol = false;
                        columns[i].timeToStop += 1000;
                        ArrangeCol(i);
                    }
                }
                bool stoppedSlot = true;
                for(int i = 0; i < columns.Length; i++)
                {
                    if(columns[i].speed != 0)
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
                for (int i = 0; i < columns.Length; i++)
                {
                    if (lightning && currentBonus == SlotBonus.None)
                    {
                        columns[i].timeToStop = Time.time + (columns[i].timeToEndSpin / 3);
                        columns[i].speed = lightningInitialSpeedPerFrame;
                    }
                    else
                    {
                        columns[i].timeToStop = Time.time + columns[i].timeToEndSpin;
                        columns[i].speed = initialSpeedPerFrame;
                    }
                    timerActivated = true;
                }
                
                for (int i = 0; i < columns.Length; i++)
                {
                    columns[i].spawnTimer = Time.time + columns[i].timeToNextSpawn;
                    
                }
            }

            //spawning
            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i].spawnTimer < Time.time && columns[i].canSpawnSymbol)
                {
                    columns[i].spawnTimer = Time.time + columns[i].timeToNextSpawn;
                    Spawn(columns[i].spawnLocation);
                }
            }

            //moving objects down stopped slots
            foreach (GameObject symbol in currentObjects)
            {
                for(int i = 0; i < columns.Length; i++)
                {
                    if (symbol.transform.position.x == columns[i].spawnLocation.x)
                    {
                        symbol.transform.position = new Vector3(symbol.transform.position.x, symbol.transform.position.y + columns[i].speed * Time.deltaTime, symbol.transform.position.z);
                    }
                }
            }
        }
        
        if (moveObjects)
        {
            moveObjects = false;
            for(int i = 0; i < columns.Length; i++)
            {
                for (int row = 0; row < rows.Length; row++)
                {
                    if (columns[i].canMove)
                    {
                        if (slotMatrix[row][i].transform.position != rows[row].slotLocation[i])
                        {
                            if (!lightning || currentBonus != SlotBonus.None)
                            {
                                slotMatrix[row][i].transform.position = Vector3.MoveTowards(slotMatrix[row][i].transform.position, rows[row].slotLocation[i], moveObjectsInterval * Time.deltaTime);
                            }
                            else
                            {
                                slotMatrix[row][i].transform.position = Vector3.MoveTowards(slotMatrix[row][i].transform.position, rows[row].slotLocation[i], lightningMoveObjectsInterval * Time.deltaTime);
                            }
                            moveObjects = true;
                        }
                    }
                }
            }
        }
    }


    public bool IsInFruitMadnessBonus() { return currentBonus == SlotBonus.FruitMadness; }

    int ReturnCoins()
    {
        return(playerStats.coins);
    }

    void AddCoins(int coinsToAdd)
    {
        playerStats.coins += coinsToAdd;
        coinText.text = "Coins: " + playerStats.coins;
    }

    //moves object towards given target location, called until it is at target location (detect if y is greater or lower to know to move up or down)
    void ArrangeCol(int colToArrange)
    {
        List<GameObject> symbolsInCol = new List<GameObject>(); 
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Symbol");
        for (int index = 0; index < allObjects.Length; index++)
        {
            if(rows[0].slotLocation[colToArrange].x == allObjects[index].transform.position.x)
            {
                symbolsInCol.Add(allObjects[index]);
            }
        }
        
        GameObject currentObject = null;
        for(int row = 0; row < rows.Length; row++)
        {
            smallestDistance = 10000;
            distance = 0;
            foreach (GameObject symb in symbolsInCol)
            {
                //distance formula..?
                distance = Mathf.Abs(rows[row].slotLocation[colToArrange].y - symb.transform.position.y);
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
            if (!usedObjects.Contains(symb))
            {
                toRemove.Add(symb);
            }
        }
            for(int i = 0; i < toRemove.Count; i++)
            {
                currentObjects.Remove(toRemove[i]);
                Destroy(toRemove[i]);
            }
        columns[colToArrange].canMove = true;
        moveObjects = true;
    }

    public void Roll()
    {
        if(ReturnCoins() >= coinBetSize || currentBonus != SlotBonus.None)
        {
            if(currentBonus == SlotBonus.None)
            {
                currentBonusPayoutText.gameObject.SetActive(false);
                AddCoins(coinBetSize * -1);
            }
            rollButton.enabled = false;
            decreaseBetButton.enabled = false;
            increaseBetButton.enabled = false;
            increaseBetButtonTen.enabled = false;
            decreaseBetButtonTen.enabled = false;
            lightningToggle.enabled = false;
            slotMatrix.Clear();
            usedObjects.Clear();
            foreach (GameObject objects in currentObjects)
            {
                Destroy(objects);
            }
            currentObjects.Clear();
            timerActivated = false;

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i].canSpawnSymbol = true;
            }
            for (int i = 0; i < rows.Length; i++)
            {
                slotMatrix.Add(new List<GameObject>());
            }
            for (int i = 0; i < columns.Length; i++)
            {
                if (!lightning || currentBonus != SlotBonus.None)
                {
                    columns[i].timeToNextSpawn = timeToNextSpawnBase;
                }
                else
                {
                    columns[i].timeToNextSpawn = lightningTimeToNextSpawnBase;
                }
            }
            for(int i = 0; i < columns.Length; i++)
            {
                columns[i].canMove = false;
            }

            rolling = true;
        }
        else
        {
            Debug.Log("Not Enough Coins!");
        }
        
    }
    private bool CheckIfAdjacent(int index1, int index2, List<int> rowList, List<int> colList)
    {
        return((rowList[index1] + 1 == rowList[index2] || rowList[index1] - 1 == rowList[index2] || rowList[index1] == rowList[index2]) && (colList[index1] + 1 == colList[index2] || colList[index1] - 1 == colList[index2] || colList[index1] == colList[index2]));
    }
    void SlotResults()
    {
        float payResult = 0;
        int scatterCounter = 0;
        for(int nameIndex = 0; nameIndex < symbols.Length; nameIndex++)
        {
            int counter = 0;
            int multiplier = 1;
            List<int> rowList = new List<int>();
            List<int> colList = new List<int>();
            float tempResult = 0;
            for (int row = 0; row < rows.Length; row++)
            {
                for (int col = 0; col < columns.Length; col++)
                {
                    if (slotMatrix[row][col].gameObject.name == symbols[nameIndex].symbolName + "(Clone)" || slotMatrix[row][col].gameObject.name ==  wildObjectName + "(Clone)") //need to change wild name
                    {
                        rowList.Add(row);
                        colList.Add(col);
                        //condition check -> passed = counter +=1
                    }
                }
            }
            //check for if index will be out of list
            List<int> connectedIndex = new List<int>();
            for(int i = 0; i < rowList.Count; i++)
            {
                for(int iTwo = i + 1; iTwo < rowList.Count; iTwo++)
                {
                    if (!connectedIndex.Contains(iTwo) && CheckIfAdjacent(i, iTwo, rowList, colList))
                    {
                        if (currentBonus == SlotBonus.FruitMadness)
                        {
                            multiplier += slotMatrix[rowList[iTwo]][colList[iTwo]].gameObject.GetComponent<Symbol>().multiplier;
                        }
                        counter++;
                        connectedIndex.Add(iTwo);
                        if (!connectedIndex.Contains(i))
                        {
                            if (currentBonus == SlotBonus.FruitMadness)
                            {
                                multiplier += slotMatrix[rowList[i]][colList[i]].gameObject.GetComponent<Symbol>().multiplier;
                            }
                            counter++;
                            connectedIndex.Add(i);
                        }
                    } 
                }
                if(i > 0)
                {
                    if (i == rowList.Count - 1)
                    {
                        if (!connectedIndex.Contains(i - 1) && (rowList[i] + 1 == rowList[i - 1] || rowList[i] - 1 == rowList[i - 1] || rowList[i] == rowList[i - 1]) && (colList[i] + 1 == colList[i - 1] || colList[i] - 1 == colList[i - 1] || colList[i] == colList[i - 1]))
                        {
                            counter++;
                            connectedIndex.Add(i - 1);
                        }
                    }
                }
            }

            tempResult += (symbols[nameIndex].payMultiplier * coinBetSize);
            if(multiplier > 1)
            {
                multiplier -= 1;
            }
            Debug.Log(symbols[nameIndex].symbolName);
            Debug.Log("multi: " + multiplier);
            Debug.Log("counter: " + counter);
            //might need re-doing...
            if (counter > 4)
            {
                tempResult *= (counter * (counter * 0.4f));
                tempResult *= multiplier;

            }
            else if (counter == 4)
            {
                tempResult *= (counter * (counter * 0.2f));
                tempResult *= multiplier;
            }
            else if(counter == 3)
            {
                tempResult *= (counter * (counter * 0.1f));
                tempResult *= multiplier;
            }
            else if(counter == 0 || counter == 1 || counter == 2)
            {
                tempResult = 0;
            }
            else
            {
                tempResult *= counter;
            }
            Debug.Log("PAID: " + tempResult);
            payResult += tempResult;
        }
        for (int row = 0; row < rows.Length; row++)
        {
            for (int col = 0; col < columns.Length; col++)
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
            if(scatterCounter > requiredScattersForBonus)
            {
                scatterCounter = ((scatterCounter - requiredScattersForBonus) * 2) + requiredScattersForBonus;
            }
            currentMaxBonusSpins += baseBonusSpins + scatterCounter;
            currentBonus = slotBonus;
            bonusText.text = "You Have Won " + (baseBonusSpins + scatterCounter) + " Free Spins!";
            bonusText.gameObject.SetActive(true);
            currentBonusPayoutText.text = "Total Bonus Round Payout: " + coinsWonTracker + " coins";
            currentBonusPayoutText.gameObject.SetActive(true);
        }
        else
        {
            if (currentBonus == SlotBonus.None)
            {
                AddCoins((int)Mathf.Round(payResult));
                lastWinText.text = "Last Win: " + (int)Mathf.Round(payResult) + " coins";
                rollButton.enabled = true;
                decreaseBetButton.enabled = true;
                increaseBetButton.enabled = true;
                decreaseBetButtonTen.enabled = true;
                increaseBetButtonTen.enabled = true;
                lightningToggle.enabled = true;
            }
        }
        if (currentBonus == SlotBonus.FruitMadness)
        {
            coinsWonTracker += (int)Mathf.Round(payResult);
            currentBonusPayoutText.text = "Total Bonus Round Payout: " + coinsWonTracker + " coins";
            lastWinText.text = "Last Win: " + (int)Mathf.Round(payResult) + " coins";
            currentBonusSpin += 1;
            if (currentBonusSpin > currentMaxBonusSpins)
            {
                AddCoins((int)Mathf.Round(coinsWonTracker));
                currentBonusPayoutText.text = "Total Bonus Round Payout: " + coinsWonTracker + " coins in " + currentMaxBonusSpins + " Spins";
                currentBonus = SlotBonus.None;
                currentMaxBonusSpins = 0;
                currentBonusSpin = 0;
                coinsWonTracker = 0;
                rollButton.enabled = true;
                decreaseBetButton.enabled = true;
                increaseBetButton.enabled = true;
                increaseBetButtonTen.enabled = true;
                decreaseBetButtonTen.enabled = true;
                lightningToggle.enabled = true;
                bonusSpinText.gameObject.SetActive(false);
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
        Roll();
    }

        void Spawn(Vector3 location)
        {
            int randNum = Random.Range(1, 101);
            bool found = false;
            for(int i = 0; i < symbols.Length; i++)
            {
                if (!found)
                {
                    if (i == 0)
                    {
                        if (randNum > 0 && randNum <= symbols[0].intervalSpawnChance)
                        {
                            currentObjects.Add(Instantiate(symbols[i].symbol, location, Quaternion.identity));
                            found = true;
                        }
                    }
                    else
                    {
                        if (randNum > symbols[i - 1].intervalSpawnChance && randNum <= symbols[i].intervalSpawnChance)
                        {
                            currentObjects.Add(Instantiate(symbols[i].symbol, location, Quaternion.identity));
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
