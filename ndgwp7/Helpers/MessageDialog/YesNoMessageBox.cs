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
    /// YesNoMessageBox is a customize 2-options (Yes/No) DialogBox from Coding4Fun control.
    /// </summary>
    public class YesNoMessageBox
    {
        /// <summary>
        /// Predefine choice option for DialogBox.
        /// </summary>
        public enum MessageResponse
        {
            /// <summary>
            /// 'Yes' resposnse.
            /// </summary>
            Yes,
            /// <summary>
            /// 'No' resposnse.
            /// </summary>
            No,
            /// <summary>
            /// None option was selected.
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
        /// <para>Create new DialogBox object with 2 buttons: Yes, No. And set default response on Cancel.</para>
        /// </summary>
        public YesNoMessageBox()
        {
            prompt.Completed += message_Completed;
            Button yesButton = new Button() { Content = Languages.AppResources.messageBox_Yes };
            yesButton.Click += OnYes;
            Button noButton = new Button() { Content = Languages.AppResources.messageBox_No };
            noButton.Click += OnNo;
            prompt.ActionPopUpButtons.Clear();
            prompt.ActionPopUpButtons.Add(yesButton);
            prompt.ActionPopUpButtons.Add(noButton);
            prompt.Background = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            Response = MessageResponse.Cancel;
        }
        void OnYes(object sender, RoutedEventArgs e)
        {
            Response = MessageResponse.Yes;
            prompt.Hide();
        }
        void OnNo(object sender, RoutedEventArgs e)
        {
            Response = MessageResponse.No;
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
        /// Method to display YesNoMessageBox.
        /// </summary>
        public void Show()
        {
            prompt.Show();
        }

        void message_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
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
