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
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using com.comarch.mobile.ndg.Settings.Model;

namespace com.comarch.mobile.ndg.Settings.ViewModel
{
    /// <summary>
    /// Class stores methods used during communication between model and view on SettingPage.
    /// </summary>
    public class SettingsViewModel
    {
        /// <summary>
        /// List of all available settings which should be display on settings page.
        /// </summary>
        public ObservableCollection<SettingEntity> List { get; set; }

        /// <summary>
        /// Default constructor which automatically sets all saved settings to List property.
        /// </summary>
        public SettingsViewModel()
        {
            List = OperationsOnSettings.Instance.Load();
        }

        /// <summary>
        /// Loads all saved settings from IsolatedStorageSettings (as SettingEntity class) and sets values to List property.
        /// </summary>
        public void LoadSettings()
        {
            List = OperationsOnSettings.Instance.Load();
        }

        /// <summary>
        /// Saves settings in IsolatedStorageSettings (new values are loaded from List property).
        /// </summary>
        public void SaveSettings()
        {
            OperationsOnSettings.Instance.Save(List);
        }

        /// <summary>
        /// Clears IsolatedStorageSettings used only for tests. This methed should be delete in future!
        /// </summary>
        public void DeleteSettings()
        {
            OperationsOnSettings.Instance.Delete();
        }

        /// <summary>
        /// Changes application theme by replacing resource with color definition.
        /// </summary>
        public void CustomColors()
        {
            var dictionaries = new ResourceDictionary();
            string source = string.Empty;
            if (OperationsOnSettings.Instance.Theme == "highcontrast")
            {
                if ((Visibility)App.Current.Resources["PhoneLightThemeVisibility"] == System.Windows.Visibility.Visible)
                    source = String.Format("/ndgwp7;component/Theme/lightTheme.xaml");      // for light theme
                else
                    source = String.Format("/ndgwp7;component/Theme/darkTheme.xaml");       // for dark theme
            }
            else
                source = String.Format("/ndgwp7;component/Theme/" + OperationsOnSettings.Instance.Theme + "Theme.xaml");

            var themeStyles = new ResourceDictionary { Source = new Uri(source, UriKind.Relative) };
            dictionaries.MergedDictionaries.Add(themeStyles);

            ResourceDictionary appResources = App.Current.Resources;
            foreach (DictionaryEntry entry in dictionaries.MergedDictionaries[0])
            {
                SolidColorBrush colorBrush = entry.Value as SolidColorBrush;
                SolidColorBrush existingBrush = appResources[entry.Key] as SolidColorBrush;
                if (existingBrush != null && colorBrush != null)
                {
                    existingBrush.Color = colorBrush.Color;
                }
            }

            // if default application theme set background picture
            var app = Application.Current as App;
            if (OperationsOnSettings.Instance.Theme == "ndg")
            {
                BitmapImage bMap = new BitmapImage(new Uri("Theme/background.png", UriKind.RelativeOrAbsolute));
                var imageBrush = new ImageBrush();
                imageBrush.ImageSource = bMap;
                app.RootFrame.Background = imageBrush;
            }
            else
            {
                app.RootFrame.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            }
        }
    }
}
