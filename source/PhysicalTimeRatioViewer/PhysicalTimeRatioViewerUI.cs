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
      GUI.Label(position.rect, gameTimeToRealtime + "%", gaugeStyle);
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
      GUILayout.BeginVertical();
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
      GUILayout.Label(new GUIContent(Utilities.getTexture("moveHere", "SmallUtilities/Textures"), "Drag this window to where you want the gauge to be"), moveHere);
      Utilities.UI.updateTooltipAndDrag();
    }
  }
}