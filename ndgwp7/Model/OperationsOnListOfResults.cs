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
using System.Linq;
using System.Xml.Linq;

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Contains all methods used for operations on list of survey results.
    /// </summary>
    public class OperationsOnListOfResults
    {
        private string _surveyId;

        /// <summary>
        /// Allows you to initialize class data with survey id and list of results.
        /// </summary>
        /// <param name="surveyId">Survey identification number.</param>
        /// <param name="list">List of results that survey contains.</param>
        public OperationsOnListOfResults(string surveyId, ObservableCollection<ResultBasicInfo> list)
        {
            _list = list;
            _surveyId = surveyId;
        }
        /// <summary>
        /// Allows you to initialize class data with survey id. 
        /// </summary>
        /// <param name="surveyId">Survey identification number.</param>
        public OperationsOnListOfResults(string surveyId)
        {
            _list = new ObservableCollection<ResultBasicInfo>();
            _surveyId = surveyId;
        }

        private ObservableCollection<ResultBasicInfo> _list;
        /// <summary>
        /// Reads listOfResults.xml in matching survey id folder set during class initialization.
        /// </summary>
        public void ReadList()
        {
            XDocument document = _savedDocument;
            if (document != null)
            {
                XElement root = document.Element("results");
                var results = from result in root.Elements("result") select new ResultBasicInfo() { Id = result.Attribute("id").Value, Title = result.Attribute("title").Value,
                                                                                                    Latitude = result.Element("latitude") != null ? result.Element("latitude").Value : null,
                                                                                                    Longitude = result.Element("longitude") != null ? result.Element("longitude").Value : null,
                                                                                                    Time = result.Element("time").Value, ParentId = result.Element("parentId").Value, IsResultCompleted = Convert.ToBoolean(result.Attribute("isCompleted").Value),
                                                                                                    IsResultSent = Convert.ToBoolean(result.Attribute("isSent").Value) };

                _list.Clear();
                foreach (ResultBasicInfo result in results)
                {
                    _list.Add(result);
                }
            }
        }
        /// <summary>
        /// Adds new survey result to list. Recreates listOfResults.xml file.
        /// </summary>
        /// <param name="basicInfo">Instance of <see cref="ResultBasicInfo"/> class.</param>
        public void Add(ResultBasicInfo basicInfo)
        {
            ReadList();
            bool add = true;
            var saved = from result in _list where result.Id.Equals(basicInfo.Id) select result;
            foreach (var result in saved)
            {
                if (result.IsResultCompleted == basicInfo.IsResultCompleted)
                {
                    return;
                }
                else
                {
                    result.IsResultCompleted = basicInfo.IsResultCompleted;
                    add = false;
                }
            }
            if (add)
            {
                _list.Add(new ResultBasicInfo()
                {
                    Id = basicInfo.Id,
                    Title = basicInfo.Title,
                    Time = basicInfo.Time,
                    Latitude = basicInfo.Latitude,
                    Longitude = basicInfo.Longitude,
                    ParentId = basicInfo.ParentId,
                    IsResultCompleted = basicInfo.IsResultCompleted,
                    IsResultSent = basicInfo.IsResultSent
                });
            }
            XDocument documentWithList = PrepareXDocument();
            _savedDocument = documentWithList;
        }
        private XDocument PrepareXDocument()
        {
            XDocument savedSurveys = new XDocument();
            XElement root = new XElement("results");
            foreach (ResultBasicInfo result in _list)
            {
                XElement resultElement = new XElement("result", new XAttribute("id", result.Id), new XAttribute("title", result.Title), new XAttribute("isCompleted", result.IsResultCompleted), new XAttribute("isSent", result.IsResultSent));
                resultElement.Add(new XElement("time", result.Time));
                if (result.Latitude != null)
                {
                    resultElement.Add(new XElement("latitude", result.Latitude));
                }
                if (result.Latitude != null)
                {
                    resultElement.Add(new XElement("longitude", result.Longitude));
                }
                resultElement.Add(new XElement("parentId", result.ParentId));
                root.Add(resultElement);
            }
            savedSurveys.AddFirst(root);
            return savedSurveys;
        }
        private XDocument _savedDocument
        {
            set
            {
                using (IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string directoryPath = string.Format("surveys/{0}", _surveyId);
                    string listOfSurveysFilePath = System.IO.Path.Combine(directoryPath, "listOfResults.xml");
                    if (!isolatedStorage.DirectoryExists(directoryPath))
                    {
                        isolatedStorage.CreateDirectory(directoryPath);
                    }
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(listOfSurveysFilePath, FileMode.Create, isolatedStorage))
                    {
                        value.Save(isoStream);
                    }
                }
            }
            get
            {
                XDocument document = null;
                try
                {
                    using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        string listOfSurveysFilePath = System.IO.Path.Combine(string.Format("surveys/{0}", _surveyId), "listOfResults.xml");
                        if (!isoStore.FileExists(listOfSurveysFilePath))
                        {
                            return null;
                        }
                        using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(listOfSurveysFilePath, FileMode.Open, isoStore))
                        {
                            document = XDocument.Load(isoStream);

                        }
                    }
                }
                catch (IsolatedStorageException)
                {
                }
                return document;
            }
        }

        /// <summary>
        /// Removes result from list. Recreates listOfResults.xml file and deletes result file.
        /// </summary>
        /// <param name="resultId">Result identification number.</param>
        public void DeleteResult(string resultId)
        {
            ReadList();
            var items = from item in _list where item.Id == resultId select item;
            ResultBasicInfo result = items.First<ResultBasicInfo>();

            _list.Remove(result);
            XDocument documentWithList = PrepareXDocument();
            _savedDocument = documentWithList;
            DeleteResultFile(result);
        }
        private void DeleteResultFile(ResultBasicInfo result)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string directoryPath = string.Format("surveys/{0}", _surveyId);
                string resultFilePath = System.IO.Path.Combine(directoryPath, string.Format("r_{0}.xml", result.Id));
                isoStore.DeleteFile(resultFilePath);
            }
        }
        /// <summary>
        /// Deletes all results for survey with id set during class initialization.
        /// </summary>
        public void DeleteAllResults()
        {
            ReadList();
            foreach (ResultBasicInfo result in _list)
            {
                DeleteResultFile(result);
            }
            _list.Clear();
            XDocument documentWithList = PrepareXDocument();
            _savedDocument = documentWithList;
        }

        /// <summary>
        /// Recreates listOfResults.xml file with Sent attribute set to true for single result.
        /// </summary>
        /// <param name="resultId">Result id you want to modify.</param>
        public void MarkResultAsSent(string resultId)
        {
            ReadList();
            var items = from item in _list where item.Id == resultId select item;
            ResultBasicInfo result = items.First<ResultBasicInfo>();
            result.IsResultSent = true;
            XDocument documentWithList = PrepareXDocument();
            _savedDocument = documentWithList;
        }

        /// <summary>
        /// Recreates listOfResults.xml file with Sent attribute set to false for single result.
        /// </summary>
        /// <param name="resultId">Result id you want to modify.</param>
        public void UnmarkSentResult(string resultId)
        {
            ReadList();
            var items = from item in _list where item.Id == resultId select item;
            ResultBasicInfo result = items.First<ResultBasicInfo>();
            result.IsResultSent = false;
            XDocument documentWithList = PrepareXDocument();
            _savedDocument = documentWithList;
        }

        /// <summary>
        /// Recreates listOfResults.xml file with new location.
        /// </summary>
        /// <param name="resultId">Result id you want to modify.</param>
        /// <param name="latitude">Result latitude you want to modify.</param>
        /// <param name="longitude">Result longitude you want to modify.</param>
        public void UpdateLocation(string resultId, string latitude, string longitude)
        {
            ReadList();
            var items = from item in _list where item.Id == resultId select item;
            ResultBasicInfo result = items.First<ResultBasicInfo>();
            if (result != null)
            {
                result.Latitude = latitude;
                result.Longitude = longitude;
                XDocument documentWithList = PrepareXDocument();
                _savedDocument = documentWithList;
            }
        }
    }
}
