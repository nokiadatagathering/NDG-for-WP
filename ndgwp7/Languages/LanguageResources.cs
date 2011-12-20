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
using System.Globalization;
using com.comarch.mobile.ndg.Settings.Model;

namespace com.comarch.mobile.ndg.Languages
{
    /// <summary>
    /// Class containing appropriate language strings.
    /// </summary>
    public class LanguageResources
    {
        private static AppResources _localizedResources = new AppResources();
        
        /// <summary>
        /// Get text in right language.
        /// </summary>
        public AppResources LocalizedResources { get { return _localizedResources; } }

        /// <summary>
        /// Selecting right application language (phone language or selected language) and set appropriate Application Culture.
        /// </summary>
        public LanguageResources()
        {
            if (OperationsOnSettings.Instance.IsAppLanguageSet())
            {
                AppResources.Culture = new CultureInfo(OperationsOnSettings.Instance.Language);
            }
            else
            {
                AppResources.Culture = new CultureInfo(CultureInfo.CurrentUICulture.Name);
            }
        }
    }
}
