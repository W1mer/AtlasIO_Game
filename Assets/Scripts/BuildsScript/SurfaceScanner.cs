using System.Collections.Generic;
using UnityEngine;

public class SurfaceScanner : BuildingInstance
{
    public override void OnDayEnd()
    {
        foreach(var continent in resourceDepositManager.DepositsOnContinentCopy)
        {
            if (continent.continent == resourceManager.CurrentContinent && continentBuilded == resourceManager.CurrentContinent)
            {
                for (int i = 0; i < continent.deposits.Count; i++)
                {
                    if (continent.deposits[i].IsDiscovered == false)
                    {
                        continent.deposits[i].Discover();
                        return;
                    }
                    continue;
                }
            }
        }
            
    }
}
