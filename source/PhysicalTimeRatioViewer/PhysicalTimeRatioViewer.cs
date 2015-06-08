using System;
using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.Flight, false)]
  public partial class PhysicalTimeRatioViewer : KerboKatzBase
  {
    private float thisTime;
    private float lastTime;
    private float ThisRealTime;
    private float lastRealTime;
    private float nextGametimeToRealtimeCheck;
    private float gameTimeToRealtime;
    public PhysicalTimeRatioViewer()
    {
      modName = "PhysicalTimeRatioViewer";
      displayName = "Physical time ratio viewer";
      tooltip = "Use left click to show/hide the physical time ratio viewer overlay.\n Use right click to open the settings menu.";
      requiresUtilities = new Version(1, 2, 10);
    }

    protected override void Started()
    {
      currentSettings.load("SmallUtilities/PhysicalTimeRatioViewer", "PhysicalTimeRatioViewerSettings", modName);
      currentSettings.setDefault("refreshRate", "0.5");
      currentSettings.setDefault("anchorOptionSelected", "4");
      anchorOptionSelected = currentSettings.getInt("anchorOptionSelected");
      refreshRate = currentSettings.getFloat("refreshRate");
      position.x = currentSettings.getFloat("gaugePosX");
      position.y = currentSettings.getFloat("gaugePosY");
      hideOnUIHidden = currentSettings.getBool("hideOnUIHidden");

      setIcon(Utilities.getTexture("icon", "SmallUtilities/PhysicalTimeRatioViewer/Textures"));
      setAppLauncherScenes(ApplicationLauncher.AppScenes.ALWAYS);
      GameEvents.onGamePause.Add(onPause);
      GameEvents.onGameUnpause.Add(onUnpause);
    }

    private void onUnpause()
    {
      nextGametimeToRealtimeCheck = Time.realtimeSinceStartup;
      thisTime = Time.time;
      ThisRealTime = Time.realtimeSinceStartup;
    }

    private void onPause()
    {
      gameTimeToRealtime = 0;
    }

    public void FixedUpdate()
    {
      if (nextGametimeToRealtimeCheck < Time.realtimeSinceStartup)
      {
        nextGametimeToRealtimeCheck = Time.realtimeSinceStartup + currentSettings.getFloat("refreshRate");
        lastTime = thisTime;
        thisTime = Time.time;
        lastRealTime = ThisRealTime;
        ThisRealTime = Time.realtimeSinceStartup;
        gameTimeToRealtime = Mathf.Round((thisTime - lastTime) / (ThisRealTime - lastRealTime) * 100);
      }
    }

    protected override void onToolbar()
    {
      if (Input.GetMouseButtonUp(0))
      {
        if (currentSettings.getBool("showGauge"))
        {
          currentSettings.set("showGauge", false);
        }
        else
        {
          currentSettings.set("showGauge", true);
        }
      }
      else if (Input.GetMouseButtonUp(1))
      {
        toggleSettingsWindow();
      }
    }

    private void toggleSettingsWindow()
    {
      if (currentSettings.getBool("showSettings"))
      {
        currentSettings.set("showSettings", false);
      }
      else
      {
        currentSettings.set("showSettings", true);
      }
    }

    protected override void beforeSaveOnDestroy()
    {
      GameEvents.onGamePause.Remove(onPause);
      GameEvents.onGameUnpause.Remove(onUnpause);
      if (currentSettings != null)
      {
        currentSettings.set("showSettings", false);
        currentSettings.set("changePosition", false);
        currentSettings.set("gaugePosX", position.x);
        currentSettings.set("gaugePosY", position.y);
      }
    }
  }
}