using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Button rollButton, increaseBetButton, increaseBetButtonTen, decreaseBetButtonTen, decreaseBetButton;
    [SerializeField]
    private TextMeshProUGUI coinText, lastWinText, bonusText, bonusSpinText, currentBonusPayoutText, betSizeText, titleText;
    [SerializeField]
    private Toggle lightningToggle;

    GameManager gameManager;

    private Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();

    public void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void SetCoinText(string text)
    {
        coinText.text = text;
    }

    public void StartSpin(bool notInBonus)
    {
        rollButton.enabled = false;
        decreaseBetButton.enabled = false;
        increaseBetButton.enabled = false;
        increaseBetButtonTen.enabled = false;
        decreaseBetButtonTen.enabled = false;
        lightningToggle.enabled = false;
        if (notInBonus)
        {
            titleText.gameObject.SetActive(true);
            bonusSpinText.gameObject.SetActive(false);
            currentBonusPayoutText.gameObject.SetActive(false);
        }
    }

    public void TriggeredBonus(int lastWin, int wonSpins, int coinsWonTracker)
    {
        lastWinText.text = "Last Win: " + lastWin;
        bonusText.text = "You Have Won " + wonSpins + " Fruit Madness Spins!";
        bonusText.gameObject.SetActive(true);
        titleText.gameObject.SetActive(false);
        currentBonusPayoutText.text = "Total Win: " + coinsWonTracker;
        currentBonusPayoutText.gameObject.SetActive(false);
        bonusSpinText.gameObject.SetActive(false);
    }

    public void EndSpin(int payResult, bool bonusEnded, int coinsWonTracker)
    {
        lastWinText.text = "Last Win: " + payResult;
        rollButton.enabled = true;
        decreaseBetButton.enabled = true;
        increaseBetButton.enabled = true;
        decreaseBetButtonTen.enabled = true;
        increaseBetButtonTen.enabled = true;
        lightningToggle.enabled = true;
        if (bonusEnded)
        {
            currentBonusPayoutText.text = "Total Win: " + coinsWonTracker;
        }
    }

    public void EndBonusSpin(int coinsWonTracker, int payResult)
    {
        currentBonusPayoutText.text = "Total Win: " + coinsWonTracker;
        lastWinText.text = "Last Win: " + payResult;
    }

    public void StartBonusSpin(int currentSpin, int maxSpins)
    {
        bonusText.gameObject.SetActive(false);
        currentBonusPayoutText.gameObject.SetActive(true);
        bonusSpinText.gameObject.SetActive(true);
        bonusSpinText.text = "Bonus Spin: " + currentSpin + " / " + maxSpins;
    }

    public void IncreaseBetSize(int betSize)
    {
        gameManager.SetCoinBetSize(gameManager.GetCoinBetSize() + betSize);
        betSizeText.text = "Bet Size: " + gameManager.GetCoinBetSize();
    }

    public void DecreaseBetSize(int betSize)
    {
        if (gameManager.GetCoinBetSize() - betSize >= 10)
        {
            gameManager.SetCoinBetSize(gameManager.GetCoinBetSize() - betSize);
        }
        else
        {
            gameManager.SetCoinBetSize(10);
        }
        betSizeText.text = "Bet Size: " + gameManager.GetCoinBetSize();
    }

    public void LightningSpinChange(bool newBool)
    {
        gameManager.SetLightning(newBool);
    }
}
