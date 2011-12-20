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
using com.comarch.mobile.ndg.Model.SurveyForms;

namespace com.comarch.mobile.ndg.View.SurveyForms
{
    /// <summary>
    /// Class contains all methods used during operation on ImageQuestionControl.
    /// </summary>
    public partial class ImageQuestionControl : UserControl
    {
        private ImageQuestion _question;
        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public ImageQuestionControl()
        {
            InitializeComponent();
            _question = (ImageQuestion)QuestionData.DataContext;
        }

        private void DisplayCapacityWarning()
        {
            MessageBox.Show(Languages.AppResources.imageQuestionControl_MaxImageCapacity);
        }

        private void OnBrowseImagesButton(object sender, RoutedEventArgs e)
        {
            _question = (ImageQuestion)QuestionData.DataContext;
            if (!_question.IsFull())
            {
                try
                {
                    _question.Parent.Parent.TakingPhoto = true;
                    _question.ChooserTask.Show();
                }
                catch (InvalidOperationException) //catching double clicks
                {
                }
            }
            else
            {
                DisplayCapacityWarning();
            }
        }

        private void OnTakePhotoButton(object sender, RoutedEventArgs e)
        {
            _question = (ImageQuestion)QuestionData.DataContext;
            if (!_question.IsFull())
            {
                try
                {
                    _question.Parent.Parent.TakingPhoto = true;
                    _question.CameraTask.Show();
                }
                catch (InvalidOperationException) //catching double clicks
                {
                }
            }
            else
            {
                DisplayCapacityWarning();
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            _question = (ImageQuestion)QuestionData.DataContext;
            Button button = sender as Button;
            _question.DeleteImage((ImageQuestion.ImageItem)button.DataContext);
        }
    }
}
