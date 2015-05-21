using KerboKatz.Extensions;
using UnityEngine;

namespace KerboKatz
{
  public partial class FPSViewer : KerboKatzBase
  {
    private static int changePositionWindowID = Utilities.UI.getNewWindowID;
    private static int settingsWindowID = Utilities.UI.getNewWindowID;
    private bool initStyle;
    private GUIStyle changePositionStyle;
    private GUIStyle textStyle;
    private GUIStyle buttonStyle;
    private GUIStyle numberFieldStyle;
    private GUIStyle horizontalSlider;
    private GUIStyle horizontalSliderThumb;
    private GUIStyle toggleStyle;
    private GUIStyle fpsStyle;
    private Rectangle position = new Rectangle();
    private Rectangle settingsWindowRect = new Rectangle(Rectangle.updateType.Cursor);
    private GUIStyle moveHere;
    private GUIStyle settingsWindowStyle;
    private bool showMinFPS;
    private bool showMaxFPS;
    private void InitStyle()
    {
      settingsWindowStyle = new GUIStyle(HighLogic.Skin.window);
      settingsWindowStyle.fixedWidth = 250;

      changePositionStyle = new GUIStyle(HighLogic.Skin.window);
      changePositionStyle.fixedHeight = 50;
      changePositionStyle.fixedWidth = 50;
      changePositionStyle.border.top = 0;
      changePositionStyle.padding.setToZero();
      changePositionStyle.contentOffset = new Vector2(0, 0);

      textStyle = new GUIStyle(HighLogic.Skin.label);
      textStyle.fixedWidth = 227;
      textStyle.margin.left = 10;

      buttonStyle = new GUIStyle(HighLogic.Skin.button);
      buttonStyle.fixedWidth = 115;

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

      fpsStyle = new GUIStyle(HighLogic.Skin.label);
      fpsStyle.margin.setToZero();
      fpsStyle.padding.setToZero();
      fpsStyle.fixedHeight = 50;
      fpsStyle.fixedWidth = 50;
      fpsStyle.alignment = TextAnchor.MiddleCenter;

      moveHere = new GUIStyle(fpsStyle);
      moveHere.alignment = TextAnchor.UpperLeft;

      initStyle = true;
    }

    public void OnGUI()
    {
      if (!initStyle)
        InitStyle();
      if (currentSettings.getBool("showFPS"))
      {
        showFPSOnDisplay();
      }
      Utilities.UI.createWindow(currentSettings.getBool("showSettings"), settingsWindowID, ref settingsWindowRect, settingsWindow, "FPSViewer", settingsWindowStyle);
      Utilities.UI.createWindow(currentSettings.getBool("changePosition"), changePositionWindowID, ref position, changePosition, "", changePositionStyle);
      Utilities.UI.showTooltip();
    }

    private void showFPSOnDisplay()
    {
      var fps = Utilities.round(FPS.instance.currentFPS).ToString();
      if (currentSettings.getBool("showMinFPS"))
      {
        fps = fps + "\n" + Utilities.round(FPS.instance.minFPS).ToString();
      }
      if (currentSettings.getBool("showMaxFPS"))
      {
        fps = fps + "\n" + Utilities.round(FPS.instance.maxFPS).ToString();
      }
      GUI.Label(position.rect, fps, fpsStyle);
      if (currentSettings.getBool("changePosition"))
      {
        GUI.depth = int.MaxValue;
      }
      else
      {
        GUI.depth = currentSettings.getInt("depth");
      }
    }

    private void settingsWindow(int id)
    {
      if (Utilities.UI.createToggle("Always on top", (currentSettings.getInt("depth") == 0), toggleStyle))
      {
        currentSettings.set("depth", 0);
      }
      else
      {
        currentSettings.set("depth", int.MaxValue - 1);
      }
      if (Utilities.UI.createToggle("Show min FPS", showMinFPS, toggleStyle))
      {
        showMinFPS = true;
      }
      else
      {
        showMinFPS = false;
      }
      if (Utilities.UI.createToggle("Show max FPS", showMaxFPS, toggleStyle))
      {
        showMaxFPS = true;
      }
      else
      {
        showMaxFPS = false;
      }
      GUILayout.BeginVertical();
      Utilities.UI.createOptionSwitcher("Use:", Toolbar.toolbarOptions, ref toolbarSelected);

      GUILayout.BeginHorizontal();
      if (Utilities.UI.createButton("Reset min/max", buttonStyle))
      {
        FPS.instance.resetMinMax();
      }
      GUILayout.FlexibleSpace();
      if (Utilities.UI.createButton("Change position", buttonStyle, "Press on this to change the position where the fps will show."))
      {
        if (currentSettings.getBool("changePosition"))
        {
          currentSettings.set("changePosition", false);
          currentSettings.set("fpsPosX", position.x);
          currentSettings.set("fpsPosY", position.y);
        }
        else
          currentSettings.set("changePosition", true);
      }
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      if (Utilities.UI.createButton("Save", buttonStyle))
      {
        currentSettings.set("showMinFPS", showMinFPS);
        currentSettings.set("showMaxFPS", showMaxFPS);
        updateToolbarBool();
      }
      GUILayout.FlexibleSpace();
      if (Utilities.UI.createButton("Close", buttonStyle))
      {
        toggleSettingsWindow();
      }
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
      Utilities.UI.updateTooltipAndDrag();
    }

    private void changePosition(int id)
    {
      GUILayout.Label(new GUIContent(Utilities.getTexture("moveHere", "SmallUtilities/Textures"), "Drag this window to where you want the fps counter to be"), moveHere);
      Utilities.UI.updateTooltipAndDrag();
    }
  }
}