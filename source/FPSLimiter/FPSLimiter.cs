using System;
using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.Instantly, true)]
  public partial class FPSLimiter : KerboKatzBase
  {
    private bool focusStatus = true;
    private bool focusStatusBool = true;
    private int targetFrameRate;
    public FPSLimiter()
    {
      modName = "FPSLimiter";
      displayName = "FPS Limiter";
      requiresUtilities = new Version(1, 2, 0);
    }

    protected override void Started()
    {
      DontDestroyOnLoad(this);
      currentSettings.load("SmallUtilities", "FPSLimiterSettings", modName);
      currentSettings.setDefault("showSettings", "false");
      currentSettings.setDefault("settingsSettingsRectX", "0");
      currentSettings.setDefault("settingsSettingsRectY", "0");
      currentSettings.setDefault("activeFPS", "35");
      currentSettings.setDefault("backgroundFPS", "10");
      currentSettings.setDefault("useVSync", "false");
      currentSettings.setDefault("useToolbar", "true");

      settingsWindowRect.x = currentSettings.getFloat("settingsSettingsRectX");
      settingsWindowRect.y = currentSettings.getFloat("settingsSettingsRectY");

      setAppLauncherScenes(ApplicationLauncher.AppScenes.ALWAYS);

      GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);
    }

    public void onGameSceneLoadRequested(GameScenes GameScene)
    {
      if (GameScene == GameScenes.MAINMENU)
      {
        setIcon(Utilities.getTexture("FPSLimiter", "SmallUtilities/Textures"));
        GameEvents.onGameSceneLoadRequested.Remove(onGameSceneLoadRequested);
      }
    }

    protected override void beforeSaveOnDestroy()
    {
      GameEvents.onGameSceneLoadRequested.Remove(onGameSceneLoadRequested);
      if (currentSettings != null)
      {
        currentSettings.set("showSettings", false);
        currentSettings.set("settingsSettingsRectX", settingsWindowRect.x);
        currentSettings.set("settingsSettingsRectY", settingsWindowRect.y);
      }
    }

    public void Update()
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

          default:
            QualitySettings.vSyncCount = 0;
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

    private void OnApplicationFocus(bool focusStatus)
    {
      this.focusStatus = focusStatus;
      focusStatusBool = true;
    }

    protected override void onToolbar()
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
  }
}