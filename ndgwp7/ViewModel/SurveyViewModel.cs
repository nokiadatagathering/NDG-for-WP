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
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using com.comarch.mobile.ndg.BusyIndicator;
using com.comarch.mobile.ndg.MessageDialog;
using com.comarch.mobile.ndg.Model;
using com.comarch.mobile.ndg.Model.SurveyForms;
using com.comarch.mobile.ndg.Settings.Model;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Class stores methods used during communication between model and view on SurveyPage.
    /// </summary>
    public class SurveyViewModel
    {
        /// <summary>
        /// Represents list of saved results.
        /// </summary>
        public ObservableCollection<ResultBasicInfo> ListOfResults;
        
        /// <summary>
        /// Represents ID of survey.
        /// </summary>
        public string SurveyId { get; set; }
        
        /// <summary>
        /// Represents favorite status of survey.
        /// </summary>
        public string IsFavorite { get; set; }
        
        /// <summary>
        /// Represents instance of <see cref="ProcessingBar"/> class.
        /// </summary>
        public ProcessingBar ProgressBar { get; set; }
        
        private SendResult _resultSender;
        
        /// <summary>
        /// Represents instance of <see cref="DialogBox"/> class.
        /// </summary>
        public DialogBox Message { get; set; }
        
        /// <summary>
        /// Represents information (true/false) about sending result on server.
        /// </summary>
        public bool SendingInProgress { get; set; }

        private OperationsOnListOfResults _operations;

        /// <summary>
        /// Constructor which initializes properties and using input surveyId like ID of survey.
        /// </summary>
        /// <param name="surveyId">ID of survey.</param>
        public SurveyViewModel(string surveyId)
        {
            ListOfResults = new ObservableCollection<ResultBasicInfo>();
            _operations = new OperationsOnListOfResults(surveyId, ListOfResults);
            SurveyId = surveyId;
            ProgressBar = new ProcessingBar();
            Message = new DialogBox();
            _resultSender = new SendResult();
            _resultSender.SendingCompleted += (object sender, EventArgs args) =>
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    SendResult.SendingEventArgs.SendingStatus status = (args as SendResult.SendingEventArgs).Status;
                    string resultId = (args as SendResult.SendingEventArgs).ResultId;
                    switch (status)
                    {
                        case Model.SendResult.SendingEventArgs.SendingStatus.Sent:
                            Message.Show(Languages.AppResources.surveyViewModel_sendingCompleted);
                            _operations.MarkResultAsSent(resultId);
                            break;
                        case Model.SendResult.SendingEventArgs.SendingStatus.ServerError:
                            Message.Show(Languages.AppResources.surveyViewModel_serverError);
                            break;
                        case Model.SendResult.SendingEventArgs.SendingStatus.UnknownError:
                            Message.Show(Languages.AppResources.surveyViewModel_unknownError);
                            break;
                        case Model.SendResult.SendingEventArgs.SendingStatus.Canceled:
                            break;
                    }

                    ProgressBar.IsEnabled = false;
                    SendingInProgress = false;
                });
            };
        }

        /// <summary>
        /// Reloads list of results.
        /// </summary>
        public void ReloadList()
        {
            _operations.ReadList();
        }

        /// <summary>
        /// Deletes result from list (and from IsolatedStoraged).
        /// </summary>
        /// <param name="result">Information about result which should be delete.</param>
        public void DeleteResult(ResultBasicInfo result)
        {
            _operations.DeleteResult(result.Id);
        }
        
        /// <summary>
        /// Aborts saving result process.
        /// </summary>
        public void AbortSaving()
        {
            if (SendingInProgress)
            {
                _resultSender.AbortSaving();
            }
        }
        
        /// <summary>
        /// Unmarks sent results.
        /// </summary>
        /// <param name="result">Information about result which should be unmarked.</param>
        public void UnmarkSent(ResultBasicInfo result)
        {
            _operations.UnmarkSentResult(result.Id);
        }
       
        /// <summary>
        /// Sends result to server.
        /// </summary>
        /// <param name="selectedListBoxItem">Information about result which should be send.</param>
        public void SendResult(ResultBasicInfo selectedListBoxItem)
        {
            ProgressBar.Information = Languages.AppResources.surveyViewModel_sendingProgressTitle;
            ProgressBar.IsEnabled = true;
            SendingInProgress = true;
            _resultSender.Send(SurveyId, selectedListBoxItem);
        }

#if DEBUG
        /// <summary>
        /// Method to prepare new results with right date and GPS location.
        /// </summary>
        public void PreapreResultsForTests()
        {
            DateOperations operationsOnDate = new DateOperations();

            // Wynik 1_1: Data utworzenia: 2/2/2010,  szerokość: 52, długość: 21 (Warszawa)
            Survey survey = new Survey();
            survey.Display(Convert.ToInt32(SurveyId));
            survey.ResultInfo.Title = "test1";
            survey.ResultInfo.Latitude = "52";
            survey.ResultInfo.Longitude = "21";
            XDocument documentXML = survey.PrepareResultDocument();
            survey.ResultInfo.Time = operationsOnDate.DateTimeToMiliseconds(new DateTime(2010, 2, 2)).ToString();
            
            SavetTestResult(survey, documentXML);

            // Wynik 1_2: Data utworzenia: 2/24/2010, bez GPS
            Survey survey1 = new Survey();
            survey1.Display(Convert.ToInt32(SurveyId));
            survey1.ResultInfo.Title = "test2";
            XDocument documentXML1 = survey1.PrepareResultDocument();
            survey1.ResultInfo.Time = operationsOnDate.DateTimeToMiliseconds(new DateTime(2010, 2, 24)).ToString();
            if (documentXML1.Element("latitude") != null)
                documentXML1.Element("latitude").Remove();

            if (documentXML1.Element("longitude") != null)
                documentXML1.Element("longitude").Remove();

            SavetTestResult(survey1, documentXML1);

            // Wynik 1_3: Data utworzenia: 5/10/2010,  szerokość: 50, długość: 20 (Kraków)
            Survey survey2 = new Survey();
            survey2.Display(Convert.ToInt32(SurveyId));
            survey2.ResultInfo.Title = "test3";
            survey2.ResultInfo.Latitude = "50";
            survey2.ResultInfo.Longitude = "20";
            XDocument documentXML2 = survey2.PrepareResultDocument();
            survey2.ResultInfo.Time = operationsOnDate.DateTimeToMiliseconds(new DateTime(2010, 5, 10)).ToString();
            SavetTestResult(survey2, documentXML2);

            // Wynik 2_1: Data utworzenia: 2/2/2011,  szerokość: 50, długość: 19 (Katowice)
            Survey survey3 = new Survey();
            survey3.Display(Convert.ToInt32(SurveyId));
            survey3.ResultInfo.Title = "test4";
            survey3.ResultInfo.Latitude = "50";
            survey3.ResultInfo.Longitude = "19";
            XDocument documentXML3 = survey3.PrepareResultDocument();
            survey3.ResultInfo.Time = operationsOnDate.DateTimeToMiliseconds(new DateTime(2011, 2, 2)).ToString();
            SavetTestResult(survey3, documentXML3);

            // Wynik 2_2: Data utworzenia: 10/2/2011, szerokość: 50, długość: 20 (Kraków)
            Survey survey4 = new Survey();
            survey4.Display(Convert.ToInt32(SurveyId));
            survey4.ResultInfo.Title = "test5";
            survey4.ResultInfo.Latitude = "50";
            survey4.ResultInfo.Longitude = "20";
            XDocument documentXML4 = survey4.PrepareResultDocument();
            survey4.ResultInfo.Time = operationsOnDate.DateTimeToMiliseconds(new DateTime(2011, 10, 2)).ToString();
            SavetTestResult(survey4, documentXML4);

            // Wynik 3_1: Data utworzenia: 10/2/2011,  szerokość: 51, długość: 17 (Wrocław)
            Survey survey5 = new Survey();
            survey5.Display(Convert.ToInt32(SurveyId));
            survey5.ResultInfo.Title = "test6";
            survey5.ResultInfo.Latitude = "51";
            survey5.ResultInfo.Longitude = "17";
            XDocument documentXML5 = survey5.PrepareResultDocument();
            survey5.ResultInfo.Time = operationsOnDate.DateTimeToMiliseconds(new DateTime(2011, 10, 2)).ToString();
            
            SavetTestResult(survey5, documentXML5);
        }

        private void SavetTestResult(Survey survey, XDocument documentXML)
        {
            String dataToSave;

            if ((bool)OperationsOnSettings.Instance.IsEncryptionEnabled)
            {
                AESEncryption encrypter = new AESEncryption();
                dataToSave = encrypter.Encrypt(documentXML.ToString(), App.AppDictionary["EncryptionPassword"] as String, "qwhmvbzx");
            }
            else
            {
                dataToSave = documentXML.ToString();
            }

            using (IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                String directoryPath = String.Format("surveys/{0}", survey.Id);
                string resultFilePath = System.IO.Path.Combine(directoryPath, String.Format("r_{0}.xml", survey.ResultInfo.Id));
                if (!isolatedStorage.DirectoryExists(directoryPath))
                {
                    isolatedStorage.CreateDirectory(directoryPath);
                }

                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(resultFilePath, FileMode.Create, isolatedStorage))
                {
                    StreamWriter writer = new StreamWriter(isoStream);
                    writer.Write(dataToSave);
                    writer.Close();
                }
            }

            OperationsOnListOfResults operationsOnListOfResults = new OperationsOnListOfResults(SurveyId);
            operationsOnListOfResults.Add(survey.ResultInfo);
        }
#endif
    }
}
