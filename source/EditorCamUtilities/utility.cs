using KerboKatz.Classes;
using System.Collections.Generic;
using UnityEngine;

namespace KerboKatz
{
  public static partial class EditorCamUtilitiesExtensions
  {
    private static Dictionary<KeyBinding, KeyBindingStorage> KeyBindingStorage = new Dictionary<KeyBinding, KeyBindingStorage>();
    private static Dictionary<AxisBinding, AxisBindingStorage> AxisBindingStorage = new Dictionary<AxisBinding, AxisBindingStorage>();
    private static settings keybindingSettings;

    public static void saveDefault(this KeyBinding KeyBinding, string name)
    {
      checkSaveFile();
      if (keybindingSettings.isSet("ended") && !keybindingSettings.getBool("ended"))
      {
        KeyBinding.primary = getKeyCode(keybindingSettings.getString(name + "_primary"));
        KeyBinding.secondary = getKeyCode(keybindingSettings.getString(name + "_secondary"));
      }
      keybindingSettings.set(name + "_primary", KeyBinding.primary.ToString());
      keybindingSettings.set(name + "_secondary", KeyBinding.secondary.ToString());
      keybindingSettings.save();
      if (!KeyBindingStorage.ContainsKey(KeyBinding))
        KeyBindingStorage.Add(KeyBinding, new KeyBindingStorage(KeyBinding.primary, KeyBinding.secondary));
    }

    private static void checkSaveFile()
    {
      if (keybindingSettings == null)
      {
        keybindingSettings = new settings();
        keybindingSettings.load("", "keybindingSettings", "keybindingSettings");
      }
    }

    public static void setSaveFileStatusToStarted()
    {
      checkSaveFile();
      keybindingSettings.set("ended", false);
      keybindingSettings.save();
    }

    public static void setSaveFileStatusToEnded()
    {
      checkSaveFile();
      keybindingSettings.set("ended", true);
      keybindingSettings.save();
    }

    public static void setNone(this KeyBinding KeyBinding)
    {
      KeyBinding.primary = KeyCode.None;
      KeyBinding.secondary = KeyCode.None;
    }

    public static void reset(this KeyBinding KeyBinding)
    {
      if (KeyBindingStorage.ContainsKey(KeyBinding))
      {
        KeyBinding.primary = KeyBindingStorage[KeyBinding].primary;
        KeyBinding.secondary = KeyBindingStorage[KeyBinding].secondary;
      }
    }

    public static KeyCode getKeyCode(string key)
    {
      return (KeyCode)System.Enum.Parse(typeof(KeyCode), key);
    }

    public static KeyCode getDefaultPrimary(this KeyBinding KeyBinding)
    {
      if (KeyBindingStorage.ContainsKey(KeyBinding))
      {
        return KeyBindingStorage[KeyBinding].primary;
      }
      return KeyCode.None;
    }

    public static void saveDefault(this AxisBinding AxisBinding, string name)
    {
      checkSaveFile();
      if (keybindingSettings.isSet("ended") && !keybindingSettings.getBool("ended"))
      {
        AxisBinding.primary.scale = keybindingSettings.getFloat(name + "_primaryScale");
        AxisBinding.secondary.scale = keybindingSettings.getFloat(name + "_secondaryScale");
      }
      keybindingSettings.set(name + "_primaryScale", AxisBinding.primary.scale.ToString());
      keybindingSettings.set(name + "_secondaryScale", AxisBinding.secondary.scale.ToString());
      keybindingSettings.save();
      if (!AxisBindingStorage.ContainsKey(AxisBinding))
        AxisBindingStorage.Add(AxisBinding, new AxisBindingStorage(AxisBinding.primary.scale, AxisBinding.secondary.scale));
    }

    public static void setZero(this AxisBinding AxisBinding)
    {
      AxisBinding.primary.scale = 0;
      AxisBinding.secondary.scale = 0;
    }

    public static void reset(this AxisBinding AxisBinding)
    {
      if (AxisBindingStorage.ContainsKey(AxisBinding))
      {
        AxisBinding.primary.scale = AxisBindingStorage[AxisBinding].primaryScale;
        AxisBinding.secondary.scale = AxisBindingStorage[AxisBinding].secondaryScale;
      }
    }
  }
}