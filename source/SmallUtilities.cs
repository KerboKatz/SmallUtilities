using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace KerboKatz
{
  [KSPAddon(KSPAddon.Startup.EveryScene, false)]
  public partial class SmallUtilities : KerboKatzBase
  {
    public static FPSLimiter FPSLimiter;
    public SmallUtilities()
    {
      modName = "SmallUtilities";
      requiresUtilities = new Version(1, 1, 0);
    }

    public override void Start()
    {
      base.Start();
      currentSettings.setDefault("showSettings", "false");
      currentSettings.setDefault("settingsSettingsRectX", "0");
      currentSettings.setDefault("settingsSettingsRectY", "0");
    }
    public override void OnGuiAppLauncherReady()
    {
      base.OnGuiAppLauncherReady();
      button.Setup(toggleSettings, toggleSettings, Utilities.getTexture("icon", "SmallUtilities/Textures"));
      button.VisibleInScenes = ApplicationLauncher.AppScenes.SPACECENTER;
    }
    void Update()
    {
    }
    public void toggleSettings()
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
          settingsWindowRect.y = 38;
        }
      }
    }
    public override void OnDestroy()
    {
      base.OnDestroy();
      if (currentSettings != null)
      {
        currentSettings.set("showSettings", false);
        currentSettings.save();
      }
    }
  }
}
