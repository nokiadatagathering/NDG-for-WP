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
using com.comarch.mobile.ndg.View;
using System.Threading;

namespace com.comarch.mobile.ndg.MessageDialog
{
    /// <summary>
    /// Class contains all methods used during display MessageBox information.
    /// </summary>
    public class DialogBox
    {
        /// <summary>
        /// Title on DialogBox.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Message information field on DialogBox.
        /// </summary>
        public string Field { get; private set; }

        /// <summary>
        /// Event which notify when DialogBox is completed (One of the option was selected by user).
        /// </summary>
        public EventHandler YesNoAnswerCompleted;

        /// <summary>
        /// Response value (Yes or No).
        /// </summary>
        public YesNoMessageBox.MessageResponse YesNoResponse { get; set; }

        /// <summary>
        /// Contain type of button for standard MessageBox (OK or OK/Cancel).
        /// </summary>
        public MessageBoxButton ButtonType { get; private set; }

        /// <summary>
        /// Contain responce value after user make a decision.
        /// </summary>
        public MessageBoxResult Result { private get; set; }

        /// <summary>
        /// Event which notify when DialogBox was changing.
        /// </summary>
        public event EventHandler DialogBoxEvent;

        /// <summary>
        /// Event which notify when YesNoDialogBox was changing.
        /// </summary>
        public event EventHandler YesNoMessageEvent;

        /// <summary>
        /// Method to display standard (notification) MessageBox containing only message information field (without title) and ‘OK’ button.
        /// </summary>
        /// <param name="message">Message notification field.</param>
        /// <returns>Return MessageBoxResponse.OK when user press 'OK' button.</returns>
        public MessageBoxResult Show(string message)
        {
            return Show(null, message, MessageBoxButton.OK);
        }

        /// <summary>
        /// Method to display standard MessageBox containing message information field, message title and one of standard MessageBox button type (OK or OK/Cancel).
        /// </summary>
        /// <param name="title">Message title.</param>
        /// <param name="field">Message information field.</param>
        /// <param name="buttonType">One of builded button type (OK or OK/Cancel).</param>
        /// <returns>Response when user press one of the button (OK -> MessageBoxResponse.OK, Cancel -> MessageBoxResponse.Cancel).</returns>
        public MessageBoxResult Show(string title, string field, MessageBoxButton buttonType)
        {
            Result = MessageBoxResult.None;
            Title = title;
            Field = field;
            ButtonType = buttonType;

            EventHandler handler = DialogBoxEvent;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            return Result;
        }

        /// <summary>
        /// Method to display DialogBox (from Coding4Fun control) with 2 options: Yes/No.
        /// </summary>
        /// <param name="title">Message title.</param>
        /// <param name="field">Message information field.</param>
        public void ShowYesNoQuestion(string title, string field)
        {
            Title = title;
            Field = field;

            EventHandler handler = YesNoMessageEvent;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
