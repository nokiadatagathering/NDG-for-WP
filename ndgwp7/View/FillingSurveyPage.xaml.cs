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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using com.comarch.mobile.ndg.BusyIndicator;
using com.comarch.mobile.ndg.Settings.Model;
using com.comarch.mobile.ndg.ViewModel;
using System.ComponentModel;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// Class contains all methods used during operation on FillingSurveyPage.
    /// </summary>
    public partial class FillingSurveyPage : PhoneApplicationPage
    {
        private bool _savingInProgress;
        private bool _backPreesed;
        private bool _isNewInstance;
        private FillingSurveyViewModel _viewModel;
        private ApplicationBarIconButton _appBarSaveResuktButton;

        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public FillingSurveyPage()
        {
            InitializeComponent();
            ProcessingBar processingBar = new ProcessingBar();
            _isNewInstance = true;
            BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar
            ApplicationBar = new ApplicationBar();

            ApplicationBar.ForegroundColor = (App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color;
            ApplicationBar.BackgroundColor = (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color;

            _appBarSaveResuktButton = new ApplicationBarIconButton(new Uri("/View/icons/SaveResultIcon.png", UriKind.Relative));
            _appBarSaveResuktButton.Text = Languages.AppResources.fillingSurveyPage_OnSaveResultTitle;
            _appBarSaveResuktButton.Click += OnSaveResult;
            _appBarSaveResuktButton.IsEnabled = false;
            ApplicationBar.Buttons.Add(_appBarSaveResuktButton);
        }

        /// <summary>
        /// Standard WP7 method which is running always when user navigates to page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            String strSurveyName = string.Empty;
            String strSurveyId = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("SurveyId", out strSurveyId))
            {
                if (_isNewInstance)
                {
                    _viewModel = new FillingSurveyViewModel(strSurveyId);

                    _viewModel.AddCategoriesToPivot(Categories);

                    _viewModel.Survey.SavingCompletedEventHandler += SavingCompleted;

                    Categories.Title = _viewModel.Survey.Title;
                    BusyIndicator.DataContext = _viewModel.ProgressBar;
                    _viewModel.Survey.ResultChangedEventHandler += (object sender, EventArgs args) => 
                    {
                        _appBarSaveResuktButton.IsEnabled = _viewModel.Survey.IsResultChanged;
                    };

                    if ((Application.Current as App).ApplicationState == App.AplicationStates.Activated)
                    {
                        _viewModel.GetTmpData();
                    }
                    else
                    {
                        String resultId;
                        String resultTitle;
                        if (NavigationContext.QueryString.TryGetValue("resultId", out resultId) && NavigationContext.QueryString.TryGetValue("resultTitle", out resultTitle))
                        {
                            _viewModel.ReadResult(resultId, resultTitle);
                        }                    
                    }
                    _isNewInstance = false;
                }
                (Application.Current as App).ApplicationState = App.AplicationStates.Runing;
            }

        }

        /// <summary>
        /// Standard WP7 method which is running always before user navigates to new page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            String uri = e.Uri.ToString();
            if (!uri.Contains("FilterListPage.xaml") && !uri.Contains("SurveyPage.xaml") && !_viewModel.Survey.TakingPhoto && !uri.Contains("FillingConditionCategoryPage.xaml") && !uri.Contains("DatePickerPage.xaml") && !uri.Contains("TimePickerPage.xaml"))
            {
                _viewModel.SaveTmpData();
            }
        }

        private void OnSaveResult(object sender, EventArgs e)
        {
            CheckIfCanSaveSurveyResultAndSave();
        }

        private void CheckIfCanSaveSurveyResultAndSave()
        {
            _savingInProgress = true;
            if (!_viewModel.Survey.IsResultCorrect)
            {

                if (MessageBox.Show(Languages.AppResources.fillingSurveyPage_isCorrectDialogMessage, "", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                {
                    _savingInProgress = false;
                    _backPreesed = false;
                    return;
                }
            }
            if (String.IsNullOrEmpty(_viewModel.Survey.ResultInfo.Title))
            {
                ShowInputTitileDialog();
            }
            else
            {
                SaveSurveyResult();
            }
        }

        private void SaveSurveyResult()
        {
            _viewModel.ProgressBar.IsEnabled = true;
            _viewModel.ProgressBar.Information = Languages.AppResources.fillingSurveyPage_savingMessage;
            _viewModel.Survey.SaveSurveyResult();
        }

        private void SavingCompleted(object sender, EventArgs args)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _savingInProgress = false;

                if (_backPreesed)
                {
                    _viewModel.ProgressBar.IsEnabled = false;
                    NavigationService.GoBack();
                    return;
                }
                if (OperationsOnSettings.Instance.GPS && !_viewModel.Survey.IsGpsSet)
                {
                    MessageBox.Show(Languages.AppResources.fillingSurveyPage_FailedGPSSave);
                    NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show(Languages.AppResources.fillingSurveyPage_successSaveDialog);
                    NavigationService.GoBack();
                }
                _viewModel.ProgressBar.IsEnabled = false;
            });
        }

        private void ShowInputTitileDialog()
        {
            InputPrompt input = new InputPrompt();
            input.Completed += Title_Completed;

            input.LostFocus += (object sender, RoutedEventArgs e) =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                   {
                       if (ApplicationBar != null)
                       {
                           if (_viewModel.ProgressBar.IsEnabled)
                                ApplicationBar.IsVisible = false;
                       }
                   });
            };
            input.Message = Languages.AppResources.fillingSurveyPage_enterResultTitleDialog;
            input.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            input.InputScope = new InputScope() { Names = { new InputScopeName() { NameValue = InputScopeNameValue.FileName } } };
            input.Value = _viewModel.Survey.GetDefaultResultTitle();
            input.IsCancelVisible = true;
            input.Show();
        }

        private void Title_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            if (!String.IsNullOrEmpty(_viewModel.Survey.ResultInfo.Title))
            {
                // ok button was clicked more than one
                return;
            }
            if (e.PopUpResult == PopUpResult.Ok)
            {
                String title = e.Result;
                if (!String.IsNullOrEmpty(title))
                {
                    _viewModel.Survey.ResultInfo.Title = title;
                    SaveSurveyResult();
                }
                else
                {
                    MessageBox.Show(Languages.AppResources.fillingSurveyPage_saveDialogError);
                    ShowInputTitileDialog();
                }
            }
            else
            {
                _savingInProgress = false;
            }
        }

        /// <summary>
        /// Standard WP7 method which is running always when user presses back key.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (_viewModel.Survey.IsResultChanged)
            {
            _backPreesed = true;
                e.Cancel = true;
                if (!_savingInProgress)
                {
                    _savingInProgress = true;
                    YesNoMessageBox messageBox = new YesNoMessageBox();
                    messageBox.Message = Languages.AppResources.fillingSurveyPage_saveQuestion;
                    messageBox.Completed += (object sender, EventArgs args) =>
                        {
                            if (((YesNoMessageBox)sender).Response == YesNoMessageBox.MessageResponse.Yes)
                            {
                                CheckIfCanSaveSurveyResultAndSave();
                            }
                            else
                            {
                                _savingInProgress = false;
                                NavigationService.GoBack();
                            }
                        };
                    messageBox.Show();
                }
            }
            base.OnBackKeyPress(e);
        }

    }
}