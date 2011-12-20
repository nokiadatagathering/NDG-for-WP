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
    /// Converter which changes GPS position (latitude) from decimal degree to DMS and converts back form DMS to decimal degree.
    /// </summary>
    public class LatitudeGeoCoordinatesConverter: IValueConverter
    {
        /// <summary>
        /// Changes decimal degree of latitude to DMS value.
        /// </summary>
        /// <param name="value">Latitude in decimal degree (form -90 to 90).</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns latitude in DMS convention.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int degree = 0;
            int minutes = 0;
            int seconds = 0;
            string direction = "N";

            if (!string.IsNullOrEmpty(value.ToString()))
            {
                double decimalDegree = System.Convert.ToDouble(value);

                direction = (decimalDegree < 0) ? "S" : "N";

                decimalDegree = Math.Abs(decimalDegree);

                degree = (int)decimalDegree;
                decimalDegree -= degree;
                decimalDegree *= 60;

                minutes = (int)(decimalDegree);
                decimalDegree -= minutes;
                decimalDegree *= 60;

                seconds = (int)(decimalDegree);
                decimalDegree -= seconds;
            }

            return string.Format("{0} {1}°{2}'{3}\"", direction, degree, minutes, seconds);

        }

        /// <summary>
        /// Changes DMS value of latitude to decimal degree.
        /// </summary>
        /// <param name="value">Latitude in DMS (form N/S 0°0'0" to 90°0'0").</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns latitude in decimal degree convention.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!string.IsNullOrEmpty(value.ToString()))
            {
                try
                {
                    char[] delimiterChars = { ' ', '°', '\'', '"' };
                    string[] degrees = value.ToString().Split(delimiterChars);

                    int sign = ( degrees[0] == "S" ) ? -1 : 1;

                    double decimalDegree = sign * (System.Convert.ToDouble(degrees[1]) + System.Convert.ToDouble(degrees[2]) / 60 + System.Convert.ToDouble(degrees[3]) / 3600);

                    return decimalDegree;
                }
                catch (IndexOutOfRangeException)
                {
                    return string.Empty;
                }
                catch (FormatException)
                {
                    return string.Empty;
                }
             }
             else
             {
                 return string.Empty;
             }
        }
    }

    /// <summary>
    /// Converter which changes GPS position (longitude) from decimal degree to DMS and converts back form DMS to decimal degree.
    /// </summary>
    public class LongitudeGeoCoordinatesConverter : IValueConverter
    {
        /// <summary>
        /// Changes decimal degree of longitude to DMS value.
        /// </summary>
        /// <param name="value">Longitude in decimal degree (form -180 to 180).</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns longitude in DMS convention.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int degree = 0;
            int minutes = 0;
            int seconds = 0;
            string direction = "E";

            if (!string.IsNullOrEmpty(value.ToString()))
            {
                double decimalDegree = System.Convert.ToDouble(value);

                direction = (decimalDegree < 0) ? "W" : "E";

                decimalDegree = Math.Abs(decimalDegree);

                degree = (int)decimalDegree;
                decimalDegree -= degree;
                decimalDegree *= 60;

                minutes = (int)(decimalDegree);
                decimalDegree -= minutes;
                decimalDegree *= 60;

                seconds = (int)(decimalDegree);
                decimalDegree -= seconds;
            }

            return string.Format("{0} {1}°{2}'{3}\"", direction, degree, minutes, seconds);

        }

        /// <summary>
        /// Changes DMS value of longitude to decimal degree.
        /// </summary>
        /// <param name="value">Longitude in DMS (form W/E 0°0'0" to 180°0'0").</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns longitude in decimal degree convention.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!string.IsNullOrEmpty(value.ToString()))
            {
                try
                {
                    char[] delimiterChars = { ' ', '°', '\'', '"' };
                    string[] degrees = value.ToString().Split(delimiterChars);

                    int sign = (degrees[0] == "W") ? -1 : 1;

                    double decimalDegree = sign * (System.Convert.ToDouble(degrees[1]) + System.Convert.ToDouble(degrees[2]) / 60 + System.Convert.ToDouble(degrees[3]) / 3600);
                    return decimalDegree;
                }
                catch (IndexOutOfRangeException)
                {
                    return string.Empty;
                }
                catch (FormatException)
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Converter which adds/removes meter unit to/from value.
    /// </summary>
    public class RadiusGeoCoordinatesConverter : IValueConverter
    {
        /// <summary>
        /// Adds meter unit to radius decimal value.
        /// </summary>
        /// <param name="value">Decimal value of radius.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns radius value with meter unit in string.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!string.IsNullOrEmpty(value.ToString()))
                return string.Format("{0} m", value);

            return value;
        }

        /// <summary>
        /// Removes meter unit from radius value.
        /// </summary>
        /// <param name="value">Radius value with meter unit in string.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Returns decimal radius value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] radius = value.ToString().Split(' ');

            return radius[0];
        }
    }
}
