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
using com.comarch.mobile.ndg.BusyIndicator;
using com.comarch.mobile.ndg.Model.SurveyForms;
using com.comarch.mobile.ndg.View;
using com.comarch.mobile.ndg.View.SurveyForms;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Class stores methods used during communication between model and view on PreviewSurveyPage.
    /// </summary>
    public class PreviewSurveyViewModel
    {
        /// <summary>
        /// Represents instance of <see cref="ProcessingBar"/> class.
        /// </summary>
        public ProcessingBar ProgressBar { get; set; }

        /// <summary>
        /// Represents instance of <see cref="Survey"/> class.
        /// </summary>
        public Survey Survey { get; private set; }
        /// <summary>
        /// Constructor which initializes Survey property and displays idSurvey survey.
        /// </summary>
        /// <param name="idSurvey">Survey ID which will be displayed in preview.</param>
        public PreviewSurveyViewModel(string idSurvey)
        {
            Survey = new Survey();
            Survey.Display(Convert.ToInt32(idSurvey));
            ProgressBar = new ProcessingBar();
        }

        /// <summary>
        /// Adds all questions in category to pivot page. Each caterogy will be in seperate pivot.
        /// </summary>
        /// <param name="pivot">Pivot name to which categories should be added.</param>
        public void AddCategoriesToPivot(Pivot pivot)
        {
            foreach (Category cat in Survey.Categories)
            {
                if (cat is NormalCategory)
                {
                    CategoryControl categoryControl = new CategoryControl();
                    PreviewCategoryViewModel normalCategoryViewModel = new PreviewCategoryViewModel(((NormalCategory)cat));
                    normalCategoryViewModel.AddQuestionsToListBox(categoryControl.QuestionsList);

                    PivotItem item = new PivotItem();
                    item.Header = ((NormalCategory)cat).Name;
                    item.Content = categoryControl;
                    pivot.Items.Add(item);
                }
                else if (cat is ConditionCategory)
                {
                    ConditionCategory category = (ConditionCategory)cat;
                    PreviewConditionCategoryPage categoryControl = new PreviewConditionCategoryPage();

                    categoryControl.DataContext = category;

                    PivotItem item = new PivotItem();
                    item.Header = category.Name;
                    item.Content = categoryControl;
                    pivot.Items.Add(item);
                }
            }
            Survey.RefreshQuestionsVisibility();
        }

        /// <summary>
        /// Reads answer for selected result from xml file saved in IsolatedStorage.
        /// </summary>
        /// <param name="resultId">ID of selected result.</param>
        /// <param name="resultTitle">Title of result.</param>
        public void ReadResult(String resultId, String resultTitle)
        {
            ProgressBar.IsEnabled = true;
            ProgressBar.Information = Languages.AppResources.fillingSurveyPage_opaningMessage;
            Survey.ResultInfo.Title = resultTitle;
            Survey.ReadSurveyResult(resultId);
            Survey.ReadingCompletedEventHandler += (object sender, EventArgs args) =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ProgressBar.IsEnabled = false;
                });
            };
        }
    }
}