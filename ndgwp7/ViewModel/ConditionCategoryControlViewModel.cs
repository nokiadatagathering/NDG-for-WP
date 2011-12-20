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
using com.comarch.mobile.ndg.MessageDialog;
using com.comarch.mobile.ndg.Model.SurveyForms;
using com.comarch.mobile.ndg.View;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Class stores methods used during communication between model and view on ConditionCategoryControl.
    /// </summary>
    public class ConditionCategoryControlViewModel
    {
        /// <summary>
        /// Represents instance of <see cref="DialogBox"/> class.
        /// </summary>
        public DialogBox MessageBox { get; private set; }
        private int _howMany;
        private bool oldIsResultChangedValue;

        /// <summary>
        /// Constructor which initializes Category property using input value.
        /// </summary>
        /// <param name="category">Contains information about condition category.</param>
        public ConditionCategoryControlViewModel(ConditionCategory category)
        {
            Category = category;
            MessageBox = new DialogBox();
        }
        /// <summary>
        /// Represents instance of <see cref="ConditionCategory"/> class.
        /// </summary>
        public ConditionCategory Category { get; private set; }
        /// <summary>
        /// Method to reorganize subcatories if answer on condiction question was changed.
        /// </summary>
        /// <param name="howMany">Answer on condition question.</param>
        public void ReorganizeSubcategories(int howMany)
        {
            _howMany = howMany;
            oldIsResultChangedValue = Category.Parent.IsResultChanged;
            if (Category.SubCategories.Count < howMany)
            {
                Category.Parent.IsResultChanged = false;
                MessageBox.YesNoAnswerCompleted += AddSubCategories;
                MessageBox.ShowYesNoQuestion(Languages.AppResources.conditionCategoryControl_MessageBoxTitle,Languages.AppResources.conditionCategoryControl_MessageBoxMessage);
            }
            else if (Category.SubCategories.Count > howMany)
            {
                Category.Parent.IsResultChanged = false;
                MessageBox.YesNoAnswerCompleted += RemoveSubCategories;
                MessageBox.ShowYesNoQuestion(Languages.AppResources.conditionCategoryControl_RemoveMessageBoxTitle, Languages.AppResources.conditionCategoryControl_RemoveMessageBoxMessage);
            }
        }
        private void RemoveSubCategories(object sender, EventArgs args) 
        {
            if (MessageBox.YesNoResponse == YesNoMessageBox.MessageResponse.Yes)
            {
                Category.RemoveLastSubcategories(Category.SubCategories.Count - _howMany);
            }
            MessageBox.YesNoAnswerCompleted -= RemoveSubCategories;
            if (!Category.Parent.IsResultChanged)
            {
                Category.Parent.IsResultChanged = oldIsResultChangedValue;
            }
        }
        private void AddSubCategories(object sender, EventArgs args)
        {
            if (MessageBox.YesNoResponse == YesNoMessageBox.MessageResponse.Yes)
            {
                Category.MakeSubCategories(Category.SubCategories.Count, _howMany);
            }
            MessageBox.YesNoAnswerCompleted -= AddSubCategories;
            if (!Category.Parent.IsResultChanged)
            {
                Category.Parent.IsResultChanged = oldIsResultChangedValue;
            }
        }
    }
}
