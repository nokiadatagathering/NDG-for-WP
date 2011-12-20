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
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using com.comarch.mobile.ndg.MessageDialog;
using com.comarch.mobile.ndg.Model;
using com.comarch.mobile.ndg.Model.Download;
using com.comarch.mobile.ndg.ViewModel;
using System.Windows.Navigation;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// Class contains all methods used during operation on ListOfNewSurveysPage.
    /// </summary>
    public partial class ListOfNewSurveysPage : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;
        ApplicationBarMenuItem _appBarDownload;
        ListOfNewSurveysViewModel _viewModel;

        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public ListOfNewSurveysPage()
        {
            InitializeComponent();
            BuildApplicationBar();
            _isNewPageInstance = true;
        }

        private void BuildApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar
            ApplicationBar = new ApplicationBar();

            ApplicationBar.ForegroundColor = (App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color;
            ApplicationBar.BackgroundColor = (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color;

            // Create a new menu item with the localized string from AppResources
            _appBarDownload = new ApplicationBarMenuItem(Languages.AppResources.listOfNewSurveysPageAppBarr_Download);
            _appBarDownload.Click += OnDownloadSurveys;
            ApplicationBarMenuItem appBarRefresh = new ApplicationBarMenuItem(Languages.AppResources.listOfNewSurveysPageAppBarr_Refresh);
            appBarRefresh.Click += OnRefresh;

            ApplicationBar.MenuItems.Add(_appBarDownload);
            ApplicationBar.MenuItems.Add(appBarRefresh);

        }

        /// <summary>
        /// Standard WP7 method which is running always when user navigates to page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string action = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("action", out action))
            {
                _viewModel = new ListOfNewSurveysViewModel();
                _viewModel.List = App.AppDictionary["NewSurveys"] as ObservableCollection<SurveyBasicInfo>;

                _viewModel.DownloadSurveysStatus.GoBackEventHandler += (object sender, EventArgs args) =>
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        NavigationService.GoBack();
                    });
                };

                _isNewPageInstance = false;
                ListOfNewSurveysBox.DataContext = _viewModel.List;
                Status.DataContext = _viewModel.DownloadListStatus;

                if (action == "downloadNewSurveys")
                    OnDownloadSurveys(null, null);
            }

            if (_isNewPageInstance)
            {
                _viewModel = new ListOfNewSurveysViewModel();
                _viewModel.DownloadListStatus.PropertyChanged += (object sender, PropertyChangedEventArgs args) =>
                {
                    if (args.PropertyName.Equals("IsDownloadButtonEnabled"))
                        _appBarDownload.IsEnabled = _viewModel.DownloadListStatus.IsDownloadButtonEnabled;
                };

                MessageView.AssignDisplay(_viewModel.DownloadListStatus.Message);
                MessageView.AssignDisplay(_viewModel.DownloadSurveysStatus.Message);

                _viewModel.DownloadSurveysStatus.GoBackEventHandler += (object sender, EventArgs args) =>
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        NavigationService.GoBack();
                    });
                };

                if ((Application.Current as App).ApplicationState == App.AplicationStates.Activated)
                {

                    if (!string.IsNullOrEmpty((Application.Current as com.comarch.mobile.ndg.App).ApplicationDataObject))
                    {
                        _viewModel.Operations.StateDictionaryData = (Application.Current as com.comarch.mobile.ndg.App).ApplicationDataObject;
                    }

                    (Application.Current as App).ApplicationState = App.AplicationStates.Runing;
                }
                else
                {
                    _viewModel.Operations.CheckForNewSurveys();
                }
                ListOfNewSurveysBox.DataContext = _viewModel.List;
                Status.DataContext = _viewModel.DownloadListStatus;
                BusyIndicator.DataContext = _viewModel.DownloadListStatus.ProgressBar;
                _isNewPageInstance = false;
            }
        }

        /// <summary>
        /// Standard WP7 method which is running always before user navigates to new page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            (Application.Current as App).ApplicationDataObject = _viewModel.Operations.StateDictionaryData;
        }

        private DownloadNewSurveys _downloader;

        private void OnDownloadSurveys(object sender, EventArgs e)
        {
            if (_viewModel.DownloadSurveysStatus.ProgressBar.IsEnabled || _viewModel.DownloadListStatus.ProgressBar.IsEnabled)
            {
                MessageBox.Show(Languages.AppResources.listOfNewSurveysPage_DownloadingProgress);
                return;
            }
            //else if (_viewModel.List.Count == 0)
            //{
            //    MessageBox.Show(Languages.AppResources.listOfNewSurveysPage_NothingDownload);
            //    return;
            //}
            BusyIndicator.DataContext = _viewModel.DownloadSurveysStatus.ProgressBar;
            _downloader = new DownloadNewSurveys(_viewModel.Operations, new List<SurveyBasicInfo>(_viewModel.List), _viewModel.DownloadSurveysStatus);
            _downloader.Download();
            //_viewModel.List.Clear();
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            if (_viewModel.DownloadSurveysStatus.ProgressBar.IsEnabled || _viewModel.DownloadListStatus.ProgressBar.IsEnabled)
            {
                MessageBox.Show(Languages.AppResources.listOfNewSurveysPage_DownloadingProgress);
                return;
            }
            BusyIndicator.DataContext = _viewModel.DownloadListStatus.ProgressBar;
            _viewModel.Operations.CheckForNewSurveys();
        }

        /// <summary>
        /// Standard WP7 method which is running always when user presses back key.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (_viewModel.DownloadListStatus.ProgressBar.IsEnabled)
            {
                _viewModel.Operations.Abort();
                e.Cancel = true;
            }
            if (_downloader != null)
            {
                _downloader.Abort();
                _viewModel.DownloadSurveysStatus.IsCanceled = true;
                e.Cancel = true;
                _downloader = null;
            }
            else if (!_viewModel.DownloadSurveysStatus.CanCancel)
            {
                MessageBox.Show(Languages.AppResources.listOfNewSurveysPage_TooLateInterrupt);
                return;
            }
            base.OnBackKeyPress(e);
        }
    }
}