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
using System.ComponentModel;
using com.comarch.mobile.ndg.BusyIndicator;
using com.comarch.mobile.ndg.MessageDialog;

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Contains status information of download list of surveys process.
    /// </summary>
    public class DownloadListStatus : INotifyPropertyChanged
    {
        /// <summary>
        /// Represents instance of <see cref="ProcessingBar"/> class.
        /// </summary>
        public ProcessingBar ProgressBar { get; set; }

        /// <summary>
        /// Represents instance of <see cref="DialogBox"/> class.
        /// </summary>
        public DialogBox Message { get; set; }

        private bool _isDownloadButtonEnabled;

        /// <summary>
        /// Represents status of download button.
        /// </summary>
        /// <value>Gets/Sets _isDownloadButtonEnabled data member.</value>
        public bool IsDownloadButtonEnabled
        {
            get { return _isDownloadButtonEnabled; }
            set 
            { 
                _isDownloadButtonEnabled = value;
                RaisePropertyChanged("IsDownloadButtonEnabled");
            }
        }

        /// <summary>
        /// Initializes all necessary data objects.
        /// </summary>
        public DownloadListStatus()
        {
            ProgressBar = new ProcessingBar();
            Message = new DialogBox();
        }

        private string _statusMessage;
        /// <summary>
        /// Represents message shown after download is completed.
        /// </summary>
        /// <value>Gets/Sets _statusMessage data member.</value>
        public string StatusMessage
        {
            get { return _statusMessage; }
            set 
            { 
                _statusMessage = value;
                RaisePropertyChanged("StatusMessage");
            }
        }

        private bool _show = false;
        /// <summary>
        /// Represents visual state of <see cref="StatusMessage"/> property. True - visible, False - collapsed.
        /// </summary>
        /// <value>Gets/Sets _show data member.</value>
        public bool Show
        {
            get { return _show; }
            set 
            { 
                _show = value;
                RaisePropertyChanged("Show");
            }
        }
        /// <summary>
        /// Event that triggers when property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string arg)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(arg));
            }
        }
    }
}
