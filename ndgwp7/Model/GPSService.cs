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
using System.Device.Location;
using System.Threading;

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Singleton class responsible for retrieving geographic location.
    /// </summary>
    public class GPSService
    {
        private static readonly GPSService _instance = new GPSService();
        /// <summary>
        /// Represents single instance of <see cref="GPSService"/> class.
        /// </summary>
        /// <value>Gets _instance data member.</value>
        public static GPSService Instance
        {
            get { return _instance; }
        }

        private GPSService()
        {
            CoordinateWatcher = new GeoCoordinateWatcher();
        }
        /// <summary>
        /// Represents instance of GeoCoordinateWatcher class that is used to retrieve location.
        /// </summary>
        public static IGeoPositionWatcher<GeoCoordinate> CoordinateWatcher { get; set; }
        private readonly ManualResetEvent _syncLocation = new ManualResetEvent(false);

        private GeoCoordinate _location;
        /// <summary>
        /// Runs geographic location retrieval process. Holds thread until location is retrieved or timeout elapses.
        /// </summary>
        public GeoCoordinate Location
        {
            get
            {
                CoordinateWatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(OnPositionChanged);
                
                bool started = CoordinateWatcher.TryStart(true, TimeSpan.FromSeconds(5));
                if (!started)
                {
                    return null;
                }
                // wait until GeoCoordinate data is set
                _syncLocation.WaitOne();
                CoordinateWatcher.Stop();
                return _location;
            }
        }

        private void OnPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            CoordinateWatcher.PositionChanged -= OnPositionChanged; // just to make sure that only one location update occures
            _location = e.Position.Location;
            _syncLocation.Set();
        }
    }
}
