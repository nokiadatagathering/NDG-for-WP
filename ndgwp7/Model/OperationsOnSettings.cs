/* 
    Copyright (C) 2011  Comarch
  
    NDG for WP7 is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either 
    version 2.1 of the License, or (at your option) any later version.
  
    NDG is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.
  
    You should have received a copy of the GNU Lesser General Public
    License along with NDG.  If not, see <http://www.gnu.org/licenses/
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using com.comarch.mobile.ndg.MessageDialog;

namespace com.comarch.mobile.ndg.Settings.Model
{
    /// <summary>
    /// Contains operation on settings. New/changed values of settings are saved in IsolatedStorageSettings and all setting values are available in any part of application. Same rule applies when you want to modify your settings.
    /// </summary>
    sealed public class OperationsOnSettings
    {
        private static readonly OperationsOnSettings _instance = new OperationsOnSettings();

        /// <summary>
        /// Returns instance of sealed class when all setting values are available.
        /// </summary>
        public static OperationsOnSettings Instance
        {
            get { return _instance; }
        }
        
        /// <summary>
        /// Contains MessageBox information field when some exception is detected.
        /// </summary>
        public DialogBox Message { get; set; }

        private SettingEntity SettingUnit { get; set; }

        private OperationsOnSettings()
        {
            SettingUnit = new SettingEntity();
            Message = new DialogBox();

            try
            {
                if (!IsolatedStorageSettings.ApplicationSettings.Contains("_isSetSettings"))
                {
                    DefaultSettings();
                }
            }
            catch (IsolatedStorageException)
            {
                Message.Show(Languages.AppResources.settings_IsolatedStorageFailed);
            }
        }

        private void DefaultSettings()
        {
            ObservableCollection<SettingEntity> list = new ObservableCollection<SettingEntity>();
            list.Add(new SettingEntity() { Key = "_isSetSettings", Type = "bool", Value = "true" });
            list.Add(new SettingEntity() { Key = "_Encryption", Type = "bool", Value = "" });
            list.Add(new SettingEntity() { Key = "_ServerURL", Value = "http://127.0.0.1:8080/ndg-servlets/", Type = "text" });
            list.Add(new SettingEntity() { Key = "Language", Type = "choice", Value = "0", RealCapabilities = new List<string> { string.Empty, "pl-PL", "en-US", "es-ES" } });
            list.Add(new SettingEntity() { Key = "Theme", Type = "choice", Value = "0", RealCapabilities = new List<string> { "ndg", "green", "highcontrast" } });
            list.Add(new SettingEntity() { Key = "FontSize", Type = "custom", Value = "2", RealCapabilities = new List<string> { "", "20", "26", "32" }, Custom = "26" });
            list.Add(new SettingEntity() { Key = "PhotoResolution", Type = "choice", Value = "0", Capabilities = new List<string> { "320x240", "640x480", "1024x768", "1280x960" }, RealCapabilities = new List<string> { "320x240", "640x480", "1024x768", "1280x960" } });
            list.Add(new SettingEntity() { Key = "GPS", Type = "bool", Value = "False" });
            list.Add(new SettingEntity() { Key = "AutoCheckNewSurvey", Type = "bool", Value = "False" });

            Save(list);
            list.Clear();
        }

        /// <summary>
        /// Loads all setting needed on SettingPage with information about type of settings and values which can be selected (in choice setting like Language or Font Size)
        /// </summary>
        /// <returns>List of settings</returns>
        public ObservableCollection<SettingEntity> Load()
        {
            ObservableCollection<SettingEntity> list = new ObservableCollection<SettingEntity>();
            IsolatedStorageSettings allsettings = IsolatedStorageSettings.ApplicationSettings;

            foreach (var savedSetting in IsolatedStorageSettings.ApplicationSettings)
            {
                if (!savedSetting.Key.Contains("_"))
                {
                    SettingUnit = (SettingEntity)allsettings[savedSetting.Key];
                    SettingEntity stgUnit = new SettingEntity() { Key = SettingUnit.Key, Value = SettingUnit.Value, Type = SettingUnit.Type, RealCapabilities = SettingUnit.RealCapabilities };
                    switch (savedSetting.Key)
                    {
                        case "GPS":
                            stgUnit.Display = Languages.AppResources.settings_GPS;
                            break;
                        case "AutoCheckNewSurvey":
                            stgUnit.Display = Languages.AppResources.settings_AutoCheckNewSurvey;
                            break;
                        case "Language":
                            stgUnit.Capabilities = new List<string> { Languages.AppResources.settings_PhoneLanguage, "Polski", "English", "Español" };
                            stgUnit.Display = Languages.AppResources.settings_Language;
                            break;
                        case "FontSize":
                            stgUnit.Capabilities = new List<string> { Languages.AppResources.settings_Custom, Languages.AppResources.settings_Small, Languages.AppResources.settings_Medium, Languages.AppResources.settings_Large };
                            stgUnit.Display = Languages.AppResources.settings_FontSize;
                            stgUnit.Custom = SettingUnit.Custom;
                            break;
                        case "PhotoResolution":
                            stgUnit.Capabilities = SettingUnit.Capabilities;
                            stgUnit.Display = Languages.AppResources.settings_PhotoResolution;
                            break;
                        case "Theme":
                            stgUnit.Capabilities = new List<string> { Languages.AppResources.settings_NDG, Languages.AppResources.settings_OtherTheme, Languages.AppResources.settings_HighContrast };
                            stgUnit.Display = Languages.AppResources.settings_Theme;
                            break;
                    }
                    list.Add(stgUnit);
                }
            }

            return list;
        }

        /// <summary>
        /// Saves all setting values in IsolatedStorageSettings where key is name of setting and value is SettingEntity class
        /// </summary>
        /// <param name="listOfSettings">List of setting which should be saved is IsolatedStorageSettings.</param>
        public void Save(ObservableCollection<SettingEntity> listOfSettings)
        {
            try {
                if (listOfSettings.Count > 0)
                {
                    foreach (SettingEntity oneSet in listOfSettings)
                    {
                        if (IsolatedStorageSettings.ApplicationSettings.Contains(oneSet.Key))
                            IsolatedStorageSettings.ApplicationSettings[oneSet.Key] = oneSet;
                        else
                            IsolatedStorageSettings.ApplicationSettings.Add(oneSet.Key, oneSet);
                    }

                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
                else
                    throw new ArgumentNullException();
            }
            catch (IsolatedStorageException)
            {
                Message.Show(Languages.AppResources.settings_SaveFailed);
            }
            catch (ArgumentNullException)
            {
                Message.Show(Languages.AppResources.settings_SaveNothing);
            }
        }

        /// <summary>
        /// Checking _IMEI key in IsolatedStorageSettings which stores data that application/device is registered on server and application can works properly.
        /// </summary>
        /// <returns>Return True when device is registered or False when registration process failed or it is first application start.</returns>
        public bool IsRegistered()
        {
            try
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("_IMEI"))
                    return true;
            }
            catch (IsolatedStorageException)
            {
                Message.Show(Languages.AppResources.settings_IsolatedStorageFailed);
            }

            return false;
        }

        /// <summary>
        /// Checking which language option is set (automatic phone language or static language).
        /// </summary>
        /// <returns>Return True when user selected one of define language or False when application language should be the same with phone language</returns>
        public bool IsAppLanguageSet()
        {
            return (ChooseValue("Language") != string.Empty);
        }

        private string Value(string name)
        {
            try
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(name))
                    return ((SettingEntity)IsolatedStorageSettings.ApplicationSettings[name]).Value;
                else
                    return string.Empty;
            }
            catch (IsolatedStorageException)
            {
                return string.Empty;
            }
        }

        private string ChooseValue(string name)
        {
            try
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(name))
                {
                    SettingEntity settingUnit = (SettingEntity)IsolatedStorageSettings.ApplicationSettings[name];
                    return settingUnit.RealCapabilities[Convert.ToInt32(settingUnit.Value)];
                }
                else
                    return string.Empty;
            }
            catch (IsolatedStorageException)
            {
                return string.Empty;
            }
        }

        private string CustomValue(string name)
        {

            if (Value("FontSize") != "0")
                return ChooseValue(name);
            else
            {
                try
                {
                    if (IsolatedStorageSettings.ApplicationSettings.Contains(name))
                    {
                        SettingEntity settingUnit = (SettingEntity)IsolatedStorageSettings.ApplicationSettings[name];
                        return settingUnit.Custom;
                    }
                    else
                        return string.Empty;
                }
                catch (IsolatedStorageException)
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Represents information about IMEI number.
        /// </summary>
        public string IMEI
        {
            get { return Value("_IMEI"); }
            set { Save(new ObservableCollection<SettingEntity>() { new SettingEntity() { Key = "_IMEI", Value = value, Type = "text" } }); }
        }

        /// <summary>
        /// Represents information about GPS configuration.
        /// </summary>
        public bool GPS
        {
            get {
                if (!string.IsNullOrEmpty(Value("GPS")))
                    return Convert.ToBoolean(Value("GPS"));
                else
                    return false;
            }
        }

        /// <summary>
        /// Represents information about checking new survey on application start.
        /// </summary>
        public bool AutoCheckNewSurvey
        {
            get {
                if (!string.IsNullOrEmpty(Value("AutoCheckNewSurvey")))
                    return Convert.ToBoolean(Value("AutoCheckNewSurvey"));
                else
                    return false;
            }
        }

        /// <summary>
        /// Represents information anout server URL.
        /// </summary>
        public string ServerURL
        {
            get { return Value("_ServerURL"); }
            set { Save(new ObservableCollection<SettingEntity>() { new SettingEntity() { Key = "_ServerURL", Value = value, Type = "text" } }); }
        }

        /// <summary>
        /// Represents information about application language.
        /// </summary>
        public string Language
        {
            get { return ChooseValue("Language"); }
        }

        /// <summary>
        /// Represents information about font size in application.
        /// </summary>
        public int FontSize
        {
            get { return Convert.ToInt32(CustomValue("FontSize")); }
            set
            {
                SettingEntity stgEntity = (SettingEntity)IsolatedStorageSettings.ApplicationSettings["FontSize"];
                stgEntity.Custom = value.ToString();
                stgEntity.Value = "0";
                Save(new ObservableCollection<SettingEntity>() { stgEntity });
            }
        }

        /// <summary>
        /// Represents information about font size for list in application.
        /// </summary>
        public int ListFontSize
        {
            get { return Convert.ToInt32(Convert.ToInt32(CustomValue("FontSize")) * 1.6); }
        }

        /// <summary>
        /// Represents information about photo width (resolution).
        /// </summary>
        public int PhotoWidth
        {
            get { return Convert.ToInt32((ChooseValue("PhotoResolution").Split('x'))[0]); }
        }

        /// <summary>
        /// Represents information about photo height (resolution).
        /// </summary>
        public int PhotoHeight
        {
            get { return Convert.ToInt32((ChooseValue("PhotoResolution").Split('x'))[1]); }
        }

        /// <summary>
        /// Represents information about application theme.
        /// </summary>
        public string Theme
        {
            get { return ChooseValue("Theme"); }
        }

        /// <summary>
        /// Represents information about survey encryption.
        /// </summary>
        public bool? IsEncryptionEnabled
        {
            get
            {
                if (!string.IsNullOrEmpty(Value("_Encryption")))
                    return Convert.ToBoolean(Value("_Encryption"));
                else
                    return null;
            }
            set { Save(new ObservableCollection<SettingEntity>() { new SettingEntity() { Key = "_Encryption", Value = value.ToString(), Type = "bool" } }); }
        }

        /// <summary>
        /// Method to clear IsolatedStorageSettings using only for testing (in debug version). This method should be deleted in future!
        /// </summary>
        public void Delete()
        {
            IsolatedStorageSettings.ApplicationSettings.Clear();
        }
    }
}
