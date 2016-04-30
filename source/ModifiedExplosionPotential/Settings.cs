using KerboKatz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerboKatz.MEP
{
  public class Settings : SettingsBase<Settings>
  {
    public class ExplosionValue
    {
      public string name;
      public float explosiveness;
    }

    public float baseExplosiveness = 0.1f;
    public float updateInterval = 0.1f;

    private Dictionary<string, ExplosionValue> _explosionValues = new Dictionary<string, ExplosionValue>();
    public List<ExplosionValue> explosionValues = new List<ExplosionValue>();

    protected override void OnLoaded()
    {
      _explosionValues.Clear();
      foreach (var setting in explosionValues)
      {
        _explosionValues.Add(setting.name, setting);
      }
    }
    protected override void OnSave()
    {
      explosionValues = new List<ExplosionValue>(_explosionValues.Values);
    }

    internal bool IsExplosionValueSet(string name)
    {
      return _explosionValues.ContainsKey(name);
    }

    internal void AddExplosionValue(string name, float density)
    {
      var value = new ExplosionValue();
      value.name = name;
      value.explosiveness = density;
      _explosionValues.Add(name, value);
    }
    internal bool GetExplosiveness(string name, out float explosiveness)
    {
      explosiveness = 0;
      ExplosionValue value;
      if (_explosionValues.TryGetValue(name, out value))
      {
        explosiveness = value.explosiveness;
        return true;
      }
      return false;
    }
  }
}
