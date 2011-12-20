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
using System.Collections.Generic;
using System.Device.Location;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;
using com.comarch.mobile.ndg.Model;
using com.comarch.mobile.ndg.ViewModel;
using System.Windows.Navigation;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// Class contains all methods used during operation on BingMapPage.
    /// </summary>
    public partial class BingMapPage : PhoneApplicationPage
    {
        private bool _isDrawing;
        private bool _isSelecting;
        private bool _isLoadedCoordinates;
        private bool _isChangedCoordinates;
        private string _latitude;
        private string _longitude;
        private string _radius;
        private int _zoom = 0;
        private BingMapViewModel _viewModel;

        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public BingMapPage()
        {
            InitializeComponent();
            _viewModel = new BingMapViewModel();
            BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar
            ApplicationBar = new ApplicationBar();

            ApplicationBar.ForegroundColor = (App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush).Color;
            ApplicationBar.BackgroundColor = (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color;

            // Create a new button and set the text value to the localized string from AppResources
            ApplicationBarIconButton appBarSelectionModeButton = new ApplicationBarIconButton(new Uri("/View/icons/MapsApply.png", UriKind.Relative));
            appBarSelectionModeButton.Text = Languages.AppResources.resultsFilterPageAppBar_Apply;
            appBarSelectionModeButton.Click += OnSelectionMode;

            ApplicationBarIconButton appBarMyPosition = new ApplicationBarIconButton(new Uri("/View/icons/MapsMe.png", UriKind.Relative));
            appBarMyPosition.Text = "Me";
            appBarMyPosition.Click += OnMyPosition;

            ApplicationBar.Buttons.Add(appBarSelectionModeButton);
            ApplicationBar.Buttons.Add(appBarMyPosition);
        }

        private void DrawCircle(int zoom = 1)
        {
            Polygon.Locations.Clear();
            double radius = Math.Sqrt(Math.Pow(GetPoint(_viewModel.Map.StartLocation).X - GetPoint(_viewModel.Map.StopLocation).X, 2) + Math.Pow(GetPoint(_viewModel.Map.StartLocation).Y - GetPoint(_viewModel.Map.StopLocation).Y, 2));
            for (double alpha = 0.0; alpha < 2 * Math.PI; alpha += Math.PI / 16.0)
            {
                double x = GetPoint(_viewModel.Map.StartLocation).X + radius * Math.Cos(alpha);
                double y = GetPoint(_viewModel.Map.StartLocation).Y + radius * Math.Sin(alpha);

                if (!_isLoadedCoordinates)
                {
                    // solution for draw circle when selecting area
                    x = x < 0 ? 0 : (x > Map.ViewportSize.Width ? Map.ViewportSize.Width : x );
                }

                Polygon.Locations.Add(GetCoords(new Point(x, y)));
            }
            
            if (_isLoadedCoordinates)
            {
                Map.SetView(_viewModel.Map.StartLocation, _zoom);
                _isLoadedCoordinates = false;
            }
        }

        private void OnSelectionMode(object sender, EventArgs e)
        {
            _isSelecting = !_isSelecting;
            Map.IsEnabled = !Map.IsEnabled;
        }

        private void OnMyPosition(object sender, EventArgs e)
        {
            if (_viewModel.Map.Location == null)
            {
                _viewModel.Map.Location = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
                _viewModel.Map.Location.StatusChanged += LocationStatusChanged;
            }
            _viewModel.Map.Location.Start();
        }

        private GeoCoordinate GetCoords(Point point)
        {
            return Map.ViewportPointToLocation(point);
        }

        private Point GetPoint(GeoCoordinate geoCoords)
        {
            return Map.LocationToViewportPoint(geoCoords);
        }

        /// <summary>
        /// Standard WP7 method which is running always when user navigates to page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string action = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("showPushpins", out action))
            {
                bool isGeoCoordinatesSet = ((App.AppDictionary["Pushpins"] as List<GPSEntity>) != null);
                if (Convert.ToBoolean(action) && isGeoCoordinatesSet)
                {
                    List<GeoCoordinate> allPinsLocation = new List<GeoCoordinate>();

                    foreach (var pinPosition in App.AppDictionary["Pushpins"] as List<GPSEntity>)
                    {
                        Pushpin pushpin = new Pushpin();
                        pushpin.Location = pinPosition.Location;
                        pushpin.Content = pinPosition.Title;
                        Map.Children.Add(pushpin);
                        allPinsLocation.Add(pinPosition.Location);
                    }

                    Map.SetView(LocationRect.CreateLocationRect(allPinsLocation));
                }
            }
            else if (!string.IsNullOrEmpty(App.AppDictionary["Latitude"] as String) && !string.IsNullOrEmpty(App.AppDictionary["Longitude"] as String) && !string.IsNullOrEmpty(App.AppDictionary["Radius"] as String))
            {
                _latitude = App.AppDictionary["Latitude"] as String;
                _longitude = App.AppDictionary["Longitude"] as String;
                _radius = App.AppDictionary["Radius"] as String;

                if (Math.Abs(Convert.ToDouble(_latitude)) <= 90 && (Math.Abs(Convert.ToDouble(_longitude)) <= 180))
                {
                    _viewModel.Map.StartLocation = new GeoCoordinate(Convert.ToDouble(_latitude), Convert.ToDouble(_longitude));
                    double resolution = 156543.04 * Math.Cos(Convert.ToDouble(_latitude)) / Math.Pow(2, Map.ZoomLevel); // resolution in m/px
                    double distance = Math.Abs(Convert.ToDouble(_radius) / resolution); // distance in px
                    Point point = GetPoint(_viewModel.Map.StartLocation);
                    _viewModel.Map.StopLocation = GetCoords(new Point(point.X, point.Y - distance));
                    _isLoadedCoordinates = true;

                    // for right zoom level
                    while (distance < 200)
                    {
                        _zoom++;
                        distance *= 2;
                    }

                    DrawCircle(_zoom);
                }
            }

            _isSelecting = false;
            Map.IsEnabled = true;
        }

        private void LocationStatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                Map.SetView(_viewModel.Map.Location.Position.Location, 10.0);
                _viewModel.Map.Location.Stop();
            }
            else if (e.Status == GeoPositionStatus.Disabled)
            {
                MessageBox.Show(Languages.AppResources.resultsFilter_LocationDisable);
            }
        }

        /// <summary>
        /// Standard WP7 method which is running always when user touches the screen.
        /// </summary>
        /// <param name="e">MouseButtonEvent argument</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (_isSelecting)
            {
                _isDrawing = true;
                _viewModel.Map.StartLocation = GetCoords(e.GetPosition(Map));
            }
            else
            {
                base.OnMouseLeftButtonDown(e);
            }
        }

        /// <summary>
        /// Standard WP7 method which is running always when user stops touching the screen.
        /// </summary>
        /// <param name="e">MouseButtonEvent argument</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_isSelecting)
            {
                _isDrawing = false;
                _isChangedCoordinates = true;
            }
            else
            {
                base.OnMouseLeftButtonUp(e);
            }
        }

        /// <summary>
        /// Standard WP7 method which is running always when user moves finger when touching the screen.
        /// </summary>
        /// <param name="e">MouseButtonEvent argument</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _viewModel.Map.StopLocation = GetCoords(e.GetPosition(Map));
                DrawCircle();
            }
            else
            {
                base.OnMouseMove(e);
            }
        }

        /// <summary>
        /// Standard WP7 method which is running always before user navigates to new page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            string strLatitude;
            string strLongitude;
            string strRadius;

            if (_isChangedCoordinates)
            {
                strLatitude = _viewModel.Map.StartLocation.Latitude.ToString();
                strLongitude = _viewModel.Map.StartLocation.Longitude.ToString();
                strRadius = _viewModel.Map.StartLocation.GetDistanceTo(_viewModel.Map.StopLocation).ToString();
            }
            else
            {
                strLatitude = _latitude;
                strLongitude = _longitude;
                strRadius = _radius;
            }

            if (App.AppDictionary.ContainsKey("Latitude"))
                App.AppDictionary["Latitude"] = strLatitude;
            else
                App.AppDictionary.Add("Latitude", strLatitude);

            if (App.AppDictionary.ContainsKey("Longitude"))
                App.AppDictionary["Longitude"] = strLongitude;
            else
                App.AppDictionary.Add("Longitude", strLongitude);

            if (App.AppDictionary.ContainsKey("Radius"))
                App.AppDictionary["Radius"] = strRadius;
            else
                App.AppDictionary.Add("Radius", strRadius);

            base.OnNavigatedFrom(e);
        }
    }
}