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
using System.Collections;

namespace KerboKatz.DF
{
  [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
  public class DailyFunds : KerboKatzBase<Settings>
  {
    internal double lastTime = 0;
    internal bool setup = false;

    internal static DailyFunds instance;
    public DailyFunds()
    {
      modName = "DailyFunds";
      displayName = "Daily Funds";
      requiresUtilities = new Version(1, 4, 6);
      LoadSettings("SmallUtilities/DailyFunds", "Settings");
      Log("Init done!");
      instance = this;
    }
    void Start()
    {
      StartCoroutine(CheckTime());
    }
    private IEnumerator CheckTime()
    {
      if (!setup)
      {
        lastTime = TimeUtils.GetDays(Planetarium.GetUniversalTime()) * TimeUtils.SecondsInDay;
        setup = true;
      }
      var waitForSeconds = new WaitForSeconds(1);
      while (true)
      {
        var pastDays = TimeUtils.GetDays(Planetarium.GetUniversalTime() - lastTime);
        if (pastDays > 0)
        {
          Log("pastDays is at: ", pastDays);
          var rep = Mathf.Clamp(Reputation.CurrentRep, settings.repLow, settings.repHigh);
          Log("reputation is at: ", Reputation.CurrentRep, " Clamping it at ", rep);
          var funds = rep * settings.fundsPerRep * pastDays;
          Log("Giving player ", funds, " funds");
          Funding.Instance.AddFunds(funds, TransactionReasons.Progression);

          ScreenMessages.PostScreenMessage("You have been awarded " + funds + " funds!", 2, ScreenMessageStyle.UPPER_RIGHT);
          lastTime = TimeUtils.GetDays(Planetarium.GetUniversalTime()) * TimeUtils.SecondsInDay;
        }
        yield return waitForSeconds;
      }
    }
  }
}