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
using System.Net;
using System.Windows;
using com.comarch.mobile.ndg.BusyIndicator;
using com.comarch.mobile.ndg.MessageDialog;
using com.comarch.mobile.ndg.Settings.Model;

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Class responsible for testing connection with server. Uses PostResults service to indicate if server is responding.
    /// </summary>
    public class TestConnection
    {
        /// <summary>
        /// Represents instance of <see cref="DialogBox"/> class.
        /// </summary>
        public DialogBox Message { get; set; }
        /// <summary>
        /// Represents instance of <see cref="ProcessingBar"/> class.
        /// </summary>
        public ProcessingBar Busy { get; set; }

        private WebRequest _request;
        private bool _aborted;

        /// <summary>
        /// Initializes all necessary data objects.
        /// </summary>
        public TestConnection()
        {
            Message = new DialogBox();
            Busy = new ProcessingBar(Languages.AppResources.operationsOnRegistration_ProcessingBarInformation);
        }

        /// <summary>
        /// Begins test connection process.
        /// </summary>
        public void Ping()
        {
            _aborted = false;
            Busy.IsEnabled = true;
            Random rand = new Random();
            _request = WebRequest.Create(string.Format("{0}PostResults?nocache={1}", OperationsOnSettings.Instance.ServerURL, rand.Next(50))); //Checking if PostResults Service is responding
            var result = (IAsyncResult)_request.BeginGetResponse(PingCallback, _request);
        }

        /// <summary>
        /// Terminates WebRequest. Stops test connection process.
        /// </summary>
        public void Abort()
        {
            if (_request != null)
            {
                _aborted = true;
                ((HttpWebRequest)_request).Abort();
            }
        }

        /// <summary>
        /// Callback used to process server response.
        /// </summary>
        /// <param name="asynchronousResult">Server response state.</param>
        public void PingCallback(IAsyncResult asynchronousResult)
        {
            if (_aborted)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Busy.IsEnabled = false;
                });
                return;
            }
            try
            {
                _request = (WebRequest)asynchronousResult.AsyncState;
                HttpWebResponse response = (HttpWebResponse)_request.EndGetResponse(asynchronousResult);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(()=>
                    {
                        Busy.IsEnabled = false;
                        Message.Show(Languages.AppResources.testConnection_ServerOk);
                    });
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(()=>
                    {
                        Busy.IsEnabled = false;
                        Message.Show(Languages.AppResources.operationsOnRegistration_ServerNotResponding);
                    });
                }
                response.Close();
            }
            catch (WebException)
            {
                Deployment.Current.Dispatcher.BeginInvoke(()=>
                {
                    Busy.IsEnabled = false;
                    Message.Show(Languages.AppResources.operationsOnRegistration_ServerNotResponding);
                });
            }
        }
    }
}
