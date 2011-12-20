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
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using com.comarch.mobile.ndg.MessageDialog;
using com.comarch.mobile.ndg.Settings.Model;
using com.comarch.mobile.ndg.Validation;
using com.comarch.mobile.ndg.ViewModel;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// Class contains all methods used during operation on RegistrationPage.
    /// </summary>
    public partial class RegistrationPage : PhoneApplicationPage
    {
        private RegistrationViewModel _registration;

        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public RegistrationPage()
        {
            InitializeComponent();

            LengthValidationRule rule = new LengthValidationRule(5, 15)
            {
                MaxErrorMessage = Languages.AppResources.operationsOnRegistration_ContainMaximum,
                MinErrorMessage = Languages.AppResources.operationsOnRegistration_ContainMinimum
            };
            ImeiTextBox.ValidationRule = rule;
            PhoneNumberTextBox.ValidationRule = rule;

            string urlPattern = "^(http(s?)\\:\\/\\/)*[0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*(:(0-9)*)*(\\/?)([a-zA-Z0-9\\-\\.\\?\\,\\'\\/\\\\+&amp;%\\$#_]*)?$";
            ServerUrlTextBox.ValidationRule = new RegexValidationRule(urlPattern)
            {
                Message = Languages.AppResources.operationsOnRegistration_ValidServerAddress
            };

            _registration = new RegistrationViewModel();
            
            _registration.Operator.LeaveRegistrationEvent += new EventHandler(LeaveRegistrationCallback);

            MessageView.AssignDisplay(_registration.Operator.ConnectionResult);

            RegistrationDisplay.DataContext = _registration.Operator.RegistrationInstance;
            BusyIndicator.DataContext = _registration.Operator.Busy;
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
            if (OperationsOnSettings.Instance.IsRegistered())
            {
                NavigationService.GoBack();
            }

            base.OnNavigatedTo(e);
            InputScopeNameValue PhoneKeyboard = InputScopeNameValue.TelephoneNumber;
            InputScopeNameValue UrlKeyboard = InputScopeNameValue.Url;

            ImeiTextBox.InputScope = new InputScope()
            {
                Names = { new InputScopeName() { NameValue = PhoneKeyboard } }
            };

            PhoneNumberTextBox.InputScope = new InputScope()
            {
                Names = { new InputScopeName() { NameValue = PhoneKeyboard } }
            };

            ServerUrlTextBox.InputScope = new InputScope()
            {
                Names = { new InputScopeName() { NameValue = UrlKeyboard } }
            };
        }

        private void LeaveRegistrationCallback(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
        
        private void OnRegisterButton(object sender, RoutedEventArgs e)
        {
            _registration.RunRegistrationProcess();
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            e.Cancel = true; // ignores navigation to previous page
            App.Quit();
        }
    }
}