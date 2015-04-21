using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.Instantly,true)]
  public partial class FPSLimiter : KerboKatzBase
  {
    private static bool toggleSettingsBool;
    private bool focusStatus;
    private bool focusStatusBool;
    private double fps;
    private double lastFPSCheck;
    private double lastFPS;
    private int targetFrameRate;
    public FPSLimiter()
    {
      modName = "SmallUtilities/FPSLimiter";
      requiresUtilities = new Version(1, 1, 0);
    }

    public override void Start()
    {
      DontDestroyOnLoad(this);
      SmallUtilities.FPSLimiter = this;
      currentSettings = new settings();
      currentSettings.load("SmallUtilities", "FPSLimiterSettings", modName);
      currentSettings.setDefault("showSettings", "false");
      currentSettings.setDefault("settingsSettingsRectX", "0");
      currentSettings.setDefault("settingsSettingsRectY", "0");
      currentSettings.setDefault("activeFPS", "35");
      currentSettings.setDefault("backgroundFPS", "10");
      currentSettings.setDefault("useVSync", "false");
    }
    public override void OnGuiAppLauncherReady()
    {
    }
    public override void OnDestroy()
    {
      base.OnDestroy();
      if (currentSettings != null)
      {
        currentSettings.set("showSettings", false);
        currentSettings.set("settingsSettingsRectX", settingsWindowRect.x);
        currentSettings.set("settingsSettingsRectY", settingsWindowRect.y);
        currentSettings.save();
      }
    }
    public void Update()
    {
      checkSettingsWindow();
      checkAppFocus();
      var currentTime = Utilities.getUnixTimestamp();
      if (lastFPSCheck + 1 > currentTime)
      {
        fps++;
      }
      else
      {
        lastFPS = fps / (currentTime - lastFPSCheck);
        fps = fps - lastFPS;
        lastFPSCheck = currentTime;
      }
    }

    private void checkAppFocus()
    {
      if ((!focusStatusBool && targetFrameRate == Application.targetFrameRate) || HighLogic.LoadedScene == GameScenes.LOADING || HighLogic.LoadedScene == GameScenes.LOADINGBUFFER)
        return;
      if (focusStatus)
      {
        Application.runInBackground = true;
        targetFrameRate = currentSettings.getInt("activeFPS");
      }
      else
      {
        var backgroundFPS = currentSettings.getInt("backgroundFPS");
        if (backgroundFPS > 0)
        {
          targetFrameRate = backgroundFPS;
        }
        else
        {
          Application.runInBackground = false;
        }
      }
      if (currentSettings.getBool("useVSync"))
      {
        switch (targetFrameRate)
        {
          case 30:
            QualitySettings.vSyncCount = 2;
            break;
          case 60:
            QualitySettings.vSyncCount = 1;
            break;
        }
      }
      else
      {
        QualitySettings.vSyncCount = 0;
      }
      Application.targetFrameRate = targetFrameRate;
      focusStatusBool = false;
    }

    private void checkSettingsWindow()
    {
      if (toggleSettingsBool)
      {
        if (currentSettings.getBool("showSettings"))
        {
          currentSettings.set("showSettings", false);
        }
        else
        {
          currentSettings.set("showSettings", true);
          if (settingsWindowRect.x == 0 && settingsWindowRect.y == 0)
          {
            settingsWindowRect.x = Input.mousePosition.x;
            settingsWindowRect.y = Screen.height - Input.mousePosition.y;
          }
        }
        toggleSettingsBool = false;
      }
    }
    void OnApplicationFocus(bool focusStatus)
    {
      this.focusStatus = focusStatus;
      focusStatusBool = true;
    }
    public static void toggleSettings()
    {
      toggleSettingsBool = true;
    }
  }
}
