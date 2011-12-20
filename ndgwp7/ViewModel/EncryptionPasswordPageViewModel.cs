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
using System.Security.Cryptography;
using com.comarch.mobile.ndg.Model;
using com.comarch.mobile.ndg.Model.SurveyForms;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Class stores methods used during communication between model and view on EncryptionPasswordPage.
    /// </summary>
    public class EncryptionPasswordPageViewModel
    {
        /// <summary>
        /// Represent password used while encrypting results.
        /// </summary>
        public string EncryptionPassword { get; set; }

        /// <summary>
        /// Method to save encryption password in application (until it closes).
        /// </summary>
        /// <returns>Returns TRUE if password is saved or FALSE when saving operation has failed.</returns>
        public bool SavePassword()
        {

            if (App.AppDictionary.ContainsKey("EncryptionPassword"))
                App.AppDictionary["EncryptionPassword"] = EncryptionPassword;
            else
                App.AppDictionary.Add("EncryptionPassword", EncryptionPassword);

            if (!CheckIfPasswordIsCorrect())
            {
                App.AppDictionary["EncryptionPassword"] = string.Empty;
                return false;
            }
            
            return true;
        }
        private bool CheckIfPasswordIsCorrect()
        {
            if (!String.IsNullOrEmpty(EncryptionPassword))
            {
                ObservableCollection<SurveyBasicInfo> surveys = new ObservableCollection<SurveyBasicInfo>();
                OperationsOnListOfSurveys operations = new OperationsOnListOfSurveys(surveys,new DownloadListStatus());
                operations.Read();
                foreach (SurveyBasicInfo survey in surveys)
                {
                    ObservableCollection<ResultBasicInfo> results = new ObservableCollection<ResultBasicInfo>();
                    OperationsOnListOfResults resultsOperator = new OperationsOnListOfResults(survey.SurveyId, results);
                    resultsOperator.ReadList();
                    foreach (ResultBasicInfo result in results)
                    {
                        Survey surveyModel = new Survey();
                        surveyModel.Id = survey.SurveyId;
                        surveyModel.ResultInfo = result;
                        try
                        {
                            surveyModel.GetSavedDocument();
                            return true;
                        }
                        catch (CryptographicException)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
