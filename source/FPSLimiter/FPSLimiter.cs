using KerboKatz.Assets;
using KerboKatz.Extensions;
using KerboKatz.Toolbar;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KerboKatz.FPSL
{
  [KSPAddon(KSPAddon.Startup.Instantly, true)]
  public class FPSLimiter : KerboKatzBase<Settings>, IToolbar
  {
    private bool focusStatus = true;
    private bool isDirty = true;
    private List<GameScenes> _activeScences = new List<GameScenes>() { GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.MAINMENU, GameScenes.PSYSTEM, GameScenes.SPACECENTER, GameScenes.TRACKSTATION };
    private Sprite _icon = AssetLoader.GetAsset<Sprite>("FPSLimiter", "Icons", "SmallUtilities/FPSLimiter/FPSLimiter");//Utilities.GetTexture("FPSLimiter", "SmallUtilities/FPSLimiter/Textures");
    private int targetFrameRate;
    private Text currentFPSLabel;
    private string settingsUIName;

    //private int targetFrameRate;

    #region init/destroy

    public FPSLimiter()
    {
      modName = "FPSLimiter";
      displayName = "FPS Limiter";
      settingsUIName = "FPSLimiter";
      requiresUtilities = new Version(1, 4, 0);
      ToolbarBase.instance.Add(this);
      LoadSettings("SmallUtilities/FPSLimiter", "Settings");
      Log("Init done!");
    }

    public override void OnAwake()
    {
      LoadUI(settingsUIName, "SmallUtilities/FPSLimiter/FPSLimiter");
      GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);
      this.enabled = true;
      DontDestroyOnLoad(this);
      Log("Awake");
    }

    public void onGameSceneLoadRequested(GameScenes GameScene)
    {
      if (GameScene == GameScenes.MAINMENU)
      {
        _icon = AssetLoader.GetAsset<Sprite>("FPSLimiter", "Icons", "SmallUtilities/FPSLimiter/FPSLimiter");//Utilities.GetTexture("icon", "SmallUtilities/FPSLimiter/Textures");
      }
      //reload the ui on scene change
      LoadUI(settingsUIName, "SmallUtilities/FPSLimiter/FPSLimiter");
    }

    protected override void AfterDestroy()
    {
      GameEvents.onGameSceneLoadRequested.Remove(onGameSceneLoadRequested);
      ToolbarBase.instance.Remove(this);
      Log("AfterDestroy");
    }

    #endregion init/destroy

    #region ui

    protected override void OnUIElemntInit(UIData uiWindow)
    {
      var prefabWindow = uiWindow.gameObject.transform as RectTransform;
      var content = prefabWindow.FindChild("Content");
      currentFPSLabel = InitTextField(content.FindChild("CurrentFPS"), "Label", "");
      InitInputField(content, "ActiveFPS", settings.active.ToString(), OnActiveFPSChange);
      InitInputField(content, "BackgroundFPS", settings.background.ToString(), OnBackgroundFPSChange);
      InitToggle(content, "VSync", settings.useVSync, OnVSyncChange);
      InitToggle(content, "DisableMod", settings.disable, OnDisableMod);
      InitToggle(content, "Debug", settings.debug, OnDebugChange);
    }

    private void OnDebugChange(bool arg0)
    {
      settings.debug = arg0;
      SaveSettings();
      Log("OnDebugChange");
    }

    private void OnDisableMod(bool arg0)
    {
      settings.disable = arg0;
      SaveSettings();
      isDirty = true;
      Log("OnDisableMod");
    }

    private void OnVSyncChange(bool arg0)
    {
      settings.useVSync = arg0;
      SaveSettings();
      isDirty = true;
      Log("OnVSyncChange");
    }

    private void OnActiveFPSChange(string arg0)
    {
      settings.active = arg0.ToInt();
      SaveSettings();
      isDirty = true;
      Log("OnActiveFPSChange");
    }

    private void OnBackgroundFPSChange(string arg0)
    {
      settings.background = arg0.ToInt();
      SaveSettings();
      isDirty = true;
      Log("OnBackgroundFPSChange");
    }

    #endregion ui

    public void Update()
    {
      if (currentFPSLabel != null)
        currentFPSLabel.text = FPS.currentFPS.ToString();
      if ((!isDirty && targetFrameRate == Application.targetFrameRate) || HighLogic.LoadedScene == GameScenes.LOADING || HighLogic.LoadedScene == GameScenes.LOADINGBUFFER)
      {
        if (HighLogic.LoadedScene == GameScenes.LOADING || HighLogic.LoadedScene == GameScenes.LOADINGBUFFER)
        {
          Application.targetFrameRate = -1;
          QualitySettings.vSyncCount = 0;
        }
        return;
      }
      Application.targetFrameRate = 60;
      QualitySettings.vSyncCount = 1;
      if (settings.disable)//currentSettings.getBool("disableMod"))
      {
        targetFrameRate = GameSettings.FRAMERATE_LIMIT;
        QualitySettings.vSyncCount = GameSettings.SYNC_VBL;
        Application.runInBackground = true;
      }
      else
      {
        if (focusStatus)
        {
          Application.runInBackground = true;
          targetFrameRate = settings.active;
        }
        else
        {
          var backgroundFPS = settings.background;
          if (backgroundFPS > 0)
          {
            targetFrameRate = backgroundFPS;
          }
          else
          {
            Application.runInBackground = false;
          }
        }
        if (settings.useVSync)
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
      }
      Application.targetFrameRate = targetFrameRate;
      isDirty = false;
    }

    private void OnApplicationFocus(bool focusStatus)
    {
      this.focusStatus = focusStatus;
      isDirty = true;
    }

    #region IToolbar

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
      var uiData = GetUIData(settingsUIName);
      if (uiData == null || uiData.canvasGroup == null)
        return;
      settings.showSettings = !settings.showSettings;
      if (settings.showSettings)
      {
        FadeCanvasGroup(uiData.canvasGroup, 1, settings.uiFadeSpeed);
      }
      else
      {
        FadeCanvasGroup(uiData.canvasGroup, 0, settings.uiFadeSpeed);
      }
      SaveSettings();
    }

    public Sprite icon
    {
      get
      {
        return _icon;
      }
      private set
      {
        if (_icon != value)
        {
          _icon = value;
          ToolbarBase.UpdateIcon();
        }
      }
    }

    #endregion IToolbar
  }
}