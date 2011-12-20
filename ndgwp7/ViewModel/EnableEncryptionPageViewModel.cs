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
using com.comarch.mobile.ndg.Settings.Model;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Class stores methods used during communication between model and view on EnableEncryption.
    /// </summary>
    public class EnableEncryptionPageViewModel
    {
        /// <summary>
        /// Represents encryption status of results in application.
        /// </summary>
        public bool IsEncryptionEnabled { get; set; }
        /// <summary>
        /// Contains encryption password when encryption is enabled.
        /// </summary>
        public string EncryptionPassword { get; set; }

        /// <summary>
        /// Method to save encryption password during first application start.
        /// </summary>
        /// <returns>Returns TRUE if password is saved or FALSE when saving operation has failed.</returns>
        public bool SaveChanges()
        {
            if (IsEncryptionEnabled)
            {
                if (!String.IsNullOrEmpty(EncryptionPassword))
                {
                    if (App.AppDictionary.ContainsKey("EncryptionPassword"))
                        App.AppDictionary["EncryptionPassword"] = EncryptionPassword;
                    else
                        App.AppDictionary.Add("EncryptionPassword", EncryptionPassword);
                }
                else
                {
                    return false;
                }
            }
            OperationsOnSettings.Instance.IsEncryptionEnabled = IsEncryptionEnabled;
            return true;
        }
    }
}
