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
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using com.comarch.mobile.ndg.Model;
using com.comarch.mobile.ndg.ViewModel;
using System.ComponentModel;
using Microsoft.Phone.Shell;
using System.Windows.Media;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// Class contains all methods used during operation on FilterListPage.
    /// </summary>
    public partial class FilterListPage : PhoneApplicationPage
    {
        private ResultsFilterViewModel _filterViewModel;

        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public FilterListPage()
        {
            _filterViewModel = new ResultsFilterViewModel();
            InitializeComponent();
            BusyIndicator.DataContext = _filterViewModel.ProgressBar;
            ListOfSurveys.ItemsSource = App.AppDictionary["FilteredResults"] as ObservableCollection<ResultsFilter.GroupedOC<ResultBasicInfo>>;
            _filterViewModel.ResultSender.SendingCompleted += (object sender, EventArgs args) =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    SendResult.SendingEventArgs.SendingStatus status = (args as SendResult.SendingEventArgs).Status;
                    string resultId = (args as SendResult.SendingEventArgs).ResultId;
                    switch (status)
                    {
                        case Model.SendResult.SendingEventArgs.SendingStatus.Sent:
                            MessageBox.Show(Languages.AppResources.surveyViewModel_sendingCompleted);
                            _filterViewModel.Operations.MarkResultAsSent(resultId);
                            _filterViewModel.Filter.ReloadResult(resultId);                                           
                            break;
                        case Model.SendResult.SendingEventArgs.SendingStatus.ServerError:
                            MessageBox.Show(Languages.AppResources.surveyViewModel_serverError);
                            break;
                        case Model.SendResult.SendingEventArgs.SendingStatus.UnknownError:
                            MessageBox.Show(Languages.AppResources.surveyViewModel_unknownError);
                            break;
                        case Model.SendResult.SendingEventArgs.SendingStatus.Canceled:
                            break;
                    }
                    _filterViewModel.ProgressBar.IsEnabled = false;
                    _filterViewModel.IsSending = false;
                });
            };
            BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar
            ApplicationBar = new ApplicationBar();

            ApplicationBar.ForegroundColor = (App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color;
            ApplicationBar.BackgroundColor = (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color;

            ApplicationBarIconButton appBarSettingsShowAll = new ApplicationBarIconButton(new Uri("/View/icons/MapShowAll.png", UriKind.Relative));
            appBarSettingsShowAll.Text = Languages.AppResources.filterListPage_ShowAllOnMap;
            appBarSettingsShowAll.Click += OnShowAllOnMap;
            ApplicationBar.Buttons.Add(appBarSettingsShowAll);
        }

        private ResultBasicInfo _lastSender;

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
            if (_lastSender != null)
            {
                _filterViewModel.Filter.ReloadResult(_lastSender.Id);
                _lastSender = null;
            }
            if (App.AppDictionary["FilteredResults"] != null)
            {
                if ((App.AppDictionary["FilteredResults"] as ObservableCollection<ResultsFilter.GroupedOC<ResultBasicInfo>>).Count == 0)
                {
                    Status.Visibility = Visibility.Visible;
                }
                else
                {
                    Status.Visibility = Visibility.Collapsed;
                }
            }
            base.OnNavigatedTo(e);
        }

        private void OnShowResult(object sender, RoutedEventArgs e)
        {
            ResultBasicInfo selectedResult = (sender as MenuItem).DataContext as ResultBasicInfo;
            NavigationService.Navigate(new Uri(string.Format("/View/PreviewSurveyPage.xaml?SurveyId={0}&resultId={1}&resultTitle={2}", selectedResult.ParentId, selectedResult.Id, selectedResult.Title), UriKind.Relative));
        }

        private void OnEditExistingResult(object sender, RoutedEventArgs e)
        {
            ResultBasicInfo selectedResult = (sender as MenuItem).DataContext as ResultBasicInfo;
            _lastSender = selectedResult;
            NavigationService.Navigate(new Uri(string.Format("/View/FillingSurveyPage.xaml?SurveyId={0}&resultId={1}&resultTitle={2}", selectedResult.ParentId, selectedResult.Id, selectedResult.Title), UriKind.Relative));
        }

        /// <summary>
        /// Standard WP7 method which is running always before user navigates to new page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (_filterViewModel.IsSending)
            {
                e.Cancel = true;
                _filterViewModel.AbortSending();
            }
            base.OnBackKeyPress(e);
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
                        ResultBasicInfo selectedResult = (sender as MenuItem).DataContext as ResultBasicInfo;
                        _filterViewModel.Filter.DeleteResult(selectedResult);
                    }
                    catch (IsolatedStorageException)
                    {
                        MessageBox.Show(Languages.AppResources.surveyPage_deleteResultError);
                    }
                }
            };
            messageBox.Show();
        }

        private void OnSendResult(object sender, RoutedEventArgs e)
        {
            ResultBasicInfo selectedListBoxItem = (sender as MenuItem).DataContext as ResultBasicInfo;
            _filterViewModel.SendResult(selectedListBoxItem);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            LayoutRoot.IsHitTestVisible = false;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            LayoutRoot.IsHitTestVisible = true;
        }

        private void OnShowOnMap(object sender, RoutedEventArgs e)
        {
            ResultsFilter.GroupedOC<ResultBasicInfo> selectedResult = (sender as MenuItem).DataContext as ResultsFilter.GroupedOC<ResultBasicInfo>;

            List<GPSEntity> pins = new List<GPSEntity>();

            foreach(var selected in selectedResult)
            {
                if (selected.Latitude != null && selected.Longitude != null)
                    pins.Add(new GPSEntity() { Location = new GeoCoordinate(Convert.ToDouble(selected.Latitude), Convert.ToDouble(selected.Longitude)), Title = selected.Title});
            }

            UseBingMapToShow(pins);
        }

        private void OnShowAllOnMap(object sender, EventArgs e)
        {
            ObservableCollection<ResultsFilter.GroupedOC<ResultBasicInfo>> allResults = App.AppDictionary["FilteredResults"] as ObservableCollection<ResultsFilter.GroupedOC<ResultBasicInfo>>;
            List<GPSEntity> pins = new List<GPSEntity>();

            foreach (ResultsFilter.GroupedOC<ResultBasicInfo> selected in allResults)
            {
                foreach (var result in selected)
                {
                    if (result.Latitude != null && result.Longitude != null)
                        pins.Add(new GPSEntity() { Location = new GeoCoordinate(Convert.ToDouble(result.Latitude), Convert.ToDouble(result.Longitude)), Title = result.Title });
                }
            }

            UseBingMapToShow(pins);
        }

        private void UseBingMapToShow(List<GPSEntity> pins)
        {
            if (pins.Count > 0)
            {
                if (App.AppDictionary.ContainsKey("Pushpins"))
                    App.AppDictionary["Pushpins"] = pins;
                else
                    App.AppDictionary.Add("Pushpins", pins);

                NavigationService.Navigate(new Uri("/View/BingMapPage.xaml?showPushpins=true", UriKind.Relative));
            }
            else
                MessageBox.Show(Languages.AppResources.resultsFilter_NoneSavedGPSPosition);
        }

        private void OnGoToSurveyPage(object sender, RoutedEventArgs e)
        {
            ResultsFilter.GroupedOC<ResultBasicInfo> selectedSurvey = (sender as MenuItem).DataContext as ResultsFilter.GroupedOC<ResultBasicInfo>;
            NavigationService.Navigate(new Uri(string.Format("/View/SurveyPage.xaml?SurveyName={0}&SurveyId={1}&isFavorite={2}", selectedSurvey.Title, selectedSurvey.Id, selectedSurvey.Favorite), UriKind.Relative));
        }

        private void OnBackToEditClick(object sender, RoutedEventArgs e)
        {
            ResultBasicInfo selectedListBoxItem = (sender as MenuItem).DataContext as ResultBasicInfo;
            _filterViewModel.UnmarkSent(selectedListBoxItem);
            _filterViewModel.Filter.ReloadResult(selectedListBoxItem.Id); 
        }
    }
}