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
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using com.comarch.mobile.ndg.Model.Download;

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Contains methods used for operations on list of surveys.
    /// </summary>
    public class OperationsOnListOfSurveys
    {
        private ObservableCollection<SurveyBasicInfo> _list;
        private DownloadListStatus _downloadStatus;

        /// <summary>
        /// Allows you to initialize all necessary data objects.
        /// </summary>
        /// <param name="list">List of surveys.</param>
        /// <param name="downloadStatus">Instance of <see cref="DownloadListStatus"/> class.</param>
        public OperationsOnListOfSurveys(ObservableCollection<SurveyBasicInfo> list, DownloadListStatus downloadStatus)
        {
            _list = list;
            _downloadStatus = downloadStatus;
        }

        /// <summary>
        /// Reads list of surveys from listOfSurveys.xml file.
        /// </summary>
        public void Read()
        {
            _list.Clear();
            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string listOfSurveysFilePath = System.IO.Path.Combine("surveys", "listOfSurveys.xml");
                    if (!isoStore.DirectoryExists("surveys") || !isoStore.FileExists(listOfSurveysFilePath))
                    {
                        return;
                    }
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(listOfSurveysFilePath, FileMode.Open, isoStore))
                    {
                        XDocument doc = XDocument.Load(isoStream);
                        XElement root = doc.Element("surveys");
                        
                        var surveys = from survey in root.Descendants("survey")
                                      select new SurveyBasicInfo()
                                      {
                                          Name = survey.Attribute("title").Value, 
                                          SurveyId = survey.Attribute("id").Value,
                                          IsFavorite = Convert.ToBoolean(survey.Attribute("favorite").Value)
                                      };

                        foreach (SurveyBasicInfo survey in surveys)
                        {
                            _list.Add(survey);
                        }
                    }
                }
            }
            catch (XmlException)
            { 
            }
            catch (IsolatedStorageException)
            {
            }
        }
 
        // Saves list of surveys on the phone, throws IsolatedStorageException 
        private void Write()
        {
            XDocument doc = PrepareXDocument();
            using (IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string listOfSurveysFilePath = System.IO.Path.Combine("surveys", "listOfSurveys.xml");

                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(listOfSurveysFilePath, FileMode.Create, isolatedStorage))
                {
                    doc.Save(isoStream);
                }
            }
        }

        // Makes  xml document with list of surveys 
        private XDocument PrepareXDocument()
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("surveys");

            foreach (SurveyBasicInfo survey in _list)
            {
                root.Add(new XElement("survey", new XAttribute("id", survey.SurveyId), new XAttribute("title", survey.Name), new XAttribute("favorite", survey.IsFavorite)));
            }
            doc.Add(root);
            return doc;
        }

        /// <summary>
        /// Adds list of surveys to list saved in listOfSurveys.xml file.
        /// </summary>
        /// <param name="listOfNewSurveys"></param>
        public void Add(List<SurveyBasicInfo> listOfNewSurveys)
        {
            Read();
            foreach (SurveyBasicInfo newSurvey in listOfNewSurveys)
                _list.Add(newSurvey);
            Write();
            _list.Clear();
        }

        /// <summary>
        /// Terminates downloader. Stops WebRequest responsible for list of surveys download.
        /// </summary>
        public void Abort()
        {
            if (_downloader != null)
            {
                _downloader.Abort();
            }
        }

        private CheckForNewSurveyList _downloader;
        /// <summary>
        /// Uses <see cref="CheckForNewSurveyList"/> class to download list of surveys.
        /// </summary>
        public void CheckForNewSurveys()
        {
            _list.Clear();
            _downloader = new CheckForNewSurveyList(_list, _downloadStatus, this);
            _downloader.DownloadList();
        }

        /// <summary>
        /// Deletes single survey from IsolatedStorage.
        /// </summary>
        /// <param name="surveyId">Id of survey you want to delete.</param>
        public void Delete(string surveyId)
        {
            Read();
            IEnumerable<SurveyBasicInfo> surveys;
            surveys = from SurveyBasicInfo in _list where SurveyBasicInfo.SurveyId == surveyId select SurveyBasicInfo;
            try
            {
                OperationsOnListOfResults resultsOperations = new OperationsOnListOfResults(surveyId);
                resultsOperations.DeleteAllResults();
                SurveyBasicInfo survey = surveys.First<SurveyBasicInfo>();
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = System.IO.Path.Combine("surveys", string.Format("{0}.xml", survey.SurveyId));
                    string directoryPath = System.IO.Path.Combine("surveys", survey.SurveyId);
                    string listOfResultPath = System.IO.Path.Combine(directoryPath, "listOfResults.xml");
                    try
                    {
                        isoStore.DeleteFile(filePath);
                        isoStore.DeleteFile(listOfResultPath);
                        isoStore.DeleteDirectory(directoryPath);
                    }
                    catch (IsolatedStorageException) { /*that means that this files doesn't exists*/ }
                }
                _list.Remove(survey);
                Write();

            }
            catch (InvalidOperationException) { }
        }

        /// <summary>
        /// Marks survey as favorite.
        /// </summary>
        /// <param name="surveyId">Id of survey you want to add to favorites.</param>
        public void AddToFavorite(string surveyId)
        {
            ChangeFavorite(surveyId, true);
        }

        /// <summary>
        /// Unmarks survey as favorite.
        /// </summary>
        /// <param name="surveyId">Id of survey you want to remove from favorites.</param>
        public void RemoveFromFavorite(string surveyId)
        {
            ChangeFavorite(surveyId, false);
        }

        private void ChangeFavorite(string surveyId, bool addSurvey)
        {
            Read();
            IEnumerable<SurveyBasicInfo> surveys;
            surveys = from SurveyBasicInfo in _list where SurveyBasicInfo.SurveyId == surveyId select SurveyBasicInfo;
            try
            {
                SurveyBasicInfo survey = surveys.First<SurveyBasicInfo>();
                int index = _list.IndexOf(survey);
                _list.Remove(survey);
                survey.IsFavorite = addSurvey;
                _list.Insert(index, survey);
                Write();
            }
            catch (InvalidOperationException)
            {
            }
        }

        /// <summary>
        /// Represents list of surveys as string xml.
        /// </summary>
        public string StateDictionaryData
        {
            get
            {
                XDocument doc = PrepareXDocument();
                string output = doc.ToString();
                return output;
            }
            set
            {
                _list.Clear();
                XDocument doc = XDocument.Parse(value);
                XElement root = doc.Element("surveys");

                var surveys = from survey in root.Descendants("survey")
                              select new SurveyBasicInfo()
                              {
                                  Name = survey.Attribute("title").Value,
                                  SurveyId = survey.Attribute("id").Value,
                                  IsFavorite = Convert.ToBoolean(survey.Attribute("favorite").Value)
                              };

                foreach (SurveyBasicInfo survey in surveys)
                {
                    _list.Add(survey);
                }
            }
        }
        
        /// <summary>
        /// Checks if survey data exists in IsolatedStorage.
        /// </summary>
        /// <param name="surveyId">Id of survey you want to check.</param>
        /// <returns>True if survey exists, false in any other case.</returns>
        public bool IsSurveySaved(string surveyId)
        {
            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string listOfSurveysFilePath = System.IO.Path.Combine("surveys", "listOfSurveys.xml");
                    if (!isoStore.DirectoryExists("surveys") || !isoStore.FileExists(listOfSurveysFilePath))
                    {
                        return false;
                    }
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(listOfSurveysFilePath, FileMode.Open, isoStore))
                    {
                        XDocument doc = XDocument.Load(isoStream);
                        XElement root = doc.Element("surveys");

                        var surveys = from survey in root.Descendants("survey") where survey.Attribute("id").Value == surveyId
                                      select survey;

                        if (surveys.Count<XElement>() >= 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (XmlException)
            {
                return false;
            }
            catch (IsolatedStorageException)
            {
                return false;
            }
        }
    }
}
