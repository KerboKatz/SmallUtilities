using KerboKatz.Extensions;
using System.Threading;
using UnityEngine;

namespace KerboKatz
{
  public partial class EditorCamUtilities : KerboKatzBase
  {
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
    private Rectangle settingsWindowRect = new Rectangle(Rectangle.updateType.Cursor);
    private GUIStyle moveHere;
    private GUIStyle settingsWindowStyle;
    private bool extendHangar;
    private Vector3 extendSPH = new Vector3();
    private Vector3 extendVAB = new Vector3();
    private bool shrink;
    private void InitStyle()
    {
      settingsWindowStyle = new GUIStyle(HighLogic.Skin.window);
      settingsWindowStyle.fixedWidth = 250;

      changePositionStyle = new GUIStyle(HighLogic.Skin.window);
      changePositionStyle.fixedHeight = 50;
      changePositionStyle.fixedWidth = 50;
      changePositionStyle.border.top = 0;
      changePositionStyle.padding.top = 0;
      changePositionStyle.padding.bottom = 0;
      changePositionStyle.padding.left = 0;
      changePositionStyle.padding.right = 0;
      changePositionStyle.contentOffset = new Vector2(0, 0);

      textStyle = new GUIStyle(HighLogic.Skin.label);
      textStyle.fixedWidth = 227;
      textStyle.padding.setToZero();
      textStyle.margin.setToZero();
      textStyle.margin.left = 10;

      buttonStyle = new GUIStyle(HighLogic.Skin.button);
      buttonStyle.fixedWidth = 115;

      numberFieldStyle = new GUIStyle(HighLogic.Skin.box);
      numberFieldStyle.fixedWidth = 50;
      numberFieldStyle.fixedHeight = 22;
      numberFieldStyle.alignment = TextAnchor.MiddleCenter;
      numberFieldStyle.padding.setToZero();
      numberFieldStyle.margin.setToZero();
      numberFieldStyle.padding.right = 7;
      numberFieldStyle.margin.top = 2;
      numberFieldStyle.margin.left = 2;

      horizontalSlider = new GUIStyle(HighLogic.Skin.horizontalSlider);
      horizontalSlider.fixedWidth = 200;
      horizontalSlider.padding.setToZero();
      horizontalSlider.margin.setToZero();
      horizontalSlider.margin.top = 7;

      horizontalSliderThumb = new GUIStyle(HighLogic.Skin.horizontalSliderThumb);

      toggleStyle = new GUIStyle(HighLogic.Skin.toggle);

      fpsStyle = new GUIStyle(HighLogic.Skin.label);
      fpsStyle.padding.setToZero();
      fpsStyle.margin.setToZero();
      fpsStyle.alignment = TextAnchor.MiddleCenter;

      moveHere = new GUIStyle(fpsStyle);
      moveHere.alignment = TextAnchor.UpperLeft;

      initStyle = true;
    }

    public void OnGUI()
    {
      if (!initStyle)
        InitStyle();
      if (Event.current.type == EventType.Layout && shrink)
      {
        settingsWindowRect.rect.height = 0;
        shrink = false;
      }
      Utilities.UI.createWindow(currentSettings.getBool("showSettings"), settingsWindowID, ref settingsWindowRect, settingsWindow, "VAB/SPH Camera", settingsWindowStyle);

      Utilities.UI.showTooltip();
    }

    private void settingsWindow(int id)
    {
      GUILayout.BeginVertical();
      rotationSpeed = Utilities.UI.createSlider("Rotation speed", rotationSpeed, 1, 10, 1, textStyle, numberFieldStyle, horizontalSlider, horizontalSliderThumb);
      HeightSpeed = Utilities.UI.createSlider("Height speed", HeightSpeed, 1, 10, 1, textStyle, numberFieldStyle, horizontalSlider, horizontalSliderThumb);
      zoomSpeed = Utilities.UI.createSlider("Zoom speed", zoomSpeed, 1, 10, 1, textStyle, numberFieldStyle, horizontalSlider, horizontalSliderThumb);
      if (Utilities.UI.createToggle("Extend hanger", extendHangar, toggleStyle))
      {
        if (editorMode == EditorFacility.VAB)
        {
          extendVAB.x = Utilities.UI.createSlider("VAB size X", extendVAB.x, 0, 300, 0.1f, textStyle, numberFieldStyle, horizontalSlider, horizontalSliderThumb);
          extendVAB.y = Utilities.UI.createSlider("VAB size Y", extendVAB.y, 0, 300, 0.1f, textStyle, numberFieldStyle, horizontalSlider, horizontalSliderThumb);
          extendVAB.z = Utilities.UI.createSlider("VAB size Z", extendVAB.z, 0, 300, 0.1f, textStyle, numberFieldStyle, horizontalSlider, horizontalSliderThumb);
        }
        else
        {
          extendSPH.x = Utilities.UI.createSlider("SPH size X", extendSPH.x, 0, 300, 0.1f, textStyle, numberFieldStyle, horizontalSlider, horizontalSliderThumb);
          extendSPH.y = Utilities.UI.createSlider("SPH size Y", extendSPH.y, 0, 300, 0.1f, textStyle, numberFieldStyle, horizontalSlider, horizontalSliderThumb);
          extendSPH.z = Utilities.UI.createSlider("SPH size Z", extendSPH.z, 0, 300, 0.1f, textStyle, numberFieldStyle, horizontalSlider, horizontalSliderThumb);
        }
        extendHangar = true;
      }
      else
      {
        extendHangar = false;
        shrink = true;
      }
      Utilities.UI.createOptionSwitcher("Use:", Toolbar.toolbarOptions, ref toolbarSelected);

      GUILayout.BeginHorizontal();
      if (Utilities.UI.createButton("Save", buttonStyle))
      {
        if (extendHangar)
        {
          if (editorMode == EditorFacility.VAB)
          {
            currentSettings.set("extendVABX", extendVAB.x);
            currentSettings.set("extendVABY", extendVAB.y);
            currentSettings.set("extendVABZ", extendVAB.z);
            setHeightLimits(extendVAB.x, extendVAB.y, extendVAB.z);
          }
          else
          {
            currentSettings.set("extendSPHX", extendSPH.x);
            currentSettings.set("extendSPHY", extendSPH.y);
            currentSettings.set("extendSPHZ", extendSPH.z);
            setHeightLimits(extendSPH.x, extendSPH.y, extendSPH.z);
          }
        }
        currentSettings.set("extendHanger", extendHangar);
        if (extendHangar)
        {
          ThreadPool.QueueUserWorkItem(new WaitCallback(updateBounds));
        }
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
  }
}