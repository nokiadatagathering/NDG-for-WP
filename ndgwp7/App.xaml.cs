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
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using com.comarch.mobile.ndg.Settings.ViewModel;

namespace com.comarch.mobile.ndg
{
    /// <summary>
    /// Static and standard application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Represents various field which cannot be sending between pages in Uri string.
        /// </summary>
        public static Dictionary<String, object> AppDictionary { get;  set; }

        /// <summary>
        /// Represents information about various application state on sepereted page.
        /// </summary>
        public enum AplicationStates
        {
            /// <summary>
            /// State when user just go to new page.
            /// </summary>
            Runing,
            /// <summary>
            /// State when application was resumed.
            /// </summary>
            Activated,
            /// <summary>
            /// State when appliation is holding.
            /// </summary>
            Deactivated
        }

        // Declare a private variable to store application state.
        private string _applicationDataObject;
        private AplicationStates _applicationState;

        /// <summary>
        /// Declare a public property to access the application data variable.
        /// </summary>
        public string ApplicationDataObject
        {
            get { return _applicationDataObject; }
            set
            {
                if (value != _applicationDataObject)
                {
                    _applicationDataObject = value;
                }
            }
        }
        /// <summary>
        /// Represents information about current appliacation state.
        /// </summary>
        public AplicationStates ApplicationState
        {
            set
            {
                _applicationState = value;
            }
            get
            {
                return _applicationState;
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Application themes
            CustomColors();

            AppDictionary = new Dictionary<string, object>();
        }

        private void CustomColors()
        {
            SettingsViewModel vm = new SettingsViewModel();
            vm.CustomColors();
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            _applicationState = AplicationStates.Runing;
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            _applicationState = AplicationStates.Activated;
            if (PhoneApplicationService.Current.State.ContainsKey("ApplicationDataObject"))
            {
                ApplicationDataObject = PhoneApplicationService.Current.State["ApplicationDataObject"] as string;
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {  // If there is data in the application member variable...

            _applicationState = AplicationStates.Deactivated;
            if (!string.IsNullOrEmpty(ApplicationDataObject))
            {
                // Store it in the State dictionary.
                PhoneApplicationService.Current.State["ApplicationDataObject"] = ApplicationDataObject;
            }
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (isoStore.FileExists("tmpresult.xml"))
            {
                isoStore.DeleteFile("tmpresult.xml");
            }
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is QuitException)
            {
                return;
            }
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
            {
                return;
            }

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
            {
                RootVisual = RootFrame;
            }

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
        
        private class QuitException : Exception { }

        /// <summary>
        /// Method to close application.
        /// </summary>
        public static void Quit()
        {
            throw new QuitException(); //Exception is thorwn for a reason. Ignore.
        }
    }
}