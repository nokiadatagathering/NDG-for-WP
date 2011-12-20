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
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using com.comarch.mobile.ndg.Languages;
using com.comarch.mobile.ndg.MessageDialog;
using com.comarch.mobile.ndg.Model.SurveyForms;
using com.comarch.mobile.ndg.Settings.Model;
using com.comarch.mobile.ndg.Settings.ViewModel;
using com.comarch.mobile.ndg.Validation;
using com.comarch.mobile.ndg.View;

namespace com.comarch.mobile.ndg.Settings
{
    /// <summary>
    /// Class contains all methods used during operation on SettingsPage.
    /// </summary>
    public partial class SettingsPage : PhoneApplicationPage
    {
        private SettingsViewModel _settingsList;
        private int _oldFontSize;
        private string _oldTheme;

        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();

            MessageView.AssignDisplay(OperationsOnSettings.Instance.Message);
        }
        
        private void BuildApplicationBar()
        {
                // Set the page's ApplicationBar to a new instance of ApplicationBar
                ApplicationBar = new ApplicationBar();

                ApplicationBar.ForegroundColor = (App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color;
                ApplicationBar.BackgroundColor = (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color;

                ApplicationBarIconButton appBarSettingsDeleteButton = new ApplicationBarIconButton(new Uri("/View/icons/deleteSettings.png", UriKind.Relative));
                appBarSettingsDeleteButton.Text = Languages.AppResources.settings_deleteSettings;
                appBarSettingsDeleteButton.Click += OnSettingsDelete;
                ApplicationBar.Buttons.Add(appBarSettingsDeleteButton);
        }

        /// <summary>
        /// Standard WP7 method which is running always when user navigates to page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if ((Application.Current as App).ApplicationState == App.AplicationStates.Activated)
            {
                // Do what you have to do when application is activated and change state for runing. 
                (Application.Current as App).ApplicationState = App.AplicationStates.Runing;
            }

            _settingsList = new SettingsViewModel();
            _oldFontSize = OperationsOnSettings.Instance.FontSize;
            BuildApplicationBar();
            LoadSettingsPageContent();
        }
        
        private void OnSettingsDelete(object sender, EventArgs e)
        {
            YesNoMessageBox messageBox = new YesNoMessageBox();
            messageBox.Title = Languages.AppResources.settings_deleteSettings;
            messageBox.Message = Languages.AppResources.settings_deleteSettingsDialogMessage;
            messageBox.Completed += (object YesNosender, EventArgs args) =>
            {
                if (((YesNoMessageBox)YesNosender).Response == YesNoMessageBox.MessageResponse.Yes)
                {
                    _settingsList.DeleteSettings();
                    MessageBox.Show(Languages.AppResources.settings_settingsDeleted);
                    NavigationService.GoBack();
                }
            };
            messageBox.Show();
        }

        /// <summary>
        /// Standard WP7 method which is running always before user navigates to new page.
        /// Saves changes when someone presses start or back button.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            
            _settingsList.SaveSettings();
        }

        private void OnChoiceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _settingsList.SaveSettings();
            bool isOtherLanguage = ((OperationsOnSettings.Instance.Language != AppResources.Culture.ToString()) && (OperationsOnSettings.Instance.Language != string.Empty));
            bool isPhoneLanguage = ((AppResources.Culture.ToString() != CultureInfo.CurrentUICulture.Name) && (OperationsOnSettings.Instance.Language == string.Empty));
            bool isLanguageChanged = isOtherLanguage || isPhoneLanguage;

            if (isLanguageChanged)
            {
                //choosed one of defined app language
                if (isOtherLanguage)
                    AppResources.Culture = new CultureInfo(OperationsOnSettings.Instance.Language);

                // back to phone language
                if (isPhoneLanguage)
                    AppResources.Culture = new CultureInfo(CultureInfo.CurrentUICulture.Name);

                _settingsList.LoadSettings();
                LoadSettingsPageContent();
                BuildApplicationBar();
            }

            bool isThemeChanged = _oldTheme != OperationsOnSettings.Instance.Theme;
            if (isThemeChanged)
            {
                _oldTheme = OperationsOnSettings.Instance.Theme;
                _settingsList.CustomColors();
                BuildApplicationBar();
            }
        }

        private void LoadSettingsPageContent()
        {
            ListOfSettingsChoice.DataContext = from SettingUnits in _settingsList.List where SettingUnits.Type == "choice" select SettingUnits;
            ListOfSettingsCustom.DataContext = from SettingUnits in _settingsList.List where SettingUnits.Type == "custom" select SettingUnits;
            ListOfSettingsBool.DataContext = from SettingUnits in _settingsList.List where SettingUnits.Type == "bool" select SettingUnits;
            PageTitle.Text = Languages.AppResources.settingsPage_PageTitle;
        }

        private void OnCustomValueChanged(object sender, RoutedEventArgs e)
        {
            if (((ValidationControl)sender).Name == "FontSize")
            {
                int minValue = 15;
                int maxValue = 35;
                RangeValidationRule rule = new RangeValidationRule(NumericQuestion.Types.IntegerType, minValue, maxValue);

                string newValue = ((ValidationControl)sender).Text;
                ((ValidationControl)sender).IsValid = true;
                ((ValidationControl)sender).IsValid = rule.Validate(newValue);
                if (((ValidationControl)sender).IsValid)
                {
                    OperationsOnSettings.Instance.FontSize = Convert.ToInt32(newValue);
                    _oldFontSize = OperationsOnSettings.Instance.FontSize;
                    LoadSettingsPageContent();
                }
                else
                {
                    ((ValidationControl)sender).ValidationContent = string.Format(Languages.AppResources.settingsPage_InvalidCustomValue, minValue.ToString(), maxValue.ToString());
                }
            }
        }

        private void OnCustomSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isFontSizeChanged = _oldFontSize != OperationsOnSettings.Instance.FontSize;

            if (isFontSizeChanged)
            {
                _oldFontSize = OperationsOnSettings.Instance.FontSize;
                LoadSettingsPageContent();
            }
        }
    }
}