using System;
using UnityEngine;

[Serializable]
public class ResourceCost
{
    public ResourceData resourceType;
    public int amount;

    [SerializeField]
    private int lastAmount;
    
    // public void ResetAmount()
    // {
    //     if (amount != 0)
    //     {
    //         lastAmount = amount;
    //         amount = 0;
    //     }
    // }

    // public void RestoreAmount()
    // {
    //     amount = lastAmount;
    // }

    public void ChangeAmount(int new_amount)
    {
        if (lastAmount != amount)
            lastAmount = amount;

        if (amount != new_amount)
            amount = new_amount;
    }
}
