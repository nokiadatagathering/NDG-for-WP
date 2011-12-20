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

namespace com.comarch.mobile.ndg.MessageDialog
{
    /// <summary>
    /// Static class containing two type of MessegaBox: standard MessageBox and customize DialogBox.
    /// </summary>
    static public class MessageView
    {
        /// <summary>
        /// Method to display standard WP7 MessageBox.
        /// </summary>
        /// <param name="box">Contain information about MessageBox (e.g. title, message field, type of button).</param>
        public static void AssignDisplay(DialogBox box)
        {
            box.DialogBoxEvent += (object sender, EventArgs args) =>
            {
                if (box.Title != null)
                {
                    box.Result = MessageBox.Show(box.Field, box.Title, box.ButtonType);
                }
                else
                {
                    box.Result = MessageBox.Show(box.Field);
                }
            };
        }

        /// <summary>
        /// Method to display customize DialogBox with options Yes/No.
        /// </summary>
        /// <param name="box">Contain information about MessageBox (e.g. title, message field)</param>
        public static void AssignYesNoMessage(DialogBox box)
        {
            box.YesNoMessageEvent += (object sender, EventArgs args) =>
            {
                YesNoMessageBox messageBox = new YesNoMessageBox();
                messageBox.Message = box.Field;
                messageBox.Title = box.Title;
                messageBox.Completed += (object YesNosender, EventArgs completedArgs) =>
                {
                    box.YesNoResponse = (YesNosender as YesNoMessageBox).Response;
                    if (box.YesNoAnswerCompleted != null)
                    {
                        box.YesNoAnswerCompleted(sender, args);
                    }
                };
                messageBox.Show();
            };
        }
    }
}
