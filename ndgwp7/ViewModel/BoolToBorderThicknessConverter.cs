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
    /// Converter which changes border thickness when input value is true. Used in ValidationControl element.
    /// </summary>
    public class BoolToBorderThicknessConverter : IValueConverter
    {
        /// <summary>
        /// Sets element border thickness on 3 px if value is TRUE or removes border (0 px) when is FALSE.
        /// </summary>
        /// <param name="value">Value of validation control (TRUE or FALSE).</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns border thickness value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "3";
            }
            return "0";
        }

        /// <summary>
        /// Converts border thickness to boolean value.
        /// </summary>
        /// <param name="value">Border thickness value.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Return TRUE if border is set or FALSE when isn't.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value != "3")
            {
                return false;
            }
            return true;
        }
    }
}
