using KerboKatz.Extensions;
using UnityEngine;

namespace KerboKatz
{
  public partial class PhysicalTimeRatioViewer : KerboKatzBase
  {
    private bool hideOnUIHidden;
    private bool initStyle;
    private GUIStyle buttonStyle;
    private GUIStyle changePositionStyle;
    private GUIStyle gaugeStyle;
    private GUIStyle moveHere;
    private GUIStyle settingsWindowStyle;
    private GUIStyle toggleStyle;
    private Rectangle position = new Rectangle();
    private Rectangle settingsWindowRect = new Rectangle(Rectangle.updateType.Cursor);
    private static int changePositionWindowID = Utilities.UI.getNewWindowID;
    private static int settingsWindowID = Utilities.UI.getNewWindowID;
    private GUIStyle sortTextStyle;
    private GUIStyle sortOptionTextStyle;

    int anchorOptionSelected = 4;
    private float refreshRate;
    private GUIStyle textStyle;
    private GUIStyle numberFieldStyle;
    private GUIStyle horizontalSlider;
    private bool ShowMaximumDeltaTime;
    private bool changeMaximumDeltaTime;
    private bool changeMaximumDeltaTimeLiveEdit;
    private float maximumDeltaTime;

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

      buttonStyle = new GUIStyle(HighLogic.Skin.button);
      buttonStyle.fixedWidth = 115;

      toggleStyle = new GUIStyle(HighLogic.Skin.toggle);

      gaugeStyle = new GUIStyle(HighLogic.Skin.label);
      gaugeStyle.margin.setToZero();
      gaugeStyle.padding.setToZero();
      gaugeStyle.fixedHeight = 50;
      gaugeStyle.fixedWidth = 50;
      gaugeStyle.alignment = TextAnchor.MiddleCenter;

      moveHere = new GUIStyle(gaugeStyle);
      moveHere.alignment = TextAnchor.UpperLeft;

      sortTextStyle = new GUIStyle(HighLogic.Skin.label);
      sortTextStyle.margin.top = 2;
      sortTextStyle.padding.setToZero();
      sortTextStyle.fixedWidth = 100;

      sortOptionTextStyle = new GUIStyle(sortTextStyle);
      sortOptionTextStyle.margin.left = 0;
      sortOptionTextStyle.padding.left = 0;
      sortOptionTextStyle.fixedWidth = 80;
      sortOptionTextStyle.alignment = TextAnchor.MiddleCenter;
      Utilities.UI.setAnchorPosition(gaugeStyle, anchorOptionSelected);

      textStyle = new GUIStyle(HighLogic.Skin.label);
      textStyle.fixedWidth = 227;
      textStyle.margin.left = 10;

      numberFieldStyle = new GUIStyle(HighLogic.Skin.box);
      numberFieldStyle.fixedWidth = 75;
      numberFieldStyle.fixedHeight = 22;
      numberFieldStyle.alignment = TextAnchor.MiddleCenter;
      numberFieldStyle.padding.right = 7;
      numberFieldStyle.margin.top = 5;

      horizontalSlider = new GUIStyle(HighLogic.Skin.horizontalSlider);
      horizontalSlider.fixedWidth = 175;
      horizontalSlider.margin.top += 7;


      initStyle = true;
    }

    public void OnGUI()
    {
      if (!initStyle)
        InitStyle();
      if (currentSettings.getBool("showGauge"))
      {
        showGaugeOnDisplay();
      }
      Utilities.UI.createWindow(currentSettings.getBool("showSettings"), settingsWindowID, ref settingsWindowRect, settingsWindow, "Physical time ratio viewer", settingsWindowStyle);
      Utilities.UI.createWindow(currentSettings.getBool("changePosition"), changePositionWindowID, ref position, changePosition, "", changePositionStyle);
      Utilities.UI.showTooltip();
    }

    private void showGaugeOnDisplay()
    {
      if (UIEvents._instance.hiddenUI && currentSettings.getBool("hideOnUIHidden"))
      {
        return;
      }
      string label = gameTimeToRealtime + "%";
      if (currentSettings.getBool("ShowMaximumDeltaTime"))
      {
        label = label + "\n" + Time.maximumDeltaTime;
      }
      GUI.Label(position.rect, label, gaugeStyle);
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
      if (Utilities.UI.createToggle("Hide on hidden UI", hideOnUIHidden, toggleStyle))
      {
        hideOnUIHidden = true;
      }
      else
      {
        hideOnUIHidden = false;
      }
      if (Utilities.UI.createToggle("Show maximumDeltaTime", ShowMaximumDeltaTime, toggleStyle))
      {
        ShowMaximumDeltaTime = true;
      }
      else
      {
        ShowMaximumDeltaTime = false;
      }
      if (Utilities.UI.createToggle("Change maximumDeltaTime", changeMaximumDeltaTime, toggleStyle))
      {
        changeMaximumDeltaTime = true;
        if (Utilities.UI.createToggle("Live edit", changeMaximumDeltaTimeLiveEdit, toggleStyle))
        {
          changeMaximumDeltaTimeLiveEdit = true;
          Time.maximumDeltaTime = Utilities.UI.createSlider("maximumDeltaTime", Time.maximumDeltaTime, 0.02f, 0.2f, 0.01f, textStyle, numberFieldStyle, horizontalSlider, HighLogic.Skin.horizontalSliderThumb, "Lower values will allow for higher frame rates on the cost of less accurate physics and slower gameplay while allowing for a higher frame rate to be archived. Higher values will do the opposite. Potentially lowering the fps but keeping the time at around the same speed as the real clock");
        }
        else
        {
          changeMaximumDeltaTimeLiveEdit = false;
          maximumDeltaTime = Utilities.UI.createSlider("maximumDeltaTime", maximumDeltaTime, 0.02f, 0.2f, 0.01f, textStyle, numberFieldStyle, horizontalSlider, HighLogic.Skin.horizontalSliderThumb, "Lower values will allow for higher frame rates on the cost of less accurate physics and slower gameplay while allowing for a higher frame rate to be archived. Higher values will do the opposite. Potentially lowering the fps but keeping the time at around the same speed as the real clock");
        }
      }
      else
      {
        changeMaximumDeltaTime = false;
      }
      GUILayout.BeginVertical();

      refreshRate = Utilities.UI.createSlider("Refresh rate", refreshRate, 0, 5, 0.125f, textStyle, numberFieldStyle, horizontalSlider, HighLogic.Skin.horizontalSliderThumb);
      Utilities.UI.createOptionSwitcher("Anchor Gauge:", Utilities.UI.anchorOptions, ref anchorOptionSelected, sortTextStyle, sortOptionTextStyle);
      Utilities.UI.createOptionSwitcher("Use:", Toolbar.toolbarOptions, ref toolbarSelected);

      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if (Utilities.UI.createButton("Change position", buttonStyle, "Press on this to change the position where the gauge will show."))
      {
        if (currentSettings.getBool("changePosition"))
        {
          currentSettings.set("changePosition", false);
        }
        else
          currentSettings.set("changePosition", true);
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      if (Utilities.UI.createButton("Save", buttonStyle))
      {
        currentSettings.set("gaugePosX", position.x);
        currentSettings.set("gaugePosY", position.y);
        currentSettings.set("hideOnUIHidden", hideOnUIHidden);
        currentSettings.set("ShowMaximumDeltaTime", ShowMaximumDeltaTime);
        currentSettings.set("refreshRate", refreshRate);
        currentSettings.set("anchorOptionSelected", anchorOptionSelected);

        currentSettings.set("changeMaximumDeltaTime", changeMaximumDeltaTime);
        currentSettings.set("changeMaximumDeltaTimeLiveEdit", changeMaximumDeltaTimeLiveEdit);
        currentSettings.set("changeMaximumDeltaTime", changeMaximumDeltaTime);
        changeMaxDeltaTime();
        Utilities.UI.setAnchorPosition(gaugeStyle, anchorOptionSelected);
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

    private void changeMaxDeltaTime()
    {
      if (changeMaximumDeltaTime)
      {
        if (!changeMaximumDeltaTimeLiveEdit)
        {
          currentSettings.set("maximumDeltaTime", maximumDeltaTime);
          Time.maximumDeltaTime = maximumDeltaTime;
        }
        else
        {
          currentSettings.set("maximumDeltaTime", Time.maximumDeltaTime);
        }
      }
    }

    private void changePosition(int id)
    {
      GUILayout.Label(new GUIContent(Utilities.getTexture("moveHere", "SmallUtilities/Textures"), "Drag this window to where you want the gauge to be"), moveHere);
      Utilities.UI.updateTooltipAndDrag();
    }
  }
}