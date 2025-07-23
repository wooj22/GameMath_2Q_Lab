using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Image bossHp_Image;
    [SerializeField] Image boost_Image;
    [SerializeField] Image playerHp_Image;
    [SerializeField] Text end_text;

    private float bossMaxHp;
    private float playerMaxHp;
    private float boostCoolTime;

    public static UIManager Instance { get; private set; }

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

    public void SetBossMaxHp(float hp) 
    {
        bossMaxHp = hp; 
    }

    public void SetPlayerMaxHp(float hp)
    {
        playerMaxHp = hp;
    }

    public void SetBoostCoolTime(float time) 
    {
        boostCoolTime = time; 
    }

    public void UpdateBossHp(float hp)
    {
        bossHp_Image.fillAmount = hp / bossMaxHp;
    }

    public void UpdatePlayerHp(float hp)
    {
        playerHp_Image.fillAmount = hp / playerMaxHp;
    }

    public void UpdatePlayerBoost(float time)
    {
        boost_Image.fillAmount = time/ boostCoolTime;
    }

    public void UpDateEndText(string text)
    {
        end_text.text = text;
    }
}
