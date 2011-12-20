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
using Microsoft.Phone.Controls;
using com.comarch.mobile.ndg.Model.SurveyForms;
using com.comarch.mobile.ndg.ViewModel;
using System.ComponentModel;
using System.Windows.Navigation;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// Class contains all methods used during operation on FillingConditionCategoryPage.
    /// </summary>
    public partial class FillingConditionCategoryPage : PhoneApplicationPage
    {
        private bool _isNewInstance;
        private bool _backPreesed;
        private NormalCategory _category;

        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public FillingConditionCategoryPage()
        {
            InitializeComponent();
            _isNewInstance = true;
        }

        /// <summary>
        /// Standard WP7 method which is running always when user navigates to page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isNewInstance)
            {
                String categoryName = String.Empty;
                if (NavigationContext.QueryString.TryGetValue("CategoryName", out categoryName))
                {
                    _category = App.AppDictionary["SentCategory"] as NormalCategory;
                    if (_category != null)
                    {
                        if (!_category.Visited)
                            _category.Visited = true;
                        SubSategoryName.Text = _category.Name;
                        CategoryName.Text = categoryName;
                        NormalCategoryViewModel normalCategoryViewModel = new NormalCategoryViewModel(_category);
                        normalCategoryViewModel.AddQuestionsToListBox(QuestionsList);
                    }
                    else
                    {
                        _backPreesed = true;
                        NavigationService.GoBack();
                    }
                }
                _isNewInstance= false;
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
            if (!_backPreesed && !_category.Parent.TakingPhoto && !uri.Contains("FillingConditionCategoryPage.xaml") && !uri.Contains("DatePickerPage.xaml") && !uri.Contains("TimePickerPage.xaml"))
            {
                _category.Parent.SaveTmpData();
            }
            _backPreesed = false;
        }

        private void OnBackPressed(object sender, CancelEventArgs e)
        {
            _backPreesed = true;
        }

    }
}