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
namespace com.comarch.mobile.ndg.Settings.Model
{
    /// <summary>
    /// Class using only in App.xaml for application.resources. In this way strings in various language can be used in *.xaml files.
    /// </summary>
    public class SettingValues
    {
        /// <summary>
        /// Get all settings in .xaml files.
        /// </summary>
        public OperationsOnSettings Values { get; private set; }

        /// <summary>
        /// Copy all settings from settings instance to Values property.
        /// </summary>
        public SettingValues()
        {
            Values = OperationsOnSettings.Instance;
        }
    }
}
