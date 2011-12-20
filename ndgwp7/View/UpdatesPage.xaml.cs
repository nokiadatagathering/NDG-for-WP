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
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace com.comarch.mobile.ndg.View
{
    /// <summary>
    /// Class contains all methods used during operation on UpdatesPage.
    /// </summary>
    public partial class UpdatesPage : PhoneApplicationPage
    {
        /// <summary>
        /// Default constuctor which initializes component on page.
        /// </summary>
        public UpdatesPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Standard WP7 method which is running always when user navigates to page.
        /// </summary>
        /// <param name="e">NavigationService argument</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if ((Application.Current as App).ApplicationState == App.AplicationStates.Activated)
            {
                // Do what you have to do when application is activated and change state for runing. 
                (Application.Current as App).ApplicationState = App.AplicationStates.Runing;
            }
        }
    }
}