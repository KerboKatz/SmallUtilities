using KerboKatz.Classes;
using System.Collections.Generic;
using UnityEngine;
using KerboKatz;

namespace KerboKatz
{
  public partial class FPSLimiter : KerboKatzBase
  {
    private bool initStyle;
    private int settingsWindowID = 971305;
    private Rect settingsWindowRect = new Rect();
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
    private void InitStyle()
    {
      backgroundFPS = currentSettings.getFloat("backgroundFPS");
      activeFPS = currentSettings.getFloat("activeFPS");
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

      activeFPS = createSlider("Focused FPS","FPS limit while the game is active. ", activeFPS, 5, maxActiveFPS);
      backgroundFPS = createSlider("Background FPS","FPS limit while the game isn't focused. Set to 0 to pause any simulation anything else will cause the game to run slower.", backgroundFPS, 0, maxActiveFPS, activeFPS);


      createVsyncOption();
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
        currentSettings.set("activeFPS", activeFPS);
        currentSettings.set("backgroundFPS", backgroundFPS);
        focusStatusBool = true;
      }
      GUILayout.FlexibleSpace();
      if (Utilities.UI.createButton("Close", buttonStyle))
      {
        toggleSettings();
      }
      GUILayout.EndHorizontal();
    }

    private void showCurrentFPS()
    {
      GUILayout.BeginHorizontal();
      Utilities.UI.createLabel("CurrentFPS", textStyle);
      Utilities.UI.createLabel(Utilities.round(lastFPS, 1).ToString(), numberFieldStyle);

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

    private float createSlider(string label, string tooltip, float current, float minValue, float maxValue, float limitValue = 0)
    {
      Utilities.UI.createLabel(label, textStyle,tooltip);
      GUILayout.BeginHorizontal();
      GUILayout.BeginVertical();
      current = Utilities.round(GUILayout.HorizontalSlider(current, minValue, maxValue, horizontalSlider, horizontalSliderThumb), 0);
      GUILayout.EndVertical();
      current = Utilities.round(Utilities.toFloat(Utilities.getOnlyNumbers(GUILayout.TextField(current.ToString(), numberFieldStyle))), 0);
      if (limitValue == 0)
      {
        if (current > maxValue)
        {
          current = maxValue;
        }
      }
      else
      {
        if (current > limitValue)
        {
          current = limitValue;
        }
      }
      GUILayout.EndHorizontal();
      return current;
    }
  }
}