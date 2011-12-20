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
using Coding4Fun.Phone.Controls;
using System.Windows.Media;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// ShowDownloadCancelMessageBox is a customize 3-options (Show/Download/Cancel) DialogBox from Coding4Fun control.
    /// </summary>
    public class ShowDownloadCancelMessageBox
    {
        /// <summary>
        /// Predefine choice option for DialogBox.
        /// </summary>
        public enum MessageResponse
        {
            /// <summary>
            /// Show list of new surveys.
            /// </summary>
            Show,
            /// <summary>
            /// Download new surveys.
            /// </summary>
            Download,
            /// <summary>
            /// Ignore information about new survey availability.
            /// </summary>
            Cancel
        }

        /// <summary>
        /// Return enum value selected by user.
        /// </summary>
        public MessageResponse Response { get; set; }

        MessagePrompt prompt = new MessagePrompt();

        /// <summary>
        /// Constructor for ShowDownloadCancelMessageBox.
        /// <para>Create new DialogBox object with 3 buttons: Show, Download, Cancel. And set default response on Cancel.</para>
        /// </summary>
        public ShowDownloadCancelMessageBox()
        {
            prompt.Completed += message_Completed;
            Button showButton = new Button() { Content = Languages.AppResources.messageBox_Show };
            showButton.Click += OnShow;
            Button downloadButton = new Button() { Content = Languages.AppResources.messageBox_Download };
            downloadButton.Click += OnDownload;
            Button cancelButton = new Button() { Content = Languages.AppResources.messageBox_Cancel };
            cancelButton.Click += OnCancel;

            prompt.ActionPopUpButtons.Clear();
            prompt.ActionPopUpButtons.Add(showButton);
            prompt.ActionPopUpButtons.Add(downloadButton);
            prompt.ActionPopUpButtons.Add(cancelButton);

            prompt.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;

            Response = MessageResponse.Cancel;
        }

        void OnShow(object sender, RoutedEventArgs e)
        {
            Response = MessageResponse.Show;
            prompt.Hide();
        }

        void OnDownload(object sender, RoutedEventArgs e)
        {
            Response = MessageResponse.Download;
            prompt.Hide();
        }

        void OnCancel(object sender, RoutedEventArgs e)
        {
            Response = MessageResponse.Cancel;
            prompt.Hide();
        }

        /// <summary>
        /// Set DialogBox message.
        /// </summary>
        public String Message
        {
            set
            {
                prompt.Message = value;
            }
        }

        /// <summary>
        /// Set DialogBox title.
        /// </summary>
        public String Title
        {
            set
            {
                prompt.Title = value;
            }
        }

        /// <summary>
        /// Method to display ShowDownloadCancelMessageBox.
        /// </summary>
        public void Show()
        {
            prompt.Show();
        }
        
        private void message_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            e.PopUpResult = PopUpResult.NoResponse;
            if (Completed != null)
            {
                Completed(this, e);
            }
        }

        /// <summary>
        /// Event which notify when DialogBox is completed (One on the option was selected by user).
        /// </summary>
        public EventHandler Completed;
    }
}
