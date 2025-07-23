using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject endObj;
    private bool isGameEnd;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if(isGameEnd && Input.GetKeyDown(KeyCode.Space))
        {
            endObj.SetActive(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void GameClear()
    {
        isGameEnd = true;
        endObj.SetActive(true);
        UIManager.Instance.UpDateEndText("Game Clear!");
    }

    public void GameOver()
    {
        isGameEnd = true;
        endObj.SetActive(true);
        UIManager.Instance.UpDateEndText("Game Over");
    }
}
