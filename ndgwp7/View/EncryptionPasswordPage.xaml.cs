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
using System.Windows;
using Microsoft.Phone.Controls;
using com.comarch.mobile.ndg.Model.SurveyForms;
using com.comarch.mobile.ndg.Validation;
using com.comarch.mobile.ndg.ViewModel;
using System.ComponentModel;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// Class contains all methods used during operation on EncryptionPasswordPage.
    /// </summary>
    public partial class EncryptionPasswordPage : PhoneApplicationPage
    {
        EncryptionPasswordPageViewModel _viewModel = new EncryptionPasswordPageViewModel();

        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public EncryptionPasswordPage()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            if (ValidatePassword())
            {
                if (_viewModel.SavePassword())
                {
                    NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show(Languages.AppResources.encryptionPasswordPage_error);
                }
            }
        }

        /// <summary>
        /// Standard WP7 method which is running always when user presses back key.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            App.Quit();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            ValidatePassword();
        }
        private bool ValidatePassword()
        {
            RangeValidationRule rule = new RangeValidationRule(NumericQuestion.Types.IntegerType, null, null);
            rule.RegexRule = new RegexValidationRule("");
            string newValue = Password.Text;
            Password.IsValid = true;
            Password.IsValid = rule.Validate(newValue);
            if (!Password.IsValid)
            {
                Password.ValidationContent = Languages.AppResources.validationRules_LengthEmptyMessage;
                OKButton.IsEnabled = false;
                return false;
            }
            else
            {
                OKButton.IsEnabled = true;
                return true;
            }
        }

    }
}