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
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using com.comarch.mobile.ndg.Settings.Model;

namespace com.comarch.mobile.ndg.Model.Download
{
    /// <summary>
    /// Class contains data and methods used to download survey list. 
    /// </summary>
    public class CheckForNewSurveyList
    {
        private ObservableCollection<SurveyBasicInfo> _list;
        private DownloadListStatus _downloadStatus;
        private OperationsOnListOfSurveys _operationsOnListOfSurveys;
        private WebRequest _request;

        /// <summary>
        /// Allows you to initialize all necessary data members.
        /// </summary>
        /// <param name="list">List of current saved surveys.</param>
        /// <param name="downloadStatus">Instance of <see cref="DownloadListStatus" /> class.</param>
        /// <param name="operations">Instance of <see cref="OperationsOnListOfSurveys" /> class.</param>
        public CheckForNewSurveyList(ObservableCollection<SurveyBasicInfo> list, DownloadListStatus downloadStatus, OperationsOnListOfSurveys operations)
        {
            _list = list;
            _downloadStatus = downloadStatus;
            _operationsOnListOfSurveys = operations;
        }

        private bool _aborted;
        /// <summary>
        /// Terminates WebRequest, stops download of list of surveys.
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
        /// Begins download process.
        /// </summary>
        public void DownloadList()
        {
            _downloadStatus.ProgressBar.Information = Languages.AppResources.checkForNewSurveyList_Downloading;
            _downloadStatus.ProgressBar.IsEnabled = true;
            _downloadStatus.IsDownloadButtonEnabled = false;

            string strIMEI = OperationsOnSettings.Instance.IMEI;
            string strBaseURL = OperationsOnSettings.Instance.ServerURL;
            string strURL = string.Empty;
            Random rand = new Random();
            strURL = string.Format("{0}ReceiveSurveys?do=list&imei={1}&nocache={2}", strBaseURL, strIMEI, rand.Next(50)); // the last parameter "nocache" is optional, added to avoid caching

            try
            {
                _request = WebRequest.Create(strURL);
                
                var result = (IAsyncResult)_request.BeginGetResponse(DownloadCallback, _request);
            }
            catch (WebException)
            {
                _downloadStatus.StatusMessage = Languages.AppResources.checkForNewSurveyList_DownloadFailed;
                _downloadStatus.Show = true;
                _downloadStatus.Message.Show(Languages.AppResources.checkForNewSurveyList_DownloadFailed);
                _downloadStatus.ProgressBar.IsEnabled = false;
            }
        }
        /// <summary>
        /// Callback used to process server response.
        /// </summary>
        /// <param name="result">Server response state.</param>
        public void DownloadCallback(IAsyncResult result)
        {
            if (_aborted)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _downloadStatus.ProgressBar.IsEnabled = false;
                });
                _aborted = false;
                return;
            }
            try
            {
                _request = (WebRequest)result.AsyncState;
                var response = (WebResponse)_request.EndGetResponse(result);
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var contents = reader.ReadToEnd();
                    XDocument documentXML = XDocument.Parse(contents);

                    var root = documentXML.Element("surveys");
                    var surveys = root.Elements("survey");

                    int howMany = 0;

                    foreach (XElement xSurvey in surveys)
                    {
                        if (_operationsOnListOfSurveys.IsSurveySaved(xSurvey.Attribute("id").Value))
                        {
                            continue;
                        }
                        ++howMany;
                        SurveyBasicInfo survey = new SurveyBasicInfo() { Name = xSurvey.Attribute("title").Value, SurveyId = xSurvey.Attribute("id").Value };
                        System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            _list.Add(survey);
                        });
                    }
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _downloadStatus.ProgressBar.IsEnabled = false;
                        if (howMany == 0)
                        {
                            _downloadStatus.StatusMessage = Languages.AppResources.checkForNewSurveyList_ZeroNewSurvey;
                            _downloadStatus.Show = true;
                        }
                        else
                        {
                            if (howMany == 1)
                            {
                                _downloadStatus.Message.Show(Languages.AppResources.checkForNewSurveyList_OneNewSurvey);
                            }
                            else
                            {
                                _downloadStatus.Message.Show(string.Format(Languages.AppResources.checkForNewSurveyList_ManyNewSurveys, howMany));
                            }
                            _downloadStatus.Show = false;
                            _downloadStatus.IsDownloadButtonEnabled = true;
                        }
                    });

                }
            }
            catch (WebException e)
            {
                var status = ((HttpWebResponse)e.Response).StatusCode;
                string message = string.Empty;
                switch (status)
                {
                    case HttpStatusCode.NotFound:
                        message = Languages.AppResources.checkForNewSurveyList_ServerNotFound;
                        break;
                    default:
                        message = Languages.AppResources.checkForNewSurveyList_ServerError;
                        break;
                }

                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            _downloadStatus.ProgressBar.IsEnabled = false;
                            _downloadStatus.Message.Show(message);
                        });
            }
            catch (XmlException)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _downloadStatus.ProgressBar.IsEnabled = false;
                    _downloadStatus.Message.Show(Languages.AppResources.checkForNewSurveyList_ServerIncompleteResponse);
                });
            }
        }
    }
}
