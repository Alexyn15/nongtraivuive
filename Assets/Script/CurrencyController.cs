using System;
using UnityEngine;

public class CurrencyController : MonoBehaviour
{
    public static CurrencyController Instance;

    [SerializeField] private int startingGold = 100;
    private int playerGold = 100;
    public event Action<int> OnGlodChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
          Destroy(gameObject);
        }
        else
        {
            Instance = this;
            playerGold = startingGold;
        }
    }

    public int GetGold() => playerGold;

    public bool SpendGold(int amount)
    {
        if(playerGold >= amount)
        {
            playerGold -= amount;
            OnGlodChanged?.Invoke(playerGold);
            return true; //spent gold
        }
        return false;// not enough gold
    }

    public void AddGold(int amount)
    {
        playerGold += amount;
        OnGlodChanged?.Invoke(playerGold);
    }

    public void SetGold(int amount)
    {
        playerGold = amount;
        OnGlodChanged?.Invoke(playerGold);
    }
}
