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
using System.Windows.Controls;
using com.comarch.mobile.ndg.Model.SurveyForms;

namespace com.comarch.mobile.ndg.View.SurveyForms
{
    /// <summary>
    /// Class contains all methods used during operation on DescriptiveQuestionControl.
    /// </summary>
    public partial class DescriptiveQuestionControl : UserControl
    {
        private DescriptiveQuestion _question;
        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public DescriptiveQuestionControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _question = (DescriptiveQuestion)QuestionData.DataContext;
            _question.AssignValidationRule();
        }
    }
}
