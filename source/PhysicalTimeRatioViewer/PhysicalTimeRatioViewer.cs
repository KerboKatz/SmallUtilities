using KerboKatz;
using KerboKatz.Assets;
using KerboKatz.Toolbar;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KerboKatz.PTRV
{
  [KSPAddon(KSPAddon.Startup.Flight, false)]
  public class PhysicalTimeRatioViewer : KerboKatzBase<Settings>, IToolbar
  {
    private List<GameScenes> _activeScences = new List<GameScenes>() { GameScenes.FLIGHT };
    private UIData labels;
    private Image labelsBackground;
    private UIData settingsWindow;
    private float nextGametimeToRealtimeCheck;
    private float thisTime;
    private float ThisRealTime;
    private float lastRealTime;
    private float gameTimeToRealtime;
    private Text realToGameTimeRatioLabel;
    private Text maxDeltaTimeLabel;

    public PhysicalTimeRatioViewer()
    {
      modName = "PhysicalTimeRatioViewer";
      displayName = "Physical Time Ratio Viewer";
      requiresUtilities = new Version(1, 3, 0);
      ToolbarBase.instance.Add(this);
      Log("Init done!");
    }
    public override void OnAwake()
    {
      LoadSettings("SmallUtilities/PhysicalTimeRatioViewer", "Settings");
      LoadUI("PhysicalTimeRatioViewer");
      LoadUI("PhysicalTimeRatioViewerSettings");
      GameEvents.onGamePause.Add(onPause);
      GameEvents.onGameUnpause.Add(onUnpause);
    }
    protected override void AfterDestroy()
    {
      GameEvents.onGamePause.Remove(onPause);
      GameEvents.onGameUnpause.Remove(onUnpause);
      ToolbarBase.instance.Remove(this);
      Log("AfterDestroy");
    }
    private void onUnpause()
    {
      //nextGametimeToRealtimeCheck = Time.realtimeSinceStartup;
      thisTime = Time.time;
      ThisRealTime = Time.realtimeSinceStartup;
    }

    private void onPause()
    {
      gameTimeToRealtime = 0;
    }

    void Update()
    {
      if (nextGametimeToRealtimeCheck + settings.refreshRate < Time.realtimeSinceStartup)
      {
        nextGametimeToRealtimeCheck = Time.realtimeSinceStartup ;
        var lastTime = thisTime;
        thisTime = Time.time;
        lastRealTime = ThisRealTime;
        ThisRealTime = Time.realtimeSinceStartup;
        gameTimeToRealtime = Mathf.Round((thisTime - lastTime) / (ThisRealTime - lastRealTime) * 100);
        if (realToGameTimeRatioLabel != null)
          realToGameTimeRatioLabel.text = gameTimeToRealtime + " %";
      }
      if (maxDeltaTimeLabel != null && settings.showMaxDeltaTime)
        maxDeltaTimeLabel.text = Time.maximumDeltaTime.ToString();
      labelsBackground.transform.SetAsLastSibling();
    }
    #region ui
    protected override void OnUIElemntInit(UIData uiWindow)
    {
      switch (uiWindow.name)
      {
        case "PhysicalTimeRatioViewer":
          labels = uiWindow;
          realToGameTimeRatioLabel = InitTextField(labels.gameObject.transform, "RealToGameTimeRatio", string.Empty);
          maxDeltaTimeLabel = InitTextField(labels.gameObject.transform, "MaxDeltaTime", string.Empty);
          labelsBackground = labels.gameObject.GetComponent<Image>();

          FadeGraphic(maxDeltaTimeLabel, settings.showMaxDeltaTime);
          FadeGraphic(labelsBackground, settings.moveLabelPosition);

          break;
        case "PhysicalTimeRatioViewerSettings":
          settingsWindow = uiWindow;
          var content = settingsWindow.gameObject.transform.FindChild("Content");
          InitToggle(content, "ShowMaximumDeltaTime", settings.showMaxDeltaTime, onShowMaxDeltaTimeChange);
          InitToggle(content, "MovePosition", settings.moveLabelPosition, OnMovePositionChange);
          InitSlider(content, "RefreshRate", settings.refreshRate, OnRefreshRateChange);
          InitSlider(content, "MaxDeltaTime", Time.maximumDeltaTime, OnMaxDeltaTimeChange);
          break;
      }
    }

    private void OnMaxDeltaTimeChange(float arg0)
    {
      Time.maximumDeltaTime = arg0;
    }

    private void OnRefreshRateChange(float arg0)
    {
      settings.refreshRate = arg0;
      settings.Save();
    }

    private void onShowMaxDeltaTimeChange(bool arg0)
    {
      settings.showMaxDeltaTime = arg0;
      FadeGraphic(maxDeltaTimeLabel, settings.showMaxDeltaTime);
      settings.Save();
    }
    private void OnMovePositionChange(bool arg0)
    {
      settings.moveLabelPosition = arg0;
      FadeGraphic(labelsBackground, settings.moveLabelPosition);
      settings.Save();
    }
    #endregion
    #region toolbar
    public List<GameScenes> activeScences
    {
      get
      {
        return _activeScences;
      }
    }

    public UnityAction onClick
    {
      get
      {
        return OnToolbar;
      }
    }

    private void OnToolbar()
    {
      var leftAlt = Input.GetKey(KeyCode.LeftAlt);
      if (leftAlt)
      {
        settings.showSettings = !settings.showSettings;
        if (settings.showSettings)
        {
          FadeCanvasGroup(settingsWindow.canvasGroup, 1, settings.uiFadeSpeed);
        }
        else
        {
          FadeCanvasGroup(settingsWindow.canvasGroup, 0, settings.uiFadeSpeed);
        }
      }
      else
      {
        settings.showLabels = !settings.showLabels;
        if (labels == null)
          return;
        if (settings.showLabels)
        {
          FadeCanvasGroup(labels.canvasGroup, 1, settings.uiFadeSpeed);
        }
        else
        {
          FadeCanvasGroup(labels.canvasGroup, 0, settings.uiFadeSpeed);
        }
      }
    }

    public Sprite icon
    {
      get
      {
        return AssetLoader.GetAsset<Sprite>("PhysicalTimeRatioViewer", "Icons");//Utilities.GetTexture("icon", "SmallUtilities/PhysicalTimeRatioViewer/Textures");
      }
    }
    #endregion
  }
}