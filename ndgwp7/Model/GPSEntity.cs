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
using System.Device.Location;

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Class which contain information about GPS location and title of survey's result.
    /// </summary>
    public class GPSEntity
    {
        /// <summary>
        /// Represents information about location when survey was filled out.
        /// </summary>
        public GeoCoordinate Location { get; set; }

        /// <summary>
        /// Represents information about tiitle of saved result.
        /// </summary>
        public string Title { get; set; }
    }
}
