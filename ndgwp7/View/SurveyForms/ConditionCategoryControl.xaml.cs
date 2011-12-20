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
using Microsoft.Phone.Controls;
using com.comarch.mobile.ndg.MessageDialog;
using com.comarch.mobile.ndg.Model.SurveyForms;
using com.comarch.mobile.ndg.Validation;
using com.comarch.mobile.ndg.ViewModel;

namespace com.comarch.mobile.ndg.View.SurveyForms
{
    /// <summary>
    /// Class contains all methods used during operation on ConditionCategoryControl.
    /// </summary>
    public partial class ConditionCategoryControl : UserControl
    {
        private bool _isNewInstance;
        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public ConditionCategoryControl()
        {
            InitializeComponent();
            _isNewInstance = true;
        }

        private void OnClickOk(object sender, RoutedEventArgs e)
        {
            int howMany=0;
            if (!NumbersOfNewCategories.IsValid)
            {
                return;
            }
            if (NumbersOfNewCategories.Text == "")
            {
                NumbersOfNewCategories.IsValid = false;
            }

            try
            {
                howMany = Convert.ToInt32(NumbersOfNewCategories.Text);
            }
            catch (FormatException)
            {
                NumbersOfNewCategories.Text = String.Empty;
                return;
            }
            ConditionCategoryControlViewModel data = (ConditionCategoryControlViewModel)DataContext;
            if (data != null)
            {
                data.ReorganizeSubcategories(howMany);
            }
        }

        private void OnSelectedCategory(object sender, SelectionChangedEventArgs e)
        {
            int selected = SubCategories.SelectedIndex;
            ConditionCategory conditionCategory = ((ConditionCategoryControlViewModel)DataContext).Category;
            if (conditionCategory != null && selected != -1 )
            {
                if (!App.AppDictionary.ContainsKey("SentCategory"))
                    App.AppDictionary.Add("SentCategory", conditionCategory.SubCategories[selected]);
                else
                    App.AppDictionary["SentCategory"] = conditionCategory.SubCategories[selected];

                (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri(String.Format("/View/FillingConditionCategoryPage.xaml?CategoryName={0}", conditionCategory.Name), UriKind.Relative));
                SubCategories.SelectedIndex = -1;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isNewInstance)
            {
                NumbersOfNewCategories.ValidationRule = new RegexValidationRule("^\\d+$");
                ConditionCategoryControlViewModel dataContext = (ConditionCategoryControlViewModel)DataContext;
                MessageView.AssignYesNoMessage(dataContext.MessageBox);
            }
            _isNewInstance = false;
        }


    }
}
