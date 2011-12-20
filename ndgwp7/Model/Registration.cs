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

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Class stores data used during registration process.
    /// </summary>
    public class Registration : INotifyPropertyChanged
    {
        /// <summary>
        /// Allows you to set members data at initialization.
        /// </summary>
        /// <param name="imei">String value that sets Imei property.</param>
        /// <param name="phoneNumber">String value that sets PhoneNumber property.</param>
        /// <param name="serverUrl">String value that sets ServerUrl property.</param>
        public Registration(string imei, string phoneNumber, string serverUrl)
        {
            _imei = imei;
            _phoneNumber = phoneNumber;
            _serverUrl = serverUrl;
        }

        /// <summary>
        /// Initializes all necesarry data members.
        /// </summary>
        public Registration() { }

        private string _imei = string.Empty;
        /// <summary>
        /// Represents current Imei.
        /// </summary>
        /// <value>Gets/Sets _imei data member.</value>
        public string Imei
        {
            get
            {
                return _imei;
            }
            set
            {
                _imei = value;
                RaisePropertyChanged("Imei");
            }
        }

        private string _phoneNumber = string.Empty;
        /// <summary>
        /// Represents current PhoneNumber.
        /// </summary>
        /// <value>Gets/Sets _phoneNumber data member.</value>
        public string PhoneNumber
        {
            get
            {
                return _phoneNumber;
            }
            set
            {
                _phoneNumber = value;
                RaisePropertyChanged("PhoneNumber");
            }
        }

        private string _serverUrl = string.Empty;
        /// <summary>
        /// Represents current ServerUrl.
        /// </summary>
        /// <value>Gets/Sets _serverUrl data member.</value>
        public string ServerUrl
        {
            get
            {
                return _serverUrl;
            }
            set
            {
                _serverUrl = value;
                RaisePropertyChanged("ServerUrl");
            }
        }

        private bool _imeiIsValid = true;
        /// <summary>
        /// Represents status of imei validation.
        /// </summary>
        /// <value>Gets/Sets _imeiIsValid data member.</value>
        public bool ImeiIsValid
        {
            get
            {
                return _imeiIsValid;
            }
            set
            {
                _imeiIsValid = value;
                RaisePropertyChanged("ImeiIsValid");
            }
        }

        private bool _phoneNumberIsValid = true;
        /// <summary>
        /// Represents status of phone number validation.
        /// </summary>
        /// <value>Gets/Sets _phoneNumberIsValid data member.</value>
        public bool PhoneNumberIsValid
        {
            get
            {
                return _phoneNumberIsValid;
            }
            set
            {
                _phoneNumberIsValid = value;
                RaisePropertyChanged("PhoneNumberIsValid");
            }
        }

        private bool _serverUrlIsValid = true;
        /// <summary>
        /// Represents status of server url validation.
        /// </summary>
        /// <value>Gets/Sets _serverUrlIsValid data member.</value>
        public bool ServerUrlIsValid
        {
            get
            {
                return _serverUrlIsValid;
            }
            set
            {
                _serverUrlIsValid = value;
                RaisePropertyChanged("ServerUrlIsValid");
            }
        }

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
