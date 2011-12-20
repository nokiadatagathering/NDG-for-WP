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
using System.Globalization;
using System.Windows.Data;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Converter which changes full server address (with name of servlets and protocol) to short URL (containing only IP address and port number) and converts back from short URL to full address.
    /// </summary>
    public class ServerUrlDisplayConverter : IValueConverter
    {
        /// <summary>
        /// Changes URL address from full address to short address by removing protocol and name of servlet.
        /// </summary>
        /// <param name="value">Full URL address (with protocol and servlet name).</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns short URL address (only IP address [or DNS] and port number.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string output = string.Empty;
            if (value != null)
            {
                output = value.ToString().Replace("http://", "");
                output = output.Replace("/ndg-servlets/", "");
            }
            return output;
        }

        /// <summary>
        /// Changes URL address from short address to full address by adding protocol and name of servlet.
        /// </summary>
        /// <param name="value">Short URL address (only IP address and port number).</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns full URL address (with protocol and servlet name).</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string output = value.ToString();
            if (!output.StartsWith("http://"))
            {
                output = "http://" + output;
            }
            output = output + "/ndg-servlets/";
            return output;
        }
    }
}
