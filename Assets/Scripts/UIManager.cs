using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Button rollButton, increaseBetButton, increaseBetButtonTen, decreaseBetButtonTen, decreaseBetButton, decreaseBetButtonHundred, increaseBetButtonHundred, buyBonusButton, noButton, yesButton;
    [SerializeField]
    private TextMeshProUGUI coinText, lastWinText, bonusText, bonusSpinText, currentBonusPayoutText, betSizeText, titleText, bonusCostText;
    [SerializeField]
    private Toggle lightningToggle, autoPlayToggle;
    [SerializeField]
    private GameObject bonusPanel;
    private SoundManager soundManager;
    private bool autoPlay = false;

    GameManager gameManager;

    public void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
    }

    public void SetCoinText(string text)
    {
        coinText.text = text;
    }

    public void StartSpin(bool notInBonus)
    {
        soundManager.PlaySound("SpinDown");
        soundManager.PlaySound("ReelSpin");
        EnableAllButtons(false);
        if (notInBonus)
        {
            titleText.gameObject.SetActive(true);
            bonusSpinText.gameObject.SetActive(false);
            currentBonusPayoutText.gameObject.SetActive(false);
        }
    }

    public void TriggeredBonus(int lastWin, int wonSpins, int coinsWonTracker, string bgm, string bonusBgm)
    {
        soundManager.PlaySound("BonusTrigger");
        soundManager.StopSound("ReelSpin");
        soundManager.StopSound(bgm);
        soundManager.PlaySound(bonusBgm);
        lastWinText.text = "Last Win: " + lastWin;
        bonusText.text = "You Have Won " + wonSpins + " Fruit Madness Spins!";
        bonusText.gameObject.SetActive(true);
        titleText.gameObject.SetActive(false);
        currentBonusPayoutText.text = "Total Win: " + coinsWonTracker;
        currentBonusPayoutText.gameObject.SetActive(false);
        bonusSpinText.gameObject.SetActive(false);
    }

    public void EndSpin(int payResult, bool bonusEnded, int coinsWonTracker, bool triggeredBon, string bgm, string bonusBgm)
    {
        soundManager.StopSound("ReelSpin");
        if (payResult > 0)
        {
            if (!triggeredBon)
            {
                soundManager.PlaySound("Win");
            }
        }
        else
        {
            if (!triggeredBon)
            {
                soundManager.PlaySound("Lose");
            }
        }
        lastWinText.text = "Last Win: " + payResult;
        EnableAllButtons(!autoPlay);
        if (bonusEnded)
        {
            if (!triggeredBon)
            {
                soundManager.StopSound(bonusBgm);
                soundManager.PlaySound(bgm);
            }
            currentBonusPayoutText.text = "Total Win: " + coinsWonTracker;
        }
    }

    public void EndBonusSpin(int coinsWonTracker, int payResult, bool triggeredBon)
    {
        soundManager.StopSound("ReelSpin");
        if (payResult > 0)
        {
            if (!triggeredBon)
            {
                soundManager.PlaySound("Win");
            }
        }
        else
        {
            if (!triggeredBon)
            {
                soundManager.PlaySound("Lose");
            }
        }
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
        if(gameManager.GetCoinBetSize() + betSize <= 9999)
        {
            if(betSize == 1)
            {
                soundManager.PlaySound("BetUp1");
            }
            else if(betSize == 10)
            {
                soundManager.PlaySound("BetUp10");
            }
            else if(betSize == 100)
            {
                soundManager.PlaySound("BetUp100");
            }
        }
        else
        {
            soundManager.PlaySound("Error");
        }
        gameManager.SetCoinBetSize(gameManager.GetCoinBetSize() + betSize);
        betSizeText.text = "Bet Size: " + gameManager.GetCoinBetSize();
    }

    public void DecreaseBetSize(int betSize)
    {
        if (gameManager.GetCoinBetSize() - betSize >= 10)
        {
            gameManager.SetCoinBetSize(gameManager.GetCoinBetSize() - betSize);
            if (betSize == 1)
            {
                soundManager.PlaySound("BetDown1");
            }
            else if (betSize == 10)
            {
                soundManager.PlaySound("BetDown10");
            }
            else if (betSize == 100)
            {
                soundManager.PlaySound("BetDown100");
            }
        }
        else
        {
            if(gameManager.GetCoinBetSize() == 10)
            {
                soundManager.PlaySound("Error");
            }
            else
            {
                if (betSize == 1)
                {
                    soundManager.PlaySound("BetDown1");
                }
                else if (betSize == 10)
                {
                    soundManager.PlaySound("BetDown10");
                }
                else if (betSize == 100)
                {
                    soundManager.PlaySound("BetDown100");
                }
            }
            gameManager.SetCoinBetSize(10);
        }
        betSizeText.text = "Bet Size: " + gameManager.GetCoinBetSize();
    }

    public void LightningSpinChange(bool newBool)
    {
        if (newBool)
        {
            soundManager.PlaySound("LightningDown");
        }
        else
        {
            soundManager.PlaySound("LightningUp");
        }
        gameManager.SetLightning(newBool);
    }

    public void AutoplayChange(bool newBool)
    {
        if (newBool)
        {
            soundManager.PlaySound("LightningDown");
        }
        else
        {
            soundManager.PlaySound("LightningUp");
        }
        autoPlay = newBool;
        if (autoPlay)
        {
            EnableAllButtons(false);
        }
        else
        {
            if (!gameManager.GetIsRolling())
            {
                EnableAllButtons(true);
            }
        }
        bool worked = gameManager.SetAutoplay(newBool);
        if (!worked)
        {
            AutoplayError();
        }
    }

    public void AutoplayError()
    {
        soundManager.PlaySound("Error");
        EnableAllButtons(true);
        autoPlay = false;
        autoPlayToggle.GetComponent<Toggle>().isOn = false;
    }

    public void EnableAllButtons(bool value)
    {
        decreaseBetButton.enabled = value;
        decreaseBetButtonTen.enabled = value;
        decreaseBetButtonHundred.enabled = value;
        increaseBetButton.enabled = value;
        increaseBetButtonTen.enabled = value;
        increaseBetButtonHundred.enabled = value;
        rollButton.enabled = value;
        lightningToggle.enabled = value;
        buyBonusButton.enabled = value;
        noButton.enabled = value;
        yesButton.enabled = value;
    }

    public void ShowBonusPanel(bool value)
    {
        if (value)
        {
            soundManager.PlaySound("BonusButton");
        }
        else
        {
            soundManager.PlaySound("NoButton");
        }
        bonusPanel.SetActive(value);
    }

    public void ChangeBonusCost(int amount)
    {
        bonusCostText.text = amount.ToString();
    }
}
