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

namespace com.comarch.mobile.ndg.BusyIndicator
{
    /// <summary>
    /// Class used to simplify usage of <see cref="BusyIndicatorControl" /> class.
    /// </summary>
    public class ProcessingBar : INotifyPropertyChanged
    {
        private bool _isEnabled;
        /// <summary>
        /// Represents visibility of processing bar.
        /// </summary>
        /// <value>Gets/Sets _isEnabled data member.</value>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
        }

        private string _information = Languages.AppResources.processingBar_Working;
        /// <summary>
        /// Represents information displayed when processing bar is visible.
        /// </summary>
        /// <value>Gets/Sets _information data member.</value>
        public string Information
        {
            get
            {
                return _information;
            }
            set
            {
                _information = value;
                RaisePropertyChanged("Information");
            }
        }
        /// <summary>
        /// Allows you to set displayed information at initialization.
        /// </summary>
        /// <param name="information">String value that is used to set <see cref="Information" /> property.</param>
        public ProcessingBar(string information)
        {
            Information = information;
        }

        /// <summary>
        /// Initializes all necassary data members.
        /// </summary>
        public ProcessingBar() { }

        /// <summary>
        /// Triggers when property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
