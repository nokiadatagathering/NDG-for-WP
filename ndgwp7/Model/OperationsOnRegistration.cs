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
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using com.comarch.mobile.ndg.BusyIndicator;
using com.comarch.mobile.ndg.MessageDialog;
using com.comarch.mobile.ndg.Settings.Model;

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Class contains all methods used during registration process.
    /// </summary>
    public class OperationsOnRegistration
    {
        /// <summary>
        /// Represents instance of <see cref="Registration"/> class.
        /// </summary>
        public Registration RegistrationInstance { get; set; }

        /// <summary>
        /// Represents instance of <see cref="DialogBox"/> class.
        /// </summary>
        public DialogBox ConnectionResult { get; set; }

        /// <summary>
        /// Represents instance of <see cref="ProcessingBar"/> class.
        /// </summary>
        public ProcessingBar Busy { get; set; }

        /// <summary>
        /// Triggers when user wants to leave registration panel.
        /// </summary>
        public event EventHandler LeaveRegistrationEvent;

        /// <summary>
        /// Represents result of registration status.
        /// </summary>
        public enum ResponseType
        {
            /// <summary>
            /// Indicates that registration was successful.
            /// </summary>
            Successful,
            /// <summary>
            /// Indicates that registration fialed.
            /// </summary>
            Failed,
            /// <summary>
            /// Indicates that server is not responding.
            /// </summary>
            ResponseIsNull
        }

        /// <summary>
        /// Initializes all necessary object data.
        /// </summary>
        public OperationsOnRegistration()
        {
            RegistrationInstance = new Registration();
            RegistrationInstance.ServerUrl = OperationsOnSettings.Instance.ServerURL;
            Busy = new ProcessingBar(Languages.AppResources.operationsOnRegistration_ProcessingBarInformation);

            ConnectionResult = new DialogBox();
        }

        /// <summary>
        /// Starts registration process.
        /// </summary>
        public void Register()
        {

            Busy.IsEnabled = true;
            string commandUrl = CreateRequestUrl(RegistrationInstance.PhoneNumber, RegistrationInstance.Imei, RegistrationInstance.ServerUrl);
            try
            {
                var request = WebRequest.Create(commandUrl);
                var result = (IAsyncResult)request.BeginGetResponse(RegisterCallback, request);
            }
            catch (WebException)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Busy.IsEnabled = false;
                    ConnectionResult.Show(Languages.AppResources.operationsOnRegistration_ConnectionProblem, Languages.AppResources.operationsOnRegistration_CheckConnection, MessageBoxButton.OK);
                });
            }
            catch (UriFormatException)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Busy.IsEnabled = false;
                    ConnectionResult.Show(Languages.AppResources.operationsOnRegistration_ConnectionProblem, Languages.AppResources.operationsOnRegistration_ValidServerAddress, MessageBoxButton.OK);
                });
            }
        }

        /// <summary>
        /// Callback used to process server response.
        /// </summary>
        /// <param name="result">Server response state.</param>
        public void RegisterCallback(IAsyncResult result)
        {
            try
            {
                var request = (WebRequest)result.AsyncState;
                var response = request.EndGetResponse(result);

                ResponseType responseStatus;
                string serverOutput = string.Empty;

                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var contents = reader.ReadToEnd();

                    responseStatus = ProccessServerOutput(contents, Encoding.UTF8);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Busy.IsEnabled = false;
                        DisplayRegistrationResult(responseStatus);
                    });
                }
            }
            catch (WebException)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Busy.IsEnabled = false;
                    ConnectionResult.Show(Languages.AppResources.operationsOnRegistration_ConnectionProblem, Languages.AppResources.operationsOnRegistration_CheckConnectionServerAddress, MessageBoxButton.OK);
                });
            }
        }

        private void DisplayRegistrationResult(ResponseType serverResponse)
        {
            switch (serverResponse)
            {
                case ResponseType.Successful:
                    ConnectionResult.Show(Languages.AppResources.operationsOnRegistration_RegistrationSuccess, Languages.AppResources.operationsOnRegistration_RegistratedSuccessfully, MessageBoxButton.OK);
                    SaveRegistrationData(RegistrationInstance);
                    try
                    {
                        LeaveRegistrationEvent(this, new EventArgs());
                    }
                    catch (NullReferenceException)
                    {
                    }
                    break;
                case ResponseType.Failed:
                    ConnectionResult.Show(Languages.AppResources.operationsOnRegistration_RegistrationFailed, Languages.AppResources.operationsOnRegistration_PhoneNotValidIMEIExist, MessageBoxButton.OK);
                    break;
                case ResponseType.ResponseIsNull:
                    ConnectionResult.Show(Languages.AppResources.operationsOnRegistration_RegistrationFailed, Languages.AppResources.operationsOnRegistration_ServerNotResponding, MessageBoxButton.OK);
                    break;
                default:
                    break;
            }
        }

        private string CreateRequestUrl(string phoneNumber, string imei, string serverUrl)
        {
            Random rand = new Random();
            return string.Format("{0}RegisterIMEI?msisdn=%2b{1}&imei={2}&nocache={3}", serverUrl, phoneNumber, imei, rand.Next(50));
        }

        private ResponseType ProccessServerOutput(string input, Encoding enc)
        {
            if (input != "")
            {
                byte[] bytes = enc.GetBytes(input);

                StringBuilder builder = new StringBuilder();
                foreach (var i in bytes)
                    builder.Append(i.ToString());

                string temp = builder.ToString();
 
                if ((temp == "0001") || (temp == "0002"))
                {
                    return ResponseType.Successful;
                }
                else
                {
                    return ResponseType.Failed;
                }
            }
            else
            {
                return ResponseType.ResponseIsNull;
            }
        }

        private void SaveRegistrationData(Registration registration)
        {
            OperationsOnSettings.Instance.IMEI = registration.Imei; 
            OperationsOnSettings.Instance.ServerURL = registration.ServerUrl; 
        }
        
        /// <summary>
        /// Runs final data checks before registration process can be started.
        /// </summary>
        public void RegistrationRunner()
        {
            RegistrationInstance.ImeiIsValid = RegistrationInstance.ImeiIsValid && (RegistrationInstance.Imei.Length != 0);
            RegistrationInstance.PhoneNumberIsValid = RegistrationInstance.PhoneNumberIsValid && (RegistrationInstance.PhoneNumber.Length != 0);
            RegistrationInstance.ServerUrlIsValid = RegistrationInstance.ServerUrlIsValid && (RegistrationInstance.ServerUrl.Length != 0);

            if (RegistrationInstance.ImeiIsValid && RegistrationInstance.PhoneNumberIsValid && RegistrationInstance.ServerUrlIsValid )
            {
                Register();
            }
        }
    }
}

