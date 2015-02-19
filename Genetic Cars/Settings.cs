// ReSharper disable All

using System.Configuration;
using System.Reflection;
using log4net;

namespace Genetic_Cars.Properties {
  // This class allows you to handle specific events on the settings class:
  //  The SettingChanging event is raised before a setting's value is changed.
  //  The PropertyChanged event is raised after a setting's value is changed.
  //  The SettingsLoaded event is raised after the setting values are loaded.
  //  The SettingsSaving event is raised before the setting values are saved.
  internal sealed partial class Settings {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);
        
    public Settings()
    {
      SettingsLoaded += (sender, args) =>
      {
        Log.Info("Loaded settings:");
        foreach (SettingsProperty property in Settings.Default.Properties)
        {
          Log.InfoFormat("{0}={1}",
            property.Name, Settings.Default[property.Name]);
        }
      };

      SettingChanging += (sender, args) =>
      {
        Log.InfoFormat("Setting {0} changed to {1}",
          args.SettingName, args.NewValue);
      };
    }
  }
}
