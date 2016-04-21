using KerboKatz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerboKatz.FPSL
{
  public class Settings : SettingsBase<Settings>
  {
    public bool useVSync = true;
    public int active = 30;
    public int background = 5;
    public bool disable;
    public bool showSettings;
  }
}
