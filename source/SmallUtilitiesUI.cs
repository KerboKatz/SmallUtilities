using KerboKatz.Classes;
using System.Collections.Generic;
using UnityEngine;
using KerboKatz;

namespace KerboKatz
{
  partial class SmallUtilities : KerboKatzBase
  {
    private bool initStyle;
    private int settingsWindowID = 971304;
    private Rect settingsWindowRect = new Rect();
    private GUIStyle settingsWindowStyle;
    private GUIStyle buttonStyle;
    private void InitStyle()
    {
      settingsWindowStyle = new GUIStyle(HighLogic.Skin.window);
      settingsWindowStyle.fixedWidth = 200;


      buttonStyle = new GUIStyle(HighLogic.Skin.button);
      buttonStyle.fixedWidth = 150;

      initStyle = true;
    }

    void OnGUI()
    {
      if (!initStyle)
        InitStyle();
      Utilities.UI.createWindow(currentSettings.getBool("showSettings"), settingsWindowID, ref settingsWindowRect, settingsWindow, "KerboKatz small utilities", settingsWindowStyle);
      Utilities.UI.showTooltip();
    }

    private void settingsWindow(int id)
    {
      GUILayout.BeginVertical();
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if (Utilities.UI.createButton("FPSLimiter",buttonStyle))
      {
        FPSLimiter.toggleSettings();
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
      Utilities.UI.updateTooltipAndDrag();
    }
  }
}