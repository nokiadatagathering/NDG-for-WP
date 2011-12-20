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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using com.comarch.mobile.ndg.Languages;
using com.comarch.mobile.ndg.MessageDialog;
using com.comarch.mobile.ndg.Model;
using com.comarch.mobile.ndg.Settings.Model;
using com.comarch.mobile.ndg.ViewModel;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// Class contains all methods used during operation on ListOfSurveysPage.
    /// </summary>
    public partial class ListOfSurveysPage : PhoneApplicationPage
    {
        private ListOfSurveysViewModel _viewModel;
        private bool _isNewPageInstance = false;
        private bool _isFavoriteView = false;
        private string _oldTheme = OperationsOnSettings.Instance.Theme;
        private string _oldLanguage = AppResources.Culture.ToString();
        ApplicationBarIconButton _appBarFavorites;

        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public ListOfSurveysPage()
        {
            InitializeComponent();
            _isNewPageInstance = true;
            Loaded += new RoutedEventHandler(ListOfSurveysPage_Loaded);
        }

        private void ListOfSurveysPage_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = new ListOfSurveysViewModel();
            _viewModel.Read();

            bool isLanguageChanged = (AppResources.Culture.ToString() != _oldLanguage);
            _oldLanguage = AppResources.Culture.ToString();

            bool isThemeChanged = (OperationsOnSettings.Instance.Theme != _oldTheme);
            _oldTheme = OperationsOnSettings.Instance.Theme;

            if (_isNewPageInstance || isLanguageChanged || isThemeChanged)
            {
                BuildApplicationBar();
            }

            if (_isFavoriteView)
                ListOfSurveysBox.DataContext = from favorites in _viewModel.List where favorites.IsFavorite == true select favorites;
            else
                ListOfSurveysBox.DataContext = _viewModel.List;

            BusyIndicator.DataContext = _viewModel.Connection.Busy;
            MessageView.AssignDisplay(_viewModel.Connection.Message);
            CheckNewSurvey();
        }

        private void BuildApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar
            ApplicationBar = new ApplicationBar();

            ApplicationBar.ForegroundColor = (App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color;
            ApplicationBar.BackgroundColor = (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color;

            // Create a new menu item with the localized string from AppResources
            ApplicationBarMenuItem appBarCheckNewSurveys = new ApplicationBarMenuItem(Languages.AppResources.listOfSurveysPageAppBar_CheckNewSurveys);
            appBarCheckNewSurveys.Click += OnCheckForNewSurveys;
            ApplicationBarMenuItem appBarTestConnection = new ApplicationBarMenuItem(Languages.AppResources.listOfSurveysPageAppBar_TestConnection);
            appBarTestConnection.Click += OnTestConnection;

            ApplicationBar.MenuItems.Add(appBarCheckNewSurveys);
            ApplicationBar.MenuItems.Add(appBarTestConnection);
#if DEBUG
            ApplicationBarMenuItem appBarCheckUpdate = new ApplicationBarMenuItem(Languages.AppResources.listOfSurveysPageAppBar_CheckUpdate);
            appBarCheckUpdate.Click += OnCheckForUpdates;
            ApplicationBar.MenuItems.Add(appBarCheckUpdate);
#endif
            ApplicationBarIconButton appBarResultsFilter = new ApplicationBarIconButton(new Uri("/View/icons/FilterResultsIcon.png", UriKind.Relative));
            appBarResultsFilter.Text = Languages.AppResources.listOfSurveysPageAppBar_Filter;
            appBarResultsFilter.Click += OnResultsFilter;

            ApplicationBarIconButton appBarSettings = new ApplicationBarIconButton(new Uri("/View/icons/SettingIcon.png", UriKind.Relative));
            appBarSettings.Text = Languages.AppResources.listOfSurveysPageAppBar_Settings;
            appBarSettings.Click += OnSettings;

            _appBarFavorites = new ApplicationBarIconButton(new Uri("/View/icons/FavoriteIcon.png", UriKind.Relative));
            _appBarFavorites.Text = Languages.AppResources.listOfSurveysPageAppBar_FavoriteSurveys;
            _appBarFavorites.Click += OnFavorites;
            
            ApplicationBar.Buttons.Add(appBarResultsFilter);
            ApplicationBar.Buttons.Add(appBarSettings);
            ApplicationBar.Buttons.Add(_appBarFavorites);
        }

        private void ChooseSurvey(object sender, SelectionChangedEventArgs e)
        {
            _isNewPageInstance = false;
            if (ListOfSurveysBox.SelectedIndex == -1)
                return;
            SurveyBasicInfo selectedItem = (SurveyBasicInfo)ListOfSurveysBox.SelectedItem;
            string strSurveyName = selectedItem.Name;
            string strSurveyId = selectedItem.SurveyId;
            bool isFavorite = selectedItem.IsFavorite;
            if ( strSurveyId != null && strSurveyName != null)
                NavigationService.Navigate(new Uri(string.Format("/View/SurveyPage.xaml?SurveyName={0}&SurveyId={1}&isFavorite={2}", strSurveyName, strSurveyId, isFavorite), UriKind.Relative));
            ListOfSurveysBox.SelectedIndex = -1;
        }

        private void OnSettings(object sender, EventArgs e)
        {
            _isNewPageInstance = false;
            NavigationService.Navigate(new Uri("/View/SettingsPage.xaml", UriKind.Relative));
        }

        private void OnResultsFilter(object sender, EventArgs e)
        {
            _isNewPageInstance = false;
            NavigationService.Navigate(new Uri("/View/ResultsFilterPage.xaml", UriKind.Relative));
        }

        private void OnCheckForNewSurveys(object sender, EventArgs e)
        {
            _isNewPageInstance = false;
            NavigationService.Navigate(new Uri("/View/ListOfNewSurveysPage.xaml", UriKind.Relative));
        }

        private void OnTestConnection(object sender, EventArgs e)
        {
            _isNewPageInstance = false;
            _viewModel.Connection.Ping();
        }

        private void OnCheckForUpdates(object sender, EventArgs e)
        {
            _isNewPageInstance = false;
            NavigationService.Navigate(new Uri("/View/UpdatesPage.xaml", UriKind.Relative));
        }

        private void OnFavorites(object sender, EventArgs e)
        {
            if (_isFavoriteView)
            {
                ListOfSurveysBox.DataContext = _viewModel.List;
                _appBarFavorites.IconUri = new Uri("/View/icons/FavoriteIcon.png", UriKind.Relative);
                _appBarFavorites.Text = Languages.AppResources.listOfSurveysPageAppBar_FavoriteSurveys;
                _isFavoriteView = false;
            }
            else
            {
                ListOfSurveysBox.DataContext = from favorites in _viewModel.List where favorites.IsFavorite == true select favorites;
                _appBarFavorites.IconUri = new Uri("/View/icons/UnFavoriteIcon.png", UriKind.Relative);
                _appBarFavorites.Text = Languages.AppResources.listOfSurveysPageAppBar_AllSurveys;
                _isFavoriteView = true;
            }
        }

        private void CheckNewSurvey()
        {
            if (OperationsOnSettings.Instance.AutoCheckNewSurvey && _isNewPageInstance)
            {
                ListOfNewSurveysViewModel newSurveyViewModel = new ListOfNewSurveysViewModel();
                newSurveyViewModel.Operations.CheckForNewSurveys();

                newSurveyViewModel.DownloadListStatus.Message.DialogBoxEvent += (object sender, EventArgs args) =>
                {
                    //don't display message if server not found
                    if ((newSurveyViewModel.DownloadListStatus.Message.Field != Languages.AppResources.checkForNewSurveyList_ServerNotFound) && (newSurveyViewModel.DownloadListStatus.Message.Field != Languages.AppResources.checkForNewSurveyList_ServerIncompleteResponse))
                    {
                        ShowDownloadCancelMessageBox messageBox = new ShowDownloadCancelMessageBox();
                        messageBox.Title = newSurveyViewModel.DownloadListStatus.Message.Field;

                        messageBox.Completed += (object snd, EventArgs arg) =>
                        {
                            if (App.AppDictionary.ContainsKey("NewSurveys"))
                                App.AppDictionary["NewSurveys"] = newSurveyViewModel.List;
                            else
                                App.AppDictionary.Add("NewSurveys", newSurveyViewModel.List);

                            if (((ShowDownloadCancelMessageBox)snd).Response == ShowDownloadCancelMessageBox.MessageResponse.Show) //show list of new surveys
                                NavigationService.Navigate(new Uri("/View/ListOfNewSurveysPage.xaml?action=showListOfSurveys", UriKind.Relative));
                            else if (((ShowDownloadCancelMessageBox)snd).Response == ShowDownloadCancelMessageBox.MessageResponse.Download) //download new surveys
                                NavigationService.Navigate(new Uri("/View/ListOfNewSurveysPage.xaml?action=downloadNewSurveys", UriKind.Relative));
                        };

                        messageBox.Show();
                    }
                    //server not found
                    else
                    {
                        MessageBox.Show(newSurveyViewModel.DownloadListStatus.Message.Field);
                    }
                };

                _isNewPageInstance = false;
            }
        }

        /// <summary>
        /// Standard WP7 method which is running always when user navigates to page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((Application.Current as App).ApplicationState == App.AplicationStates.Activated)
            {
                // Do what you have to do when application is activated and change state for runing. 
                (Application.Current as App).ApplicationState = App.AplicationStates.Runing;
            }

            if (!OperationsOnSettings.Instance.IsRegistered())
            {
                NavigationService.Navigate(new Uri("/View/RegistrationPage.xaml", UriKind.Relative));
            }
            else
            {
                bool? isEncryptionEnabled = OperationsOnSettings.Instance.IsEncryptionEnabled;
                if (isEncryptionEnabled == null)
                {
                    NavigationService.Navigate(new Uri("/View/EnableEncryptionPage.xaml", UriKind.Relative));
                }
                else
                {
                    if ((bool)isEncryptionEnabled && !App.AppDictionary.ContainsKey("EncryptionPassword"))
                        NavigationService.Navigate(new Uri("/View/EncryptionPasswordPage.xaml", UriKind.Relative));
                }
            }
            PageTitle.Text = Languages.AppResources.listOfSurveysPage_PageTitle;

            // clean filtering criteria
            if (App.AppDictionary.ContainsKey("Latitude"))
                App.AppDictionary["Latitude"] = string.Empty;
            else
                App.AppDictionary.Add("Latitude", string.Empty);

            if (App.AppDictionary.ContainsKey("Longitude"))
                App.AppDictionary["Longitude"] = string.Empty;
            else
                App.AppDictionary.Add("Longitude", string.Empty);

            if (App.AppDictionary.ContainsKey("Radius"))
                App.AppDictionary["Radius"] = string.Empty;
            else
                App.AppDictionary.Add("Radius", string.Empty);
        }

        /// <summary>
        /// Standard WP7 method which is running always when user presses back key.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (BusyIndicator.IsBusy == true)
            {
                _viewModel.Connection.Abort();
                e.Cancel = true;
            }

            if (_isFavoriteView)
            {
                e.Cancel = true;
                OnFavorites(null, null);
            }
        }
    }
}