using KerboKatz.Extensions;
using UnityEngine;

namespace KerboKatz
{
  public partial class FPSLimiter : KerboKatzBase
  {
    private bool initStyle;
    private static int settingsWindowID = Utilities.UI.getNewWindowID;
    private Rectangle settingsWindowRect = new Rectangle(Rectangle.updateType.Cursor);
    private float backgroundFPS;
    private GUIStyle settingsWindowStyle;
    private GUIStyle textStyle;
    private GUIStyle buttonStyle;
    private float activeFPS;
    private float maxActiveFPS;
    private GUIStyle numberFieldStyle;
    private GUIStyle horizontalSlider;
    private GUIStyle horizontalSliderThumb;
    private GUIStyle toggleStyle;
    private bool useVSync;
    private GUIStyle sortTextStyle;
    private bool disableMod;
    private bool dontLimit;
    private void InitStyle()
    {
      settingsWindowStyle = new GUIStyle(HighLogic.Skin.window);
      settingsWindowStyle.fixedWidth = 300;

      textStyle = new GUIStyle(HighLogic.Skin.label);
      textStyle.fixedWidth = 227;
      textStyle.margin.left = 10;

      buttonStyle = new GUIStyle(HighLogic.Skin.button);
      buttonStyle.fixedWidth = 100;

      numberFieldStyle = new GUIStyle(HighLogic.Skin.box);
      numberFieldStyle.fixedWidth = 60;
      numberFieldStyle.fixedHeight = 22;
      numberFieldStyle.alignment = TextAnchor.MiddleCenter;
      numberFieldStyle.padding.right = 7;
      numberFieldStyle.margin.top = 5;

      horizontalSlider = new GUIStyle(HighLogic.Skin.horizontalSlider);
      horizontalSlider.fixedWidth = 232;
      horizontalSlider.margin.top += 7;

      horizontalSliderThumb = new GUIStyle(HighLogic.Skin.horizontalSliderThumb);

      toggleStyle = new GUIStyle(HighLogic.Skin.toggle);

      Utilities.UI.getTooltipStyle();
      sortTextStyle = new GUIStyle(Utilities.UI.sortTextStyle);
      sortTextStyle.padding.left += 6;
      sortTextStyle.fixedWidth += 50;
      initStyle = true;
    }

    public void OnGUI()
    {
      if (!initStyle)
        InitStyle();
      Utilities.UI.createWindow(currentSettings.getBool("showSettings"), settingsWindowID, ref settingsWindowRect, settingsWindow, "FPSLimiter", settingsWindowStyle);
      Utilities.UI.showTooltip();
    }

    private void settingsWindow(int id)
    {
      maxActiveFPS = 120;
      GUILayout.BeginVertical();

      if (Utilities.UI.createToggle("Disable", disableMod, toggleStyle))
      {
        disableMod = true;
      }
      else
      {
        disableMod = false;
      }
      if (Utilities.UI.createToggle("Don't limit FPS", dontLimit, toggleStyle))
      {
        dontLimit = true;
      }
      else
      {
        dontLimit = false;
      }
      activeFPS = Utilities.UI.createSlider("Focused FPS", activeFPS, 5, maxActiveFPS, 1, textStyle, numberFieldStyle, horizontalSlider, horizontalSliderThumb, "FPS limit while the game is active.");
      backgroundFPS = Utilities.UI.createSlider("Background FPS", backgroundFPS, 0, maxActiveFPS, 1, textStyle, numberFieldStyle, horizontalSlider, horizontalSliderThumb, "FPS limit while the game isn't focused. Set to 0 to pause any simulation anything else will cause the game to run slower.", activeFPS);

      createVsyncOption();
      Utilities.UI.createOptionSwitcher("Use:", Toolbar.toolbarOptions, ref toolbarSelected, sortTextStyle);
      showCurrentFPS();

      createButtons();
      GUILayout.EndVertical();
      Utilities.UI.updateTooltipAndDrag();
    }

    private void createButtons()
    {
      GUILayout.BeginHorizontal();
      if (Utilities.UI.createButton("Save", buttonStyle))
      {
        currentSettings.set("useVSync", useVSync);
        currentSettings.set("disableMod", disableMod);
        currentSettings.set("dontLimit", dontLimit);
        currentSettings.set("activeFPS", activeFPS);
        currentSettings.set("backgroundFPS", backgroundFPS);
        updateToolbarBool();
        focusStatusBool = true;
      }
      GUILayout.FlexibleSpace();
      if (Utilities.UI.createButton("Close", buttonStyle))
      {
        onToolbar();
      }
      GUILayout.EndHorizontal();
    }

    private void showCurrentFPS()
    {
      GUILayout.BeginHorizontal();
      Utilities.UI.createLabel("CurrentFPS", textStyle);
      Utilities.UI.createLabel(Utilities.round(FPS.instance.currentFPS, 1).ToString(), numberFieldStyle);

      GUILayout.EndHorizontal();
    }

    private void createVsyncOption()
    {
      GUILayout.BeginHorizontal();
      Utilities.UI.createLabel("Use VSync", textStyle, "If you turn on this option vertical synchronization will be used to reduce screen tearing. This option will only take effect at 30 and 60 fps.");
      GUILayout.FlexibleSpace();
      if (Utilities.UI.createToggle("", useVSync, toggleStyle))
      {
        useVSync = true;
      }
      else
      {
        useVSync = false;
      }
      GUILayout.EndHorizontal();
    }
  }
}