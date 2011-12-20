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
using System.Windows.Controls;
using com.comarch.mobile.ndg.Model.SurveyForms;
using com.comarch.mobile.ndg.View.SurveyForms;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Class stores methods used during communication between model and view on PreviewCategory.
    /// </summary>
    public class PreviewCategoryViewModel
    {
        private NormalCategory _category;
        /// <summary>
        /// Constructor which sets private _category field on input value.
        /// </summary>
        /// <param name="category">Value contains information about category which will be added to pivot.</param>
        public PreviewCategoryViewModel(NormalCategory category)
        {
            _category = category;
        }

        /// <summary>
        /// Adds all questions in category to ListBox on pivot page.
        /// </summary>
        /// <param name="listOfQuestions">ListBox element name on pivot page to which questions should be added.</param>
        public void AddQuestionsToListBox(ListBox listOfQuestions)
        {
            foreach (Question question in _category.Questions)
            {
                Type type = question.GetType();
                
                UserControl questionControl = new PreviewResultControl();
                ((PreviewResultControl)questionControl).QuestionData.DataContext = question;

                if (questionControl != null)
                    listOfQuestions.Items.Add(questionControl);
            }
        }
    }
}
