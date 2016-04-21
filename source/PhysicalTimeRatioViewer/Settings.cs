using KerboKatz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerboKatz.PTRV
{
  public class Settings : SettingsBase<Settings>
  {
    public bool showSettings;
    public float refreshRate;
    public bool showLabels;
    public bool showMaxDeltaTime;
    public bool moveLabelPosition;
  }
}
