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
using System.Windows;
using System.Windows.Data;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Converter which sets element visibility if right index is selected (e.g. in Listpicker).
    /// </summary>
    public class SelectedIndexToVisibilityConverter: IValueConverter
    {
        /// <summary>
        /// Sets element visibility to Visible when first index is selected or Collapsed when isn't.
        /// </summary>
        /// <param name="value">Selected index in element.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns Visibility.Visible or Visibility.Collapsed when selected index is equal 0 or not.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value.ToString() == "0") ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// This converter is never used but must be defined.
        /// </summary>
        /// <param name="value">Value changed by converter.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns TRUE when is input value is Visible or FALSE when is Collapsed.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }
}
