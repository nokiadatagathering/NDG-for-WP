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
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using com.comarch.mobile.ndg.MessageDialog;
using com.comarch.mobile.ndg.Model;
using com.comarch.mobile.ndg.ViewModel;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// Class contains all methods used during operation on SurveyPage.
    /// </summary>
    public partial class SurveyPage : PhoneApplicationPage
    {
        private SurveyViewModel _viewModel;
        private bool _isNewInstance = true;
        private ApplicationBarIconButton _appBarFavorite;

        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public SurveyPage()
        {
            InitializeComponent();
        }

        private void BuildApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar
            ApplicationBar = new ApplicationBar();

            ApplicationBar.ForegroundColor = (App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color;
            ApplicationBar.BackgroundColor = (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color;

            // Create a new button and set the text value to the localized string from AppResources
            ApplicationBarIconButton appBarDeleteSurveyButton = new ApplicationBarIconButton(new Uri("/View/icons/NewResultIcon.png", UriKind.Relative));
            appBarDeleteSurveyButton.Text = Languages.AppResources.surveyPageAppBar_NewResult;
            appBarDeleteSurveyButton.Click += OnNewResult;

            _appBarFavorite = new ApplicationBarIconButton();
            _appBarFavorite.Click += OnFavorite;

            if (Convert.ToBoolean(_viewModel.IsFavorite))
            {
                _appBarFavorite.IconUri = new Uri("/View/icons/RemoveFavoriteIcon.png", UriKind.Relative);
                _appBarFavorite.Text = Languages.AppResources.surveyPageAppBar_RemoveFromFavorite;
            }
            else
            {
                _appBarFavorite.IconUri = new Uri("/View/icons/AddFavoriteIcon.png", UriKind.Relative);
                _appBarFavorite.Text = Languages.AppResources.surveyPageAppBar_AddToFavorite;
            }

            ApplicationBar.Buttons.Add(appBarDeleteSurveyButton);
            ApplicationBar.Buttons.Add(_appBarFavorite);

            // Create a new menu item with the localized string from AppResources
            ApplicationBarMenuItem appBarDeleteSurvey = new ApplicationBarMenuItem(Languages.AppResources.surveyPageAppBar_Delete);
            appBarDeleteSurvey.Click += OnDeleteSurvey;
#if DEBUG
            ApplicationBarMenuItem testsBar = new ApplicationBarMenuItem("Tests results");
            testsBar.Click += OnTestResults;

            ApplicationBar.MenuItems.Add(testsBar);
#endif
            ApplicationBar.MenuItems.Add(appBarDeleteSurvey);
        }

        /// <summary>
        /// Standard WP7 method which is running always when user navigates to page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isNewInstance)
            {
                if ((Application.Current as App).ApplicationState == App.AplicationStates.Activated)
                {
                    // Do what you have to do when application is activated and change state for runing. 
                    (Application.Current as App).ApplicationState = App.AplicationStates.Runing;
                }
                base.OnNavigatedTo(e);
                string strSurveyName = string.Empty;
                string strSurveyId = string.Empty;
                string isFavorite;
                if (NavigationContext.QueryString.TryGetValue("SurveyName", out strSurveyName) && NavigationContext.QueryString.TryGetValue("SurveyId", out strSurveyId) && NavigationContext.QueryString.TryGetValue("isFavorite", out isFavorite))
                {
                    PageTitle.Text = strSurveyName;
                    _viewModel = new SurveyViewModel(strSurveyId);
                    _viewModel.IsFavorite = isFavorite;
                    BusyIndicator.DataContext =_viewModel.ProgressBar ;
                    ListOfResults.DataContext = _viewModel.ListOfResults;
                    MessageView.AssignDisplay(_viewModel.Message);
                }
                _isNewInstance = false;
                BuildApplicationBar();
            }
            _viewModel.ReloadList();
        }

        private void OnDeleteSurvey(object sender, EventArgs e)
        {

            YesNoMessageBox messageBox = new YesNoMessageBox();
            messageBox.Title = Languages.AppResources.surveyPage_deleteSurveyDialogTitle;
            messageBox.Message = Languages.AppResources.surveyPage_deleteSurveyDialogMessage;
            messageBox.Completed += (object YesNosender, EventArgs args) =>
            {
                if (((YesNoMessageBox)YesNosender).Response == YesNoMessageBox.MessageResponse.Yes)
                {
                    try
                    {
                        OperationsOnListOfSurveys op = new OperationsOnListOfSurveys(new ObservableCollection<SurveyBasicInfo>(), new DownloadListStatus());
                        op.Delete(_viewModel.SurveyId);

                        NavigationService.GoBack();
                    }
                    catch (IsolatedStorageException)
                    {
                        MessageBox.Show(Languages.AppResources.surveyPage_deleteSurveyError);
                    }
                }
            };
            messageBox.Show();
        }

        private void OnNewResult(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/View/FillingSurveyPage.xaml?SurveyId={0}", _viewModel.SurveyId), UriKind.Relative));
        }

        private void OnEditExistingResult(object sender, RoutedEventArgs e)
        {
            ResultBasicInfo selectedListBoxItem = (sender as MenuItem).DataContext as ResultBasicInfo;

            String resultId = selectedListBoxItem.Id;
            String resultTitle = selectedListBoxItem.Title;
            NavigationService.Navigate(new Uri(string.Format("/View/FillingSurveyPage.xaml?SurveyId={0}&resultId={1}&resultTitle={2}", _viewModel.SurveyId, resultId, resultTitle), UriKind.Relative));
        }

        private void OnDeleteResult(object sender, RoutedEventArgs e)
        {
            YesNoMessageBox messageBox = new YesNoMessageBox();
            messageBox.Title = Languages.AppResources.surveyPage_deleteResultQuestionTitle;
            messageBox.Message = Languages.AppResources.surveyPage_deleteResultQuestionMessage;
            messageBox.Completed += (object YesNosender, EventArgs args) =>
            {
                if (((YesNoMessageBox)YesNosender).Response == YesNoMessageBox.MessageResponse.Yes)
                {
                    try
                    {
                        ResultBasicInfo selectedListBoxItem = (sender as MenuItem).DataContext as ResultBasicInfo;
                        _viewModel.DeleteResult(selectedListBoxItem);
                    }
                    catch (IsolatedStorageException)
                    {
                        MessageBox.Show(Languages.AppResources.surveyPage_deleteResultError);
                    }
                }
            };
            messageBox.Show();

        }

        private void OnShowResult(object sender, RoutedEventArgs e)
        {
            ResultBasicInfo selectedListBoxItem = (sender as MenuItem).DataContext as ResultBasicInfo;

            String resultId = selectedListBoxItem.Id;
            String resultTitle = selectedListBoxItem.Title;
            NavigationService.Navigate(new Uri(string.Format("/View/PreviewSurveyPage.xaml?SurveyId={0}&resultId={1}&resultTitle={2}", _viewModel.SurveyId, resultId, resultTitle), UriKind.Relative));
        }

        private void OnFavorite(object sender, EventArgs e)
        {
            OperationsOnListOfSurveys op = new OperationsOnListOfSurveys(new ObservableCollection<SurveyBasicInfo>(), new DownloadListStatus());

            //remove from favorite surveys
            if (Convert.ToBoolean(_viewModel.IsFavorite))
            {
                op.RemoveFromFavorite(_viewModel.SurveyId);
                _appBarFavorite.IconUri = new Uri("/View/icons/AddFavoriteIcon.png", UriKind.Relative);
                _appBarFavorite.Text = Languages.AppResources.surveyPageAppBar_AddToFavorite;
                _viewModel.IsFavorite = "false";
            }
            //add to favorite surveys
            else
            {
                op.AddToFavorite(_viewModel.SurveyId);
                _appBarFavorite.IconUri = new Uri("/View/icons/RemoveFavoriteIcon.png", UriKind.Relative);
                _appBarFavorite.Text = Languages.AppResources.surveyPageAppBar_RemoveFromFavorite;
                _viewModel.IsFavorite = "true";
            }
        }

        private void OnSendResult(object sender, RoutedEventArgs e)
        {
            ResultBasicInfo selectedListBoxItem = (sender as MenuItem).DataContext as ResultBasicInfo;
            _viewModel.SendResult(selectedListBoxItem);
        }

        private void OnBackToEditClick(object sender, RoutedEventArgs e)
        {
            ResultBasicInfo selectedListBoxItem = (sender as MenuItem).DataContext as ResultBasicInfo;
            _viewModel.UnmarkSent(selectedListBoxItem);
        }

        private void OnBackPressed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_viewModel.SendingInProgress)
            {
                e.Cancel = true;
                _viewModel.AbortSaving();
            }
        }
#if DEBUG
        private void OnTestResults(object sender, EventArgs e)
        {
            _viewModel.PreapreResultsForTests();
        }
#endif
       
    }
}