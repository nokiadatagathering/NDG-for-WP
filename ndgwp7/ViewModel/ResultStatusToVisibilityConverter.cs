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
using com.comarch.mobile.ndg.Model;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Converter which sets element svisibility in context menu based on restult status.
    /// </summary>
    public class ResultStatusToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Sets elements visibility based on actual result status.
        /// </summary>
        /// <param name="value">Result status of result.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns Visibility.Visible and Visibility.Collapsed for various items in context menu based on current result status.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ResultBasicInfo.ResultStatus result = (ResultBasicInfo.ResultStatus)value;
            switch (parameter.ToString())
            {
                case "Invert":
                    switch (result)
                    {
                        case ResultBasicInfo.ResultStatus.NotReadyToSend:
                            return Visibility.Collapsed;
                        case ResultBasicInfo.ResultStatus.Sent:
                            return Visibility.Visible;
                        case ResultBasicInfo.ResultStatus.ReadyToSend:
                            return Visibility.Collapsed;
                    }
                    break;
                default:
                    switch (result)
                    {
                        case ResultBasicInfo.ResultStatus.NotReadyToSend:
                            return Visibility.Visible;
                        case ResultBasicInfo.ResultStatus.Sent:
                            return Visibility.Collapsed;
                        case ResultBasicInfo.ResultStatus.ReadyToSend:
                            return Visibility.Visible;
                    }
                    break;
            }
            return null;
        }
        /// <summary>
        /// This converter is never used but must be defined.
        /// </summary>
        /// <param name="value">Value changed by converter.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns null value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
