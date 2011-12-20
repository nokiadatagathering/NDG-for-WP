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
using Microsoft.Phone.Controls;
using com.comarch.mobile.ndg.Model.SurveyForms;
using com.comarch.mobile.ndg.ViewModel;

namespace com.comarch.mobile.ndg.View.SurveyForms
{
    /// <summary>
    /// Class contains all methods used during operation on CategoryPivotItem.
    /// </summary>
    public class CategoryPivotItem : PivotItem
    {
        private Category _category;
        /// <summary>
        /// Constuctor which initializes component on page (each category on seperate page).
        /// </summary>
        /// <param name="category">Contains information about category (e.g. category ID, suvery ID, etc.)</param>
        public CategoryPivotItem(Category category)
        {
            _category = category;
            if (_category is NormalCategory)
            {
                CategoryControl categoryControl = new CategoryControl();
                NormalCategoryViewModel normalCategoryViewModel = new NormalCategoryViewModel(((NormalCategory)_category));
                normalCategoryViewModel.AddQuestionsToListBox(categoryControl.QuestionsList);

                Header = ((NormalCategory)_category).Name;
                Content = categoryControl;
            }
            else if (_category is ConditionCategory)
            {
                ConditionCategoryControl categoryControl = new ConditionCategoryControl();

                categoryControl.DataContext = new ConditionCategoryControlViewModel((ConditionCategory)_category);

                Header = category.Name;
                Content = categoryControl;
            }
        }

        /// <summary>
        /// Method to mark category as visited when user changes pivot page.
        /// </summary>
        public void SetAsVisited()
        {
            if (!_category.Visited)
                _category.Visited = true;
        }
    }
}
