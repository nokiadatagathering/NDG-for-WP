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
using com.comarch.mobile.ndg.BusyIndicator;
using com.comarch.mobile.ndg.MessageDialog;

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Contains status information of download of surveys process.
    /// </summary>
    public class DownloadSurveysStatus
    {
        /// <summary>
        /// Represents instance of <see cref="DialogBox"/> class.
        /// </summary>
        public DialogBox Message { get; set; }

        /// <summary>
        /// Initializes all necessary data objects.
        /// </summary>
        public DownloadSurveysStatus()
        {
            ProgressBar = new ProcessingBar();
            Message = new DialogBox();
            CanCancel = true;
        }

        /// <summary>
        /// Triggers when user wants to go back.
        /// </summary>
        public event EventHandler GoBackEventHandler;

        /// <summary>
        /// Represents instance of <see cref="ProcessingBar"/> class.
        /// </summary>
        public ProcessingBar ProgressBar { get; set; }

        /// <summary>
        /// Method used to trigger GoBackEventhandler.
        /// </summary>
        public void GoBack()
        {
            EventHandler handler = GoBackEventHandler;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// Represents whether download can be canceled or not.
        /// </summary>
        public bool CanCancel { get; set; }

        /// <summary>
        /// Represents whether download was canceled or not.
        /// </summary>
        public bool IsCanceled { get; set; }
        
    }
}
