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
using System.Collections.Generic;
using System.ComponentModel;

namespace com.comarch.mobile.ndg.Settings.Model
{
    /// <summary>
    /// Class which contain setting structure (saved in IsolatedStorageSettings and using in OperationOnSettings).
    /// </summary>
    public class SettingEntity : INotifyPropertyChanged
    {
        /// <summary>
        /// Represents information about setting key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Represents value which will be display on SettingPage.
        /// </summary>
        public string Display { get; set; }

        private string _value;
        /// <summary>
        /// Represents setting value.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
            }
        }

        /// <summary>
        /// Represents type of setting (diffrent type of setting choice element on SettingPage).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Represents values which can be selected by user on SettingPage (in 'choice' type setting).
        /// </summary>
        public IList<string> Capabilities { get; set; }

        /// <summary>
        /// Represents real values linked with Capabilities (which are 'display layer' for real value).
        /// </summary>
        public IList<string> RealCapabilities { get; set; }

        private string _custom;
        /// <summary>
        /// Represents value in 'custom' type setting (like font size).
        /// </summary>
        public string Custom
        {
            get { return _custom; }
            set
            {
                _custom = value;
                RaisePropertyChanged("Custom");
            }
        }

        /// <summary>
        /// Event which notify when values was changed.
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
