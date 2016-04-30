using KerboKatz.Toolbar;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using KSP.UI;
using System.Text;
using KerboKatz.Assets;

namespace KerboKatz.FPSV
{
  [KSPAddon(KSPAddon.Startup.EveryScene, false)]
  public class FPSViewer : KerboKatzBase<Settings>, IToolbar
  {
    private List<GameScenes> _activeScences = new List<GameScenes>() { GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.MAINMENU, GameScenes.PSYSTEM, GameScenes.SPACECENTER, GameScenes.TRACKSTATION };
    private Text maxLabel;
    private Text currentLabel;
    private Text minLabel;
    private UIData labels;
    private Image labelsBackground;
    private UIData settingsWindow;

    public FPSViewer()
    {
      modName = "FPSViewer";
      displayName = "FPS Viewer";
      requiresUtilities = new Version(1, 3, 3);
      ToolbarBase.instance.Add(this);
      Log("Init done!");
    }
    public override void OnAwake()
    {
      LoadSettings("SmallUtilities/FPSViewer", "Settings");
      LoadUI("FPSViewer", "SmallUtilities/FPSViewer/FPSViewer");
      LoadUI("FPSViewerSettings", "SmallUtilities/FPSViewer/FPSViewer");
    }
    protected override void AfterDestroy()
    {
      ToolbarBase.instance.Remove(this);
      Log("AfterDestroy");
    }
    void Update()
    {
      if (!settings.showLabels || labels == null || maxLabel == null || currentLabel == null || minLabel == null)
        return;
      if (settings.showMaxFPS || maxLabel.color.a > 0)
        maxLabel.text = FPS.maxFPS.ToString();

      currentLabel.text = FPS.currentFPS.ToString();

      if (settings.showMinFPS || minLabel.color.a > 0)
        minLabel.text = FPS.minFPS.ToString();

      labelsBackground.transform.SetAsLastSibling();
    }
    #region ui
    protected override void OnUIElemntInit(UIData uiWindow)
    {
      switch (uiWindow.name)
      {
        case "FPSViewer":
          var prefabWindow = uiWindow.gameObject.transform;
          labels = uiWindow;
          maxLabel = InitTextField(prefabWindow, "Max", FPS.maxFPS.ToString());
          currentLabel = InitTextField(prefabWindow, "Current", FPS.currentFPS.ToString());
          minLabel = InitTextField(prefabWindow, "Min", FPS.minFPS.ToString());
          labelsBackground = prefabWindow.GetComponent<Image>();
          FadeGraphic(labelsBackground, settings.moveLabelPosition);
          UpdateMinMaxLabels();
          break;
        case "FPSViewerSettings":
          settingsWindow = uiWindow;
          var content = settingsWindow.gameObject.transform.FindChild("Content");
          InitToggle(content, "ShowMax", settings.showMaxFPS, OnShowMaxFPSChange);
          InitToggle(content, "ShowMin", settings.showMinFPS, OnShowMinFPSChange);
          InitToggle(content, "MovePosition", settings.moveLabelPosition, OnMovePositionChange);
          InitButton(content, "ResetMinMax", OnResetMinMax);
          break;
      }
    }

    private void OnResetMinMax()
    {
      FPS.instance.ResetMinMax();
    }

    private void OnMovePositionChange(bool arg0)
    {
      settings.moveLabelPosition = arg0;
      FadeGraphic(labelsBackground, settings.moveLabelPosition);
      settings.Save();
    }

    private void OnShowMinFPSChange(bool arg0)
    {
      settings.showMinFPS = arg0;
      UpdateMinMaxLabels();
      settings.Save();
    }

    private void OnShowMaxFPSChange(bool arg0)
    {
      settings.showMaxFPS = arg0;
      UpdateMinMaxLabels();
      settings.Save();
    }
    private void UpdateMinMaxLabels()
    {
      FadeGraphic(maxLabel, settings.showMaxFPS);
      FadeGraphic(minLabel, settings.showMinFPS);
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
      if (Input.GetMouseButtonUp(1))
      {
        settings.showSettings = !settings.showSettings;
        Log("Set to: ", settings.showSettings);
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
        return AssetLoader.GetAsset<Sprite>("FPSViewer", "Icons", "SmallUtilities/FPSViewer/FPSViewer");//Utilities.GetTexture("icon", "SmallUtilities/FPSViewer/Textures");
      }
    }
    #endregion
  }
}