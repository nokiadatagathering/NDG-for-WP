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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using com.comarch.mobile.ndg.Model;
using com.comarch.mobile.ndg.Validation;
using com.comarch.mobile.ndg.ViewModel;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// Class contains all methods used during operation on ResultsFilterPage.
    /// </summary>
    public partial class ResultsFilterPage : PhoneApplicationPage
    {
        private ListOfSurveysViewModel _surveysViewModel;
        private ResultsFilterViewModel _filterViewModel;

        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public ResultsFilterPage()
        {
            InitializeComponent();
            _surveysViewModel = new ListOfSurveysViewModel();
            _surveysViewModel.Read();
            _filterViewModel = new ResultsFilterViewModel(_surveysViewModel.List);
            Filters.DataContext = _filterViewModel.Filter;
            _filterViewModel.Filter.DateFiltration.ChosenDateFilter = "0";
            SurveySearchList.DataContext = _filterViewModel.Filter.SurveyFiltration.SelectedSurveys;
            BusyIndicator.DataContext = _filterViewModel.Filter.Busy;
            _filterViewModel.Filter.SearchingCompletedEventHandler += OnSearchingCompleted;
            RadiusTextBox.ValidationRule = new RangeValidationRule(Model.SurveyForms.NumericQuestion.Types.DecimalType, 0, null);
            BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar
            ApplicationBar = new ApplicationBar();

            ApplicationBar.ForegroundColor = (App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color;
            ApplicationBar.BackgroundColor = (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color;

            // Create a new button and set the text value to the localized string from AppResources
            ApplicationBarIconButton appBarChooseFilterButton = new ApplicationBarIconButton(new Uri("/View/icons/FilterResultsIcon.png", UriKind.Relative));
            appBarChooseFilterButton.Text = Languages.AppResources.resultsFilterPageAppBar_Apply;
            appBarChooseFilterButton.Click += OnChooseFilter;
            ApplicationBar.Buttons.Add(appBarChooseFilterButton);
        }

        private void OnAddButton(object sender, RoutedEventArgs e)
        {
            _filterViewModel.Filter.SurveyFiltration.AddChosenSurvey(_surveysViewModel.List);
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
            _filterViewModel.Filter.LocationFiltration.Latitude = string.IsNullOrEmpty(App.AppDictionary["Latitude"] as String) ? string.Empty : App.AppDictionary["Latitude"] as String;
            _filterViewModel.Filter.LocationFiltration.Longitude = string.IsNullOrEmpty(App.AppDictionary["Longitude"] as String) ? string.Empty : App.AppDictionary["Longitude"] as String;
            _filterViewModel.Filter.LocationFiltration.Radius = string.IsNullOrEmpty(App.AppDictionary["Radius"] as String) ? string.Empty : App.AppDictionary["Radius"] as String;
            RadiusTextBox.IsValid = true;

            _filterViewModel.Filter.SurveyFiltration.UpdateSurveyPicker(_surveysViewModel.List);
            SurveysFilterListPicker.SelectedIndex = _surveysViewModel.List.Count;
            ApplicationBar.IsVisible = true;
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Standard WP7 method which is running always before user navigates to new page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _filterViewModel.Filter.Busy.IsEnabled = false;
            if (App.AppDictionary.ContainsKey("Latitude"))
                App.AppDictionary["Latitude"] = _filterViewModel.Filter.LocationFiltration.Latitude;
            else
                App.AppDictionary.Add("Latitude", _filterViewModel.Filter.LocationFiltration.Latitude);

            if (App.AppDictionary.ContainsKey("Longitude"))
                App.AppDictionary["Longitude"] = _filterViewModel.Filter.LocationFiltration.Longitude;
            else
                App.AppDictionary.Add("Longitude", _filterViewModel.Filter.LocationFiltration.Longitude);

            if (App.AppDictionary.ContainsKey("Radius"))
                App.AppDictionary["Radius"] = _filterViewModel.Filter.LocationFiltration.Radius;
            else
                App.AppDictionary.Add("Radius", _filterViewModel.Filter.LocationFiltration.Radius);
                                                             
            base.OnNavigatedFrom(e);
        }

        private void OnSearchingCompleted(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/FilterListPage.xaml", UriKind.Relative));
        }

        private void OnChooseFilter(object sender, EventArgs e)
        {
            if (_filterViewModel.Filter.LocationFiltration.ValuesAreCorrect())
            {
                _filterViewModel.Filter.Display(_surveysViewModel.List);
            }
        }

        private void OnDateFilterListPickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var picker = sender as ListPicker;
            if (picker.SelectedIndex == 3)
            {
                BetweenDateOptionGrid.Visibility = Visibility.Visible;
            }
            else
            {
                BetweenDateOptionGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void OnRemoveSelectedButtonClick(object sender, RoutedEventArgs e)
        {
            if (SurveySearchList.SelectedIndex != -1)
            {
                SurveyBasicInfo selectedListBoxItem = SurveySearchList.SelectedItem as SurveyBasicInfo;
                _filterViewModel.Filter.SurveyFiltration.RemoveSelectedSurvey(selectedListBoxItem);
            }
        }

        private void OnBingMapButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/BingMapPage.xaml", UriKind.Relative));
        }

        private void RadiusTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            string[] radius = RadiusTextBox.Text.Split(' ');
            RadiusTextBox.Text = radius[0];
        }
    }
}