using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool cantAutoplay = false;
    private SoundManager soundManager;
    private bool autoPlay = false;
    public string BGMName;
    public string BonusBGMName;
    public enum SlotBonus
    {
        FruitMadness,
        None
    }
    public enum PossibleConnections
    {
        Adjacent
    }

    private SlotBonus currentBonus = SlotBonus.None;
    public PossibleConnections connectionToCheck;

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
        public int minSymbolsToConnect;
    }
    public SymbolObject[] symbols;

    [System.Serializable]
    public class Column
    {
        public Vector3 spawnLocation;
        public float timeToEndSpin;
        private bool canSpawnSymbol;
        private float timeToNextSpawn;
        private float timeToStop;
        private float speed;
        private float spawnTimer;
        private bool canMove;

        public float GetTimeToStop() { return timeToStop; }
        public float GetSpeed() { return speed; }
        public bool GetCanMove() { return canMove; }
        public bool GetCanSpawnSymbol() { return canSpawnSymbol; }
        public float GetTimeToNextSpawn() { return timeToNextSpawn; }
        public float GetSpawnTimer() { return spawnTimer; }
        public void SetSpeed(float spd) { speed = spd; }
        public void SetTimeToNextSpawn(float spawnTime) { timeToNextSpawn = spawnTime; }
        public void SetTimeToStop(float time) { timeToStop = time; }
        public void SetCanSpawnSymbol(bool canSpawn) { canSpawnSymbol = canSpawn; }
        public void SetSpawnTimer(float time) { spawnTimer = time; }
        public void SetCanMove(bool move) { canMove = move; }
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
    public float spawnDecreaseInterval, speedDecreaseInterval, moveToSlotInterval;
    public string wildObjectName, scatterObjectName;
    public int requiredScattersForBonus;
    [SerializeField] private int baseBonusSpins;
    public SlotBonus slotBonus;

    private PlayerManager playerStats;

    private int currentBonusSpin, currentMaxBonusSpins;
    private int coinsWonTracker = 0;
    private float lightningSpawnDecreaseInterval, lightningSpeedDecreaseInterval, lightningMoveToSlotInterval, lightningInitialSpeedPerFrame, lightningTimeToNextSpawnBase;
    private bool lightning = false;
    private UIManager ui;

    IEnumerator Start()
    {
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
        ui = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        lightningSpawnDecreaseInterval = spawnDecreaseInterval/2;
        lightningSpeedDecreaseInterval = speedDecreaseInterval * 4;
        lightningMoveToSlotInterval = moveToSlotInterval * 6;
        lightningInitialSpeedPerFrame = initialSpeedPerFrame * 3;
        lightningTimeToNextSpawnBase = timeToNextSpawnBase / 3;
        AddCoins(0);
        yield return new WaitForSeconds(0.05f);
        soundManager.PlaySound(BGMName);
    }

    void Update()
    {
        if (rolling)
        {
            if (timerActivated)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    if (columns[i].GetTimeToStop() - Time.time <= 1.5f && columns[i].GetTimeToStop() - Time.time > 0)
                    {
                        if (columns[i].GetSpeed() < 0)
                        {
                            if (!lightning || currentBonus != SlotBonus.None)
                            {
                                columns[i].SetSpeed(columns[i].GetSpeed() + (speedDecreaseInterval * Time.deltaTime));
                                columns[i].SetTimeToNextSpawn(columns[i].GetTimeToNextSpawn() + (spawnDecreaseInterval * Time.deltaTime));
                            }
                            else
                            {
                                columns[i].SetSpeed(columns[i].GetSpeed() + (lightningSpeedDecreaseInterval * Time.deltaTime));
                                columns[i].SetTimeToNextSpawn(columns[i].GetTimeToNextSpawn() + (lightningSpawnDecreaseInterval * Time.deltaTime));
                            }
                        }
                        else
                        {
                            columns[i].SetSpeed(0);
                        }
                    }
                    else if (columns[i].GetTimeToStop() - Time.time <= 0)
                    {
                        columns[i].SetSpeed(0);
                        columns[i].SetCanSpawnSymbol(false);
                        columns[i].SetTimeToStop(columns[i].GetTimeToStop() + 1000);
                        ArrangeCol(i);
                    }
                }
                bool stoppedSlot = true;
                for(int i = 0; i < columns.Length; i++)
                {
                    if(columns[i].GetCanSpawnSymbol())
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
                        columns[i].SetTimeToStop(Time.time + (columns[i].timeToEndSpin / 3));
                        columns[i].SetSpeed(lightningInitialSpeedPerFrame);
                    }
                    else
                    {
                        columns[i].SetTimeToStop(Time.time + columns[i].timeToEndSpin);
                        columns[i].SetSpeed(initialSpeedPerFrame);
                    }
                    timerActivated = true;
                }
                
                for (int i = 0; i < columns.Length; i++)
                {
                    columns[i].SetSpawnTimer(Time.time + columns[i].GetTimeToNextSpawn());
                    
                }
            }

            //spawning
            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i].GetSpawnTimer() < Time.time && columns[i].GetCanSpawnSymbol())
                {
                    columns[i].SetSpawnTimer(Time.time + columns[i].GetTimeToNextSpawn());
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
                        symbol.transform.position = new Vector3(symbol.transform.position.x, symbol.transform.position.y + columns[i].GetSpeed() * Time.deltaTime, symbol.transform.position.z);
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
                    if (columns[i].GetCanMove())
                    {
                        if (slotMatrix[row][i].transform.position != rows[row].slotLocation[i])
                        {
                            if (!lightning || currentBonus != SlotBonus.None)
                            {
                                slotMatrix[row][i].transform.position = Vector3.MoveTowards(slotMatrix[row][i].transform.position, rows[row].slotLocation[i], moveToSlotInterval * Time.deltaTime);
                            }
                            else
                            {
                                slotMatrix[row][i].transform.position = Vector3.MoveTowards(slotMatrix[row][i].transform.position, rows[row].slotLocation[i], lightningMoveToSlotInterval * Time.deltaTime);
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
        ui.SetCoinText(playerStats.coins.ToString());
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
        columns[colToArrange].SetCanMove(true);
        moveObjects = true;
        soundManager.PlaySound("ReelStop");
    }

    public void Roll()
    {
        if(ReturnCoins() >= coinBetSize || currentBonus != SlotBonus.None)
        {
            if(currentBonus == SlotBonus.None)
            {
                AddCoins(coinBetSize * -1);
            }
            ui.StartSpin(currentBonus == SlotBonus.None);
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
                columns[i].SetCanSpawnSymbol(true);
            }
            for (int i = 0; i < rows.Length; i++)
            {
                slotMatrix.Add(new List<GameObject>());
            }
            for (int i = 0; i < columns.Length; i++)
            {
                if (!lightning || currentBonus != SlotBonus.None)
                {
                    columns[i].SetTimeToNextSpawn(timeToNextSpawnBase);
                }
                else
                {
                    columns[i].SetTimeToNextSpawn(lightningTimeToNextSpawnBase);
                }
            }
            for(int i = 0; i < columns.Length; i++)
            {
                columns[i].SetCanMove(false);
            }

            rolling = true;
        }
        else
        {
            soundManager.PlaySound("Error");
        }
        
    }
    private bool CheckIfAdjacent(int index1, int index2, List<int> rowList, List<int> colList)
    {
        return((rowList[index1] + 1 == rowList[index2] || rowList[index1] - 1 == rowList[index2] || rowList[index1] == rowList[index2]) &&
            (colList[index1] + 1 == colList[index2] || colList[index1] - 1 == colList[index2] || colList[index1] == colList[index2]));
    }
    void SlotResults()
    {
        float payResult = 0;
        int scatterCounter = 0;
        List<GameObject> connectingSymbols = new List<GameObject>();
        for (int nameIndex = 0; nameIndex < symbols.Length; nameIndex++)
        {
            int minConnections = symbols[nameIndex].minSymbolsToConnect;
            int counter = 0;
            int multiplier = 1;
            List<int> rowList = new List<int>();
            List<int> colList = new List<int>();
            float tempResult = 0;
            for (int row = 0; row < rows.Length; row++)
            {
                for (int col = 0; col < columns.Length; col++)
                {
                    if (slotMatrix[row][col].gameObject.name == symbols[nameIndex].symbolName + "(Clone)" ||
                        slotMatrix[row][col].gameObject.name == wildObjectName + "(Clone)")
                    {
                        rowList.Add(row);
                        colList.Add(col);
                        //condition check -> passed = counter +=1
                    }
                }
            }
            if (connectionToCheck == PossibleConnections.Adjacent)
            {
                int[] counterMultiArray = AdjacentConnectionCheck(rowList, colList, multiplier, counter, connectingSymbols, minConnections);
                counter = counterMultiArray[0];
                multiplier = counterMultiArray[1];
                if (multiplier > 1)
                {
                    multiplier -= 1;
                }
                tempResult += (symbols[nameIndex].payMultiplier * coinBetSize);

                //formula
                if (counter >= minConnections)
                {
                    float counterFloat = counter;
                    tempResult *= (counterFloat * counterFloat * counterFloat / ((counterFloat * 2) + 12));
                    tempResult *= multiplier;
                }
                else
                {
                    tempResult = 0;
                }
            }
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

        bool triggeredBonus = false;
        if (scatterCounter >= requiredScattersForBonus)
        {
            if (scatterCounter > requiredScattersForBonus)
            {
                scatterCounter = ((scatterCounter - requiredScattersForBonus) * 2) + requiredScattersForBonus;
            }
            currentMaxBonusSpins += baseBonusSpins + scatterCounter;
            currentBonus = slotBonus;
            ui.TriggeredBonus(Mathf.FloorToInt(payResult), baseBonusSpins + scatterCounter, coinsWonTracker, BGMName, BonusBGMName);
            triggeredBonus = true;
        }
        else
        {
            if (currentBonus == SlotBonus.None)
            {
                AddCoins(Mathf.FloorToInt(payResult));
                ui.EndSpin(Mathf.FloorToInt(payResult), false, 0, triggeredBonus, BGMName, BonusBGMName);
                if (autoPlay)
                {
                    StartCoroutine(AutoplayStart());
                }
            }
        }
        if (currentBonus != SlotBonus.None)
        {
            coinsWonTracker += Mathf.FloorToInt(payResult);
            ui.EndBonusSpin(coinsWonTracker, Mathf.FloorToInt(payResult), triggeredBonus);

            currentBonusSpin += 1;
            if (currentBonusSpin > currentMaxBonusSpins)
            {
                AddCoins(Mathf.FloorToInt(coinsWonTracker));
                ui.EndSpin(Mathf.FloorToInt(payResult), true, coinsWonTracker, triggeredBonus, BGMName, BonusBGMName);
                if (autoPlay)
                {
                    StartCoroutine(AutoplayStart());
                }
                currentBonus = SlotBonus.None;
                currentMaxBonusSpins = 0;
                currentBonusSpin = 0;
                coinsWonTracker = 0;
            }
            else
            {
                StartCoroutine(BonusStall());
            }
        }
        for (int row = 0; row < rows.Length; row++)
        {
            for (int col = 0; col < columns.Length; col++)
            {
                if (!connectingSymbols.Contains(slotMatrix[row][col].gameObject))
                {
                    if (slotMatrix[row][col].gameObject.name == scatterObjectName + "(Clone)" && triggeredBonus)
                    {
                        slotMatrix[row][col].GetComponentInChildren<Animator>().SetTrigger("SpinEnd");
                    }
                    else
                    {
                        slotMatrix[row][col].GetComponentInChildren<SpriteRenderer>().color =
                        new Color(slotMatrix[row][col].GetComponentInChildren<SpriteRenderer>().color.r,
                        slotMatrix[row][col].GetComponentInChildren<SpriteRenderer>().color.g,
                        slotMatrix[row][col].GetComponentInChildren<SpriteRenderer>().color.b,
                        .4f);
                        slotMatrix[row][col].GetComponent<Symbol>().TransparentMultiplier();
                    }
                }
                else
                {
                    slotMatrix[row][col].GetComponentInChildren<Animator>().SetTrigger("SpinEnd");
                }
            }
        }
    }

    private IEnumerator AutoplayStart()
    {
        if (lightning)
        {
            yield return new WaitForSeconds(.5f);
        }
        else
        {
            yield return new WaitForSeconds(1.75f);
        }
        if (autoPlay && !rolling)
        {
            bool worked = (ReturnCoins() >= coinBetSize);
            Roll();
            if (!worked)
            {
                ui.AutoplayError();
            }
        }
    }

    private int[] AdjacentConnectionCheck(List<int> rowList, List<int> colList, int multiplier, int counter, List<GameObject> connectingSymbols, int minConnections)
    {
        int[] counterMultiArray = new int[2];
        //check for if index will be out of list
        List<int> connectedIndex = new List<int>();
        for (int i = 0; i < rowList.Count; i++)
        {
            for (int iTwo = i + 1; iTwo < rowList.Count; iTwo++)
            {
                if (CheckIfAdjacent(i, iTwo, rowList, colList) && (!connectedIndex.Contains(iTwo) || !connectedIndex.Contains(i)))
                {
                    if(!connectedIndex.Contains(iTwo))
                    {
                        if (currentBonus == SlotBonus.FruitMadness)
                        {
                            multiplier += slotMatrix[rowList[iTwo]][colList[iTwo]].gameObject.GetComponent<Symbol>().multiplier;
                        }
                        counter++;
                        connectedIndex.Add(iTwo);
                    }
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
            if (i > 0)
            {
                if (i == rowList.Count - 1)
                {
                    if (CheckIfAdjacent(i, i - 1, rowList, colList))
                    {
                        if(!connectedIndex.Contains(i - 1))
                        {
                            if (currentBonus == SlotBonus.FruitMadness)
                            {
                                multiplier += slotMatrix[rowList[i-1]][colList[i-1]].gameObject.GetComponent<Symbol>().multiplier;
                            }
                            counter++;
                            connectedIndex.Add(i - 1);
                        }
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
            }
        }
        if(counter >= minConnections)
        {
            List<List<int>> connectionsAtIndex = new List<List<int>>();
            for(int i = 0; i < connectedIndex.Count; i++)
            {
                List<int> connections = new List<int>();
                for (int iTwo = 0; iTwo < connectedIndex.Count; iTwo++)
                {
                    if(i != iTwo)
                    {
                        if(CheckIfAdjacent(connectedIndex[i], connectedIndex[iTwo], rowList, colList))
                        {
                            connections.Add(iTwo);
                        }
                    }
                }
                connectionsAtIndex.Add(connections);
            }
            List<int> numToRemove = new List<int>();
            for (int i = 0; i < connectionsAtIndex.Count; i++)
            {
                for(int iTwo = 0; iTwo < connectionsAtIndex.Count; iTwo++)
                {
                    if(i != iTwo)
                    {
                        if(connectionsAtIndex[iTwo].Count == 1 && connectionsAtIndex[iTwo][0] == i && connectionsAtIndex[i].Count == 1 && connectionsAtIndex[i][0] == iTwo && minConnections > 2)
                        {
                            if (!numToRemove.Contains(connectedIndex[i]))
                            {
                                numToRemove.Add(connectedIndex[i]);
                            }
                            if (!numToRemove.Contains(connectedIndex[iTwo]))
                            {
                                numToRemove.Add(connectedIndex[iTwo]);
                            }
                        }
                    }
                }
            }
            if(numToRemove.Count > 0)
            {
                foreach(int index in numToRemove)
                {
                    multiplier -= slotMatrix[rowList[index]][colList[index]].gameObject.GetComponent<Symbol>().multiplier;
                    counter--;
                    connectedIndex.Remove(index);
                }
            }
            for (int i = 0; i < connectedIndex.Count; i++)
            {
                if (!connectingSymbols.Contains(slotMatrix[rowList[connectedIndex[i]]][colList[connectedIndex[i]]].gameObject))
                {
                    connectingSymbols.Add(slotMatrix[rowList[connectedIndex[i]]][colList[connectedIndex[i]]].gameObject);
                }
            }
        }
        counterMultiArray[0] = counter;
        counterMultiArray[1] = multiplier;
        return counterMultiArray;
    }

    private IEnumerator BonusStall()
    {
        yield return new WaitForSeconds(2.5f);
        cantAutoplay = false;
        BonusSpins();
    }

    void BonusSpins()
    {
        ui.StartBonusSpin(currentBonusSpin, currentMaxBonusSpins);
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

    public void SetLightning(bool value) { lightning = value; }

    public bool SetAutoplay(bool value) { 
        autoPlay = value; 
        if (autoPlay && !rolling && !cantAutoplay) 
        {
            bool worked = false;
            if(ReturnCoins() >= coinBetSize)
            {
                worked = true;
            }
            Roll();
            return worked;
        }
        return true;
    }

    public void SetCoinBetSize(int amount) { if (amount > 9999) { amount = 9999; } coinBetSize = amount; }

    public bool GetIsRolling() { return rolling; }
    public int GetCoinBetSize() { return coinBetSize; }

    public void BonusCostCalc()
    {
        ui.ChangeBonusCost(coinBetSize * 100);
    }

    public void BuyBonus()
    {
        if(!rolling && ReturnCoins() >= coinBetSize * 100)
        {
            cantAutoplay = true;
            AddCoins(coinBetSize * -100);
            currentMaxBonusSpins = baseBonusSpins + requiredScattersForBonus;
            currentBonus = slotBonus;
            ui.TriggeredBonus(Mathf.FloorToInt(0),requiredScattersForBonus + baseBonusSpins, coinsWonTracker, BGMName, BonusBGMName);
            ui.EndBonusSpin(coinsWonTracker, Mathf.FloorToInt(0), true);
            currentBonusSpin += 1;
            StartCoroutine(BonusStall());
            ui.EnableAllButtons(false);
        }
        else
        {
            soundManager.PlaySound("Error");
        }
    }
}
