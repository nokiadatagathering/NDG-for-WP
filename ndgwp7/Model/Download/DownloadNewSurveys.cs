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
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using com.comarch.mobile.ndg.Settings.Model;

namespace com.comarch.mobile.ndg.Model.Download
{
    /// <summary>
    /// Class contains data and methods used to download surveys. 
    /// </summary>
    public class DownloadNewSurveys
    {
        private OperationsOnListOfSurveys _operationsOnList;
        private List<SurveyBasicInfo> _surveysToDownload;
        private DownloadSurveysStatus _downloadStatus;
        private WebRequest _request;

        private string _downloadUrl;
        private string _ackUrl;

        private bool _aborted;
        /// <summary>
        /// Terminates WebRequest, stops download of surveys.
        /// </summary>
        public void Abort()
        {
            if ((_request != null) && _downloadStatus.CanCancel)
            {
                _aborted = true;
                _downloadStatus.ProgressBar.IsEnabled = false;
                ((HttpWebRequest)_request).Abort();
            }
        }
        /// <summary>
        /// Allows you to initialize all necessary data members.
        /// </summary>
        /// <param name="operationsOnList">Instance of <see cref="OperationsOnListOfSurveys" /> class.</param>
        /// <param name="surveysToDownload">List of surveys downloaded with <see cref="CheckForNewSurveyList" /> class.</param>
        /// <param name="downloadStatus">Instance of <see cref="DownloadSurveysStatus" /> class.</param>
        public DownloadNewSurveys(OperationsOnListOfSurveys operationsOnList, List<SurveyBasicInfo> surveysToDownload, DownloadSurveysStatus downloadStatus)
        {
            _operationsOnList = operationsOnList;
            _surveysToDownload = surveysToDownload;
            _downloadStatus = downloadStatus;

            string strIMEI = OperationsOnSettings.Instance.IMEI;
            string strBaseURL = OperationsOnSettings.Instance.ServerURL; 

            Random rand = new Random();
            _downloadUrl = string.Format("{0}ReceiveSurveys?do=download&imei={1}&nocache={2}", strBaseURL, strIMEI, rand.Next(50));
            _ackUrl = string.Format("{0}ReceiveSurveys?do=ack&imei={1}&nocache={2}", strBaseURL, strIMEI, rand.Next(50));
        }

        /// <summary>
        /// Begins download process.
        /// </summary>
        public void Download()
        {
            _downloadStatus.ProgressBar.Information = Languages.AppResources.downloadNewSurveys_Downloading;
            _downloadStatus.ProgressBar.IsEnabled = true;
            _downloadStatus.CanCancel = true;
            _downloadStatus.IsCanceled = false;
            try
            {
                _request = WebRequest.Create(_downloadUrl);
                var result = (IAsyncResult)_request.BeginGetResponse(DownloadCallback, _request);
            }
            catch (WebException)
            {
                _downloadStatus.Message.Show(Languages.AppResources.downloadNewSurveys_CantDownloading);
            }
        }

        /// <summary>
        /// Callback used to process server response for download WebRequest.
        /// </summary>
        /// <param name="result">Server response state.</param>
        protected void DownloadCallback(IAsyncResult result)
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
                    contents = contents.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>", "");
                    contents = string.Format("<surveys>{0}</surveys>", contents);
                    XDocument documentXML = XDocument.Parse(contents);
                    XElement root = documentXML.Element("surveys");

                    var surveys = from survey in root.Descendants("survey") select survey;

                    List<XDocument> surveyXDocuments = new List<XDocument>();

                    if (_downloadStatus.IsCanceled)
                    {
                        return;
                    }
                    foreach (XElement survey in surveys)
                    {
                        if (_operationsOnList.IsSurveySaved(survey.Attribute("id").Value))
                        {
                            continue;
                        }
                        XDocument doc = new XDocument();
                        doc.Add(survey);
                        surveyXDocuments.Add(doc);
                    }
                    if (_surveysToDownload.Count == surveyXDocuments.Count)
                    {
                        bool canISave = true;
                        foreach (XDocument doc in surveyXDocuments)
                        {
                            XElement surveyRoot = doc.Element("survey");
                            var isInListToDownload = from survey in _surveysToDownload where survey.SurveyId == surveyRoot.Attribute("id").Value select survey;
                            if (isInListToDownload.Count<SurveyBasicInfo>() != 1)
                            {
                                canISave = false;
                                break;
                            }
                        }
                        if (canISave)
                        {
                            if (_downloadStatus.IsCanceled)
                            {
                                return;
                            }
                            _downloadStatus.CanCancel = false;
                            try
                            {
                                foreach (XDocument surveyDocument in surveyXDocuments)
                                {
                                    SaveSurvey(surveyDocument);
                                }
                                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    _operationsOnList.Add(_surveysToDownload);
                                });
                                SendAcknowledge();
                            }
                            catch (IsolatedStorageException)
                            {
                                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                                   {
                                       _downloadStatus.ProgressBar.IsEnabled = false;
                                       _downloadStatus.Message.Show(Languages.AppResources.downloadNewSurveys_SavingProblem);
                                       _downloadStatus.CanCancel = true;
                                   });
                            }
                        }
                        else
                        {
                           System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                           {
                               _downloadStatus.ProgressBar.IsEnabled = false;
                               _downloadStatus.Message.Show(Languages.AppResources.downloadNewSurveys_WrongResponse);
                           });
                        }
                    }
                    else
                    {
                        System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                _downloadStatus.ProgressBar.IsEnabled = false;
                                _downloadStatus.Message.Show(Languages.AppResources.downloadNewSurveys_MoreSurveys);
                            });
                    }
                }
            }

            catch (WebException)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _downloadStatus.ProgressBar.IsEnabled = false;
                        _downloadStatus.Message.Show(Languages.AppResources.checkForNewSurveyList_ServerNotFound);
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
        /// <summary>
        /// Sends acknowledge to server that survey had been downloaded properly.
        /// </summary>
        protected void SendAcknowledge()
        {
            try
            {
                var request = WebRequest.Create(_ackUrl);
                var result = (IAsyncResult)request.BeginGetResponse(AcknowledgeCallback, request);
            }
            catch (WebException)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                   {
                       _downloadStatus.ProgressBar.IsEnabled = false;
                       _downloadStatus.Message.Show(Languages.AppResources.downloadNewSurveys_CantConfirm);
                       _downloadStatus.CanCancel = true;
                   });
            }
        }
        /// <summary>
        /// Callback used to process server response for acknowledge WebRequest.
        /// </summary>
        /// <param name="result">Server response state.</param>
        protected void AcknowledgeCallback(IAsyncResult result)
        {
            try
            {
                var request = (WebRequest)result.AsyncState;
                var response = (HttpWebResponse)request.EndGetResponse(result);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _downloadStatus.ProgressBar.IsEnabled = false;
                        _downloadStatus.GoBack();
                        _downloadStatus.CanCancel = true;
                    });
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           _downloadStatus.ProgressBar.IsEnabled = false;
                           _downloadStatus.Message.Show(Languages.AppResources.checkForNewSurveyList_ServerNotFound);
                       });
                }
                else
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           _downloadStatus.CanCancel = true;
                           _downloadStatus.ProgressBar.IsEnabled = false;
                           _downloadStatus.Message.Show(Languages.AppResources.downloadNewSurveys_CantConfirm);
                       });
                }
            }
            catch (WebException)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                   {
                       _downloadStatus.CanCancel = true;
                       _downloadStatus.ProgressBar.IsEnabled = false;
                       _downloadStatus.Message.Show(Languages.AppResources.checkForNewSurveyList_ServerNotFound);
                   });
            }
        }

        private void SaveSurvey(XDocument documentXML)
        {
            string surveyId = documentXML.Element("survey").Attribute("id").Value;

            using (IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isolatedStorage.DirectoryExists("surveys"))
                {
                    isolatedStorage.CreateDirectory("surveys");
                }
                string surveyFilePath = System.IO.Path.Combine("surveys", string.Format("{0}.xml", surveyId));
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(surveyFilePath, FileMode.Create, isolatedStorage))
                {
                    documentXML.Save(isoStream);
                }

                string directoryPath = System.IO.Path.Combine("surveys", surveyId);
                isolatedStorage.CreateDirectory(directoryPath);
            }

        }
    }
}
