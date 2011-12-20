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
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using com.comarch.mobile.ndg.Settings.Model;

namespace com.comarch.mobile.ndg.Model.SurveyForms
{
    /// <summary>
    /// Class stores survey data and methods to operate on surveys.
    /// </summary>
    public class Survey
    {
        /// <summary>
        /// Default contructor to initial all data in class.
        /// </summary>
        public Survey()
        {
            ResultInfo = new ResultBasicInfo();
            Categories = new List<Category>();
        }
        /// <summary>
        /// Represents survey title.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Represents survey ID number.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Represents all categories included in survey.
        /// </summary>
        public List<Category> Categories { get; private set; }
        private bool _isResultCompleted;
        /// <summary>
        /// Event for saving result.
        /// </summary>
        public EventHandler SavingCompletedEventHandler;
        /// <summary>
        /// Event for reading result.
        /// </summary>
        public EventHandler ReadingCompletedEventHandler;
        /// <summary>
        /// Event for changed value in result.
        /// </summary>
        public EventHandler ResultChangedEventHandler;
        /// <summary>
        /// Represents information about taking photo.
        /// </summary>
        public bool TakingPhoto { get; set; }
        private bool _isResultChanged;
        /// <summary>
        /// Represents situation when result was changed.
        /// </summary>
        public bool IsResultChanged 
        {
            get
            {
                return _isResultChanged;
            }
            set
            {
                if (_isResultChanged == value)
                    return;
                _isResultChanged = value;
                EventHandler handler = ResultChangedEventHandler;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Represents instance of <see cref="ResultBasicInfo"/> class. Stores information about result.
        /// </summary>
        public ResultBasicInfo ResultInfo { get; set; }

        /// <summary>
        /// Method to refresh questions visibility in category.
        /// </summary>
        public void RefreshQuestionsVisibility()
        {
            var normalCategories = from category in Categories where category.GetType() == typeof(NormalCategory) select category;
            foreach (NormalCategory category in normalCategories)
            {
                category.RefreshQuestionsVisibility();
            }
        }

        private XDocument Load(int surveyId)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string surveyFilePath = System.IO.Path.Combine("surveys", string.Format("{0}.xml", surveyId));
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(surveyFilePath, FileMode.Open, storage))
                {
                    return XDocument.Load(stream);            
                }
            }
        }

        private Question CreateDescriptiveQuestion(XElement questionIterator, Category parent)
        {
            return new DescriptiveQuestion(parent)
            {
                Length = Convert.ToInt32(questionIterator.Element("length").Value)
            };
        }

        private Question CreateNumericQuestion(XElement questionIterator, NumericQuestion.Types type, Category parent)
        {
            NumericQuestion numericQuestion = new NumericQuestion(parent, type);
            if (questionIterator.Attribute("min").Value != "")
            {
                numericQuestion.MinValue = Convert.ToDouble(questionIterator.Attribute("min").Value);
            }
            if (questionIterator.Attribute("max").Value != "")
            {
                numericQuestion.MaxValue = Convert.ToDouble(questionIterator.Attribute("max").Value);
            }
            numericQuestion.Length = Convert.ToInt32(questionIterator.Element("length").Value);

            return numericQuestion;
        }

        private Question CreateExclusiveChoiceQuestion(XElement questionIterator, Category parent)
        {
            ExclusiveChoiceQuestion exclusiveQuestion = new ExclusiveChoiceQuestion(parent);
            ExclusiveChoiceQuestion.ChoiceItem exclusiveChoiceItem;
            foreach (XElement item in questionIterator.Elements("item"))
            {
                exclusiveChoiceItem = new ExclusiveChoiceQuestion.ChoiceItem(exclusiveQuestion);
                if (item.Attribute("def") != null)
                {
                    exclusiveChoiceItem.IsChosen = item.Attribute("def").Value == "1";
                }
                exclusiveChoiceItem.IsMoreDetailsEnabled = item.Attribute("otr").Value == "1";
                exclusiveChoiceItem.Name = item.Value;
                exclusiveQuestion.ChoiceItems.Add(exclusiveChoiceItem);
            }

            XElement skipLogic = questionIterator.Element("SkipLogic");
            if (skipLogic!= null)
            {
                int index = Convert.ToInt32(skipLogic.Attribute("operand").Value);
                bool inverse = skipLogic.Attribute("operator").Value == "1";
                exclusiveQuestion.ChoiceItems[index].SetAsSkipLogic(Convert.ToInt32(skipLogic.Attribute("skipTo").Value), inverse);
            }
            return exclusiveQuestion;
        }

        private Question CreateMultipleChoiceQuestion(XElement questionIterator, Category parent)
        {
            MultipleChoiceQuestion multipleQuestion = new MultipleChoiceQuestion(parent);
            MultipleChoiceQuestion.CheckBoxItem multipleChoiceItem;
            foreach (XElement item in questionIterator.Elements("item"))
            {
                multipleChoiceItem = new MultipleChoiceQuestion.CheckBoxItem(multipleQuestion);
                if (item.Attribute("def") != null)
                {
                    multipleChoiceItem.IsChecked = item.Attribute("def").Value == "1";
                }
                multipleChoiceItem.IsMoreDetailsEnabled = item.Attribute("otr").Value == "1";
                multipleChoiceItem.Name = item.Value;
                multipleQuestion.CheckBoxItems.Add(multipleChoiceItem);
            }
            return multipleQuestion;
        }

        private Question CreateTimeQuestion(XElement questionIterator,Category parent)
        {
            TimeQuestion timeQuestion = new TimeQuestion(parent);
            if (questionIterator.Attribute("convention").Value == "24")
            {
                timeQuestion.TimeConvension = TimeQuestion.TimeConvensions.TwentyFourConvension;
            }
            else
            {
                timeQuestion.TimeConvension = TimeQuestion.TimeConvensions.TwelveConvension;
            }
            timeQuestion.Answer = DateTime.Now.ToShortTimeString();
            return timeQuestion;
        }

        private Question CreateDateQuestion(XElement questionIterator, Category parent)
        {
            DateOperations operationsOnDate = new DateOperations();
            DateQuestion dateQuestion = new DateQuestion(parent);
            if (questionIterator.Attribute("max").Value != "")
            {
                dateQuestion.MaxDate = operationsOnDate.ParseDate(questionIterator.Attribute("max").Value);
            }
            if (questionIterator.Attribute("min").Value != "")
            {
                dateQuestion.MinDate = operationsOnDate.ParseDate(questionIterator.Attribute("min").Value);
            }
            dateQuestion.Answer = DateTime.Now.ToLongDateString();
            return dateQuestion;
        }

        private Question CreateImageQuestion(XElement questionIterator, Category parent)
        {
            return new ImageQuestion(parent)
            {
                MaxCount = Convert.ToInt32(questionIterator.Attribute("maxCount").Value)
            };
        }

        private void ParseQuestion(XElement questionIterator, List<Question> list, Category parent)
        {
            Question question = null;
            switch (questionIterator.Attribute("type").Value)
            {
                case "_str":
                    question = CreateDescriptiveQuestion(questionIterator, parent);
                    break;

                case "_int":
                    question = CreateNumericQuestion(questionIterator, NumericQuestion.Types.IntegerType, parent);
                    break;

                case "_decimal":
                    question = CreateNumericQuestion(questionIterator, NumericQuestion.Types.DecimalType, parent);
                    break;

                case "_choice":
                    if (questionIterator.Element("select").Value == "exclusive")
                    {
                        question = CreateExclusiveChoiceQuestion(questionIterator, parent);
                    }
                    else
                    {
                        question = CreateMultipleChoiceQuestion(questionIterator, parent);
                    }
                    break;

                case "_time":
                    question = CreateTimeQuestion(questionIterator, parent);
                    break;

                case "_date":
                    question = CreateDateQuestion(questionIterator, parent);
                    break;

                case "_img":
                    question = CreateImageQuestion(questionIterator, parent);
                    break;
            }
            list.Add(question);
            list[list.Count - 1].Id = Convert.ToInt32(questionIterator.Attribute("id").Value);
            list[list.Count - 1].Description = questionIterator.Element("description").Value;
        }

        private void ParseCategories(IEnumerable<XElement> categories)
        {
            Category category;
            foreach (var categoryIterator in categories)
            {
                var questions = categoryIterator.Descendants("question");
                string categoryId = categoryIterator.Attribute("id").Value;
                string categoryName = categoryIterator.Attribute("name").Value;
                if (categoryIterator.Attribute("condition") != null)
                {
                    ConditionCategory condintionalCategoryInstance = new ConditionCategory(this) { Id = categoryId, Name = categoryName };
                    condintionalCategoryInstance.ConditionQuestion = categoryIterator.Attribute("condition").Value;
                    foreach (var questionIterator in questions)
                    {
                        ParseQuestion(questionIterator, condintionalCategoryInstance.QuestionsTemplate, condintionalCategoryInstance);
                    }
                    category = condintionalCategoryInstance;
                }
                else
                {
                    NormalCategory normalCategoryInstance = new NormalCategory(this) { Id = categoryId, Name = categoryName };

                    foreach (var questionIterator in questions)
                    {
                        ParseQuestion(questionIterator, normalCategoryInstance.Questions, normalCategoryInstance);
                    }
                    category = normalCategoryInstance;
                }
                Categories.Add(category);
            }
        }

        private void Parse(XDocument xmlDocument)
        {
            var survey = xmlDocument.Element("survey");
            var categories = survey.Descendants("category");

            Title = survey.Attribute("title").Value;
            Id = survey.Attribute("id").Value;
            ParseCategories(categories);
        }

        /// <summary>
        /// Method to display survey.
        /// </summary>
        /// <param name="surveyId">Survey identification number.</param>
        public void Display(int surveyId)
        {
            Parse(Load(surveyId));
            IsResultChanged = false;
        }

        /// <summary>
        /// Method to reading saved result.
        /// </summary>
        /// <param name="resultId">Result identification number.</param>
        public void ReadSurveyResult(string resultId)
        {
            Thread t = new Thread(new ThreadStart(() =>
            {
                ResultInfo .Id= resultId;
                if (!string.IsNullOrEmpty(ResultInfo.Title) && !string.IsNullOrEmpty(ResultInfo.Id))
                {
                    XDocument document = null;
                    try
                    {
                       document = GetSavedDocument();
                    }
                    catch (XmlException)
                    {
                    }
                    catch (CryptographicException)
                    {
                    }
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ReadResultFromXDocument(document);
                        IsResultChanged = false;
                        EventHandler handler = ReadingCompletedEventHandler;
                        if (handler != null)
                            handler(this, EventArgs.Empty);
                    });
                }
            }));
            t.Start();
        }

        private void ReadResultFromXDocument(XDocument document)
        {
            if (document != null)
            {
                XElement root = document.Element("result");
                if (root.Element("latitude") != null && root.Element("longitude") != null)
                {
                    ResultInfo.Latitude = root.Element("latitude").Value;
                    ResultInfo.Longitude = root.Element("longitude").Value;
                }
                foreach (XElement categoryXML in root.Elements("category"))
                {
                    int categoryId = Convert.ToInt32(categoryXML.Attribute("id").Value);
                    Category category = Categories[categoryId - 1];
                    category.ReadLastResult(categoryXML);
                }
            }
        }

        /// <summary>
        /// Method to reading saved result from temporary file.
        /// </summary>
        /// <param name="result">Result identification number.</param>
        public void ReadSurveyResultFromStateDictionary(string result)
        {
            XDocument document = XDocument.Parse(result);
            XElement root = document.Element("result");
            ResultInfo.Title = root.Element("title").Value;
            ResultInfo.Id = root.Attribute("r_id").Value;
            if (root.Element("latitude") != null && root.Element("longitude") != null)
            {
                ResultInfo.Latitude = root.Element("latitude").Value;
                ResultInfo.Longitude = root.Element("longitude").Value;
            }
            ReadResultFromXDocument(document);
        }

        /// <summary>
        /// Method to loading document from xml file saved in IsolatedStorage.
        /// </summary>
        /// <returns>Return all information about document as XDocument.</returns>
        public XDocument GetSavedDocument()
        {
            XDocument document = null;
            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string directoryPath = string.Format("surveys/{0}", Id);
                    string resultFilePath = System.IO.Path.Combine(directoryPath, string.Format("r_{0}.xml", ResultInfo.Id));
                    if (!isoStore.FileExists(resultFilePath))
                    {
                        return null;
                    }
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(resultFilePath, FileMode.Open, isoStore))
                    {
                        StreamReader reader = new StreamReader(isoStream);
                        string strDoc = reader.ReadToEnd();

                        if ((bool)OperationsOnSettings.Instance.IsEncryptionEnabled)
                        {
                            AESEncryption encrypter = new AESEncryption();
                            strDoc = encrypter.Decrypt(strDoc, App.AppDictionary["EncryptionPassword"] as string, "qwhmvbzx");
                        }

                        document = XDocument.Parse(strDoc);
                    }
                }
            }
            catch (IsolatedStorageException)
            {
            }
            return document;
        }

        /// <summary>
        /// Method to saving new/modified result.
        /// </summary>
        public void SaveSurveyResult()
        {
            Thread t = new Thread(new ThreadStart(() => 
            {
                if (!string.IsNullOrEmpty(ResultInfo.Title))
                {
                    DateOperations operationsOnDate = new DateOperations();

                    XDocument documentXML = PrepareResultDocument();
                    string dataToSave;

                    if ((bool)OperationsOnSettings.Instance.IsEncryptionEnabled)
                    {
                        AESEncryption encrypter = new AESEncryption();
                        dataToSave = encrypter.Encrypt(documentXML.ToString(), App.AppDictionary["EncryptionPassword"] as string, "qwhmvbzx");
                    }
                    else

                    {
                        dataToSave = documentXML.ToString();
                    }

                    using (IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        string directoryPath = string.Format("surveys/{0}", Id);
                        string resultFilePath = System.IO.Path.Combine(directoryPath, string.Format("r_{0}.xml", ResultInfo.Id));
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
                    AddResultToList();
                }
                EventHandler handler = SavingCompletedEventHandler;
                if (handler != null)
                    handler(this, EventArgs.Empty);
                IsResultChanged = false;
            }));
            t.Start();
        }

        /// <summary>
        /// Represents information about saved GPS location (in result).
        /// </summary>
        public bool IsGpsSet { get; set; }

        /// <summary>
        /// Method to prepare XML file based on information about filled out survey.
        /// </summary>
        /// <returns>Return result in proper XML format.</returns>
        public XDocument PrepareResultDocument()
        {
            _isResultCompleted = true;
            DateOperations operationsOnDate = new DateOperations();
            XDocument resultDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            
            DateTime centuryBegin = new DateTime(2001, 1, 1);

            string userId = OperationsOnSettings.Instance.IMEI;
            string time = operationsOnDate.DateTimeToMiliseconds(DateTime.Now).ToString();
            ResultInfo.Time = time;
            ResultInfo.ParentId = Id;
            string resultId;
            if (string.IsNullOrEmpty(ResultInfo.Id))
            {
                ResultInfo.Id = GenerateUniqueID();
            }
            resultId = ResultInfo.Id;

            XElement root = new XElement("result", new XAttribute("r_id", resultId), new XAttribute("s_id", Id), new XAttribute("u_id", userId), new XAttribute("time", time), new XAttribute("version", 2));
            if (((ResultInfo.Latitude == null) || (ResultInfo.Longitude == null)))
            {
                if (OperationsOnSettings.Instance.GPS)
                {
                    var position = GPSService.Instance.Location;
                    if (position != null)
                    {
                        IsGpsSet = true;
                        XElement latitude = new XElement("latitude");
                        XElement longitude = new XElement("longitude");
                        latitude.Value = position.Latitude.ToString();
                        longitude.Value = position.Longitude.ToString();
                        ResultInfo.Latitude = position.Latitude.ToString();
                        ResultInfo.Longitude = position.Longitude.ToString();
                        root.Add(latitude);
                        root.Add(longitude);
                        OperationsOnListOfResults listOperator = new OperationsOnListOfResults(Id);
                        listOperator.UpdateLocation(ResultInfo.Id, position.Latitude.ToString(), position.Longitude.ToString());
                    }
                    else
                    {
                        IsGpsSet = false;
                    }
                }
                else
                {
                    IsGpsSet = true;
                }
            }
            else
            {
                XElement latitude = new XElement("latitude");
                XElement longitude = new XElement("longitude");
                latitude.Value = ResultInfo.Latitude;
                longitude.Value = ResultInfo.Longitude;
                root.Add(latitude);
                root.Add(longitude);
                IsGpsSet = true;
            }
            XElement titleElement = new XElement("title");
            if (!string.IsNullOrEmpty(ResultInfo.Title))
                titleElement.Value = ResultInfo.Title;
            root.Add(titleElement);

            foreach (Category category in Categories)
            {
                XElement categoryXElement = new XElement("category", new XAttribute("name", category.Name), new XAttribute("id", category.Id));
                if (!category.AddResult(categoryXElement))
                {
                    _isResultCompleted = false;
                }
                root.Add(categoryXElement);
            }
            resultDocument.AddFirst(root);
            ResultInfo.IsResultCompleted = _isResultCompleted;
            return resultDocument;
        }

        private string GenerateUniqueID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            int id = BitConverter.ToInt32(buffer, 0);
            if ( id < 0)
                id *= -1;
            return id.ToString("X");
        }

        private void AddResultToList()
        {
            OperationsOnListOfResults operationsOnListOfResults = new OperationsOnListOfResults(Id);
            operationsOnListOfResults.Add(ResultInfo);
        }

        /// <summary>
        /// Represents information about full/part filled out survey.
        /// </summary>
        public bool IsResultCorrect
        {
            get
            {
                foreach (Category category in Categories)
                {
                    if (!category.IsResultCorrect)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Method to saving temporary XML file with result (using when application is holding).
        /// </summary>
        public void SaveTmpData()
        {
            XDocument document = PrepareResultDocument();

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            StreamWriter sw = new StreamWriter(isoStore.OpenFile("tmpresult.xml", FileMode.Create));
            sw.Write(document);
            sw.Close();
        }

        /// <summary>
        /// Method to reading result details from temporary XML file (after resuming application).
        /// </summary>
        public void GetTmpData()
        {
            Thread t = new Thread(new ThreadStart(() =>
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (isoStore.FileExists("tmpresult.xml") )
                {

                    StreamReader sr = new StreamReader(isoStore.OpenFile("tmpresult.xml", FileMode.Open));
                    string data = sr.ReadToEnd();
                    sr.Close();
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ReadSurveyResultFromStateDictionary(data);
                    });
                    isoStore.DeleteFile("tmpresult.xml");
                }
            }));
            t.Start();
        }

        /// <summary>
        /// Method to generating default result title.
        /// </summary>
        /// <returns>Return generated result title.</returns>
        public string GetDefaultResultTitle()
        {
            foreach (Category cat in Categories)
            {
                if (cat is NormalCategory)
                {
                    foreach (Question question in (cat as NormalCategory).Questions)
                    {
                        if (question is DescriptiveQuestion && question.IsEnabled)
                        {
                            if (!string.IsNullOrEmpty((question as DescriptiveQuestion).Answer))
                            {
                                return (question as DescriptiveQuestion).Answer;
                            }
                        }
                    }
                }
                else
                {
                    foreach (NormalCategory subCat in (cat as ConditionCategory).SubCategories)
                    {
                        foreach (Question question in subCat.Questions)
                        {
                            if (question is DescriptiveQuestion && question.IsEnabled)
                            {
                                if (!string.IsNullOrEmpty((question as DescriptiveQuestion).Answer))
                                {
                                    return (question as DescriptiveQuestion).Answer;
                                }
                            }
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(ResultInfo.Id))
                ResultInfo.Id = GenerateUniqueID();
            return ResultInfo.Id;
        }
        
    }
}
