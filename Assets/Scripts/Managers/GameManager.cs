using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Animator gameLose;
    public Animator gameWin;
    public LayerMask monsterLayer;
    public PlayerController playerController;
    private MonsterController monsterController;
    public GameObject monsterPrefab;
    public Popup popupManager;
    public MoneyCounter moneyCounter;
    private MathHandler mhandler;
    public float resetTime = 1f;
    private float waitedTime = 0;
    private float skipBtnDelay = 0;
    private GameObject monster;
    [SerializeField] private AnimatorOverrideController[] overrideControllers;
    public Sprite[] backgrounds;
    private void Awake()
    {
        Generation.clear();
        GameObject.FindGameObjectWithTag("Background").GetComponent<Image>().sprite = backgrounds[Variables.currentBackground];
 
        mhandler = GetComponent<MathHandler>();
        Instantiate(monsterPrefab, new Vector3(25, -46, 0), Quaternion.identity);
        monster = GameObject.FindGameObjectWithTag("Monster");
        monsterController = monster.GetComponent<MonsterController>();
        monster.GetComponent<Animator>().runtimeAnimatorController = overrideControllers[Random.Range(0, 2)];
    }

    private void Update()
    {
        waitTime();

        if (isPlayerLose() || isPlayerWin()) exit();
        else
        {
            updateGame();
        }
    }

    private void exit()
    {
		if (Variables.prevScene == "Book" && isPlayerWin())
        {
            int bookStar = Variables.bookStar;
            int bookLevel = PlayerPrefs.GetInt("bookLevel", 0);
            int playerScore = Mathf.Min(bookStar + 1, 3);
            PlayerPrefs.SetInt(Variables.gameType, playerScore);
            if (playerScore > bookStar) PlayerPrefs.SetInt("allStars", PlayerPrefs.GetInt("allStars", 0) + 1);
            if (playerScore >= 2) PlayerPrefs.SetInt("bookLevel", Mathf.Max(bookLevel, Variables.bookLevel + 1));
        }
        if (waitedTime > resetTime && checkInput())
        {
            waitedTime = 0;
            SceneManager.LoadScene(Variables.prevScene);
        }
    }
    private bool checkInput()
    {
        return Input.GetMouseButtonDown(0) || Input.touchCount > 0;
    }
    private void waitTime()
    {
        if (!playerController.isAlive || !monsterController.isAlive)
        {
            waitedTime += Time.deltaTime;
        }
    }
    private bool isPlayerWin()
    {
        gameWin.SetBool("endgame", !monsterController.isAlive);
        return !monsterController.isAlive;
    }
    private bool isPlayerLose()
    {
        gameLose.SetBool("endgame", !playerController.isAlive);
        return !playerController.isAlive;
    }

    private void updateGame()
    {
        skipBtnDelay += Time.deltaTime;
        if (!popupManager.opened)
        {
            if (checkInput())
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, monsterLayer);
                if (hit)
                {
                    invokePop();
                }
            }
        }
        else
        {
            mhandler.mathInput();
            if (popupManager.progreesBarEnd())
            {
                changePop(false);
            }
            if (Variables.currentAnswer == Variables.rightAnswer)
            {
                changePop(true);
            }
        }
    }
    public void invokePop()
    {
        Variables.timeToHide = 1.2f - PlayerPrefs.GetFloat("hiding_speed", 1);
        var newEquation = mhandler.genEquation();
        popupManager.showPopUp(newEquation.Item1, newEquation.Item2);
    }
    public void changePop(bool right)
    {
        if (skipBtnDelay < 0.6f) return;
        skipBtnDelay = 0;
        characterHit(right);
        var newEquation = mhandler.genEquation();
        if (isPlayerLose() || isPlayerWin()) return;
        popupManager.changePopUp(newEquation.Item1, newEquation.Item2, right);
    }
    public void closePop()
    {
        popupManager.closePopUp();
        if (isPlayerWin()) moneyCounter.StartCount();
    }
    public void characterHit(bool player)
    {
        var health = popupManager.decreaseHealth(!player);
        if (player)
        {
            playerController.attack();
            monsterController.takeHit();
        }
        else
        {
            monsterController.attack();
            playerController.takeHit();
        }
        if (health.Item1 == 0)
        {
            playerController.isAlive = false;
            closePop();
        }
        if (health.Item2 == 0)
        {
            monsterController.dead();
            closePop();
        }
    }
}
