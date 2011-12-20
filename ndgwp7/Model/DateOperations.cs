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


namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Contains methods used for operations on various date formats.
    /// </summary>
    public class DateOperations
    {
        /// <summary>
        /// Converts DateTime class to miliseconds.
        /// </summary>
        /// <param name="date">Instance of DateTime class.</param>
        /// <returns>Time in miliseconds that elapsed since 01-01-1970.</returns>
        public long DateTimeToMiliseconds(DateTime date)
        {
            DateTime januar_1_1970 = new DateTime(1970, 1, 1);
            return (date.Ticks - januar_1_1970.Ticks) / 10000;
        }

        /// <summary>
        /// Converts miliseconds to DateTime class.
        /// </summary>
        /// <param name="miliseconds">Amount of miliseconds that elapsed since 01-01-1970.</param>
        /// <returns>Instance of DateTime class.</returns>
        public DateTime MilisecondsToDateTime(long miliseconds)
        {
            DateTime januar_1_1970 = new DateTime(1970, 1, 1);
            return new DateTime(miliseconds * 10000 + januar_1_1970.Ticks);
        }

        /// <summary>
        /// Converts date string to DateTime class.
        /// </summary>
        /// <param name="date">Date in string format - dd/mm/yyyy.</param>
        /// <returns>Instance of DateTime class.</returns>
        public DateTime ParseDate(string date)
        {
            string[] split = date.Split('/');
            int day = Convert.ToInt32(split[0]);
            int month = Convert.ToInt32(split[1]);
            int year = Convert.ToInt32(split[2]);
            return new DateTime(year, month, day);
        }
    }
}
