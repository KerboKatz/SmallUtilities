using KerboKatz.Assets;
using KerboKatz.Extensions;
using KerboKatz.Toolbar;
using KerboKatz.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KerboKatz.DF
{
  [KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames, GameScenes.SPACECENTER)]
  public class DailyFundsScenario : ScenarioModule
  {

    public override void OnLoad(ConfigNode node)
    {
      DailyFunds.instance.setup = node.GetBoolValue("Setup");
      if (DailyFunds.instance.setup)
      {
        DailyFunds.instance.lastTime = node.GetIntValue("lastTime");
      }
    }

    public override void OnSave(ConfigNode node)
    {
      DailyFunds.instance.Log("DailyFundsScenario Save");
      node.AddValue("lastTime", DailyFunds.instance.lastTime);
      node.AddValue("Setup", DailyFunds.instance.setup);
    }
  }
}