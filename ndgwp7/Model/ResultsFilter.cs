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
using System.ComponentModel;
using System.Device.Location;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using com.comarch.mobile.ndg.BusyIndicator;

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Class responsible for filtering and grouping results by selected surveys, date or location.
    /// </summary>
    public class ResultsFilter
    {
        /// <summary>
        /// Represents instance of <see cref="SurveyFilter"/> class.
        /// </summary>
        public SurveyFilter SurveyFiltration { get; set; }
        /// <summary>
        /// Represents instance of <see cref="DateFilter"/> class.
        /// </summary>
        public DateFilter DateFiltration { get; set; }
        /// <summary>
        /// Represents instance of <see cref="LocationFilter"/> class.
        /// </summary>
        public LocationFilter LocationFiltration { get; set; }
        /// <summary>
        /// Represents instance of <see cref="ProcessingBar"/> class.
        /// </summary>
        public ProcessingBar Busy { get; set; }

        /// <summary>
        /// Class that groups all filtered results and allows you to display grouped results.
        /// </summary>
        /// <typeparam name="T">Type of objects you want to group.</typeparam>
        public class GroupedOC<T> : ObservableCollection<T>
        {
            /// <summary>
            /// Represents group name.
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// Represents group identification number.
            /// </summary>
            public string Id { get; set; } //represents group id == survey id
            /// <summary>
            /// Represents xml value whether group is marked as favorite or not. 
            /// </summary>
            public string Favorite { get; set; }

            /// <summary>
            /// Allows you to assign all necessary data at initialization.
            /// </summary>
            /// <param name="name">Title of a group.</param>
            /// <param name="id">Identification number of a group.</param>
            /// <param name="favorite">Notifies of xml favorite attribute value. </param>
            public GroupedOC(string name, string id, string favorite)
            {
                Title = name;
                Id = id;
                Favorite = favorite;
            }
        }

        /// <summary>
        /// Allows you to initialize all necessary data objects. Requires current list of surveys.
        /// </summary>
        /// <param name="list">Current list of surveys saved in IsolatedStorage</param>
        public ResultsFilter(ObservableCollection<SurveyBasicInfo> list)
        {
            SurveyFiltration = new SurveyFilter();
            DateFiltration = new DateFilter();
            LocationFiltration = new LocationFilter();
            Busy = new ProcessingBar(Languages.AppResources.resultsFilter_Searching);
            SurveyFiltration.UpdateSurveyPicker(list);
        }

        /// <summary>
        /// Use only if you dont need to use list of surveys.
        /// </summary>
        public ResultsFilter() { }

        private XDocument LoadListOfResults(string surveyId)
        {
            XDocument document = null;
            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string listOfSurveysFilePath = System.IO.Path.Combine(string.Format("surveys/{0}", surveyId), "listOfResults.xml");
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

        /// <summary>
        /// Removes result from list. Uses <see cref="OperationsOnListOfResults"/> to remove data from IsolatedStorage.
        /// </summary>
        /// <param name="result">Instance of <see cref="ResultBasicInfo"/> class which holds that of result you want to remove.</param>
        public void DeleteResult(ResultBasicInfo result)
        {
            var results = from item in App.AppDictionary["FilteredResults"] as ObservableCollection<ResultsFilter.GroupedOC<ResultBasicInfo>> where item.Contains(result) select item;
            GroupedOC<ResultBasicInfo> entity = results.First<GroupedOC<ResultBasicInfo>>();
            entity.Remove(result);
            if (entity.Count == 0)
            {
                (App.AppDictionary["FilteredResults"] as ObservableCollection<ResultsFilter.GroupedOC<ResultBasicInfo>>).Remove(entity);
            }
            OperationsOnListOfResults operations = new OperationsOnListOfResults(result.ParentId);
            operations.DeleteResult(result.Id);
        }

        private bool HasResult(GroupedOC<ResultBasicInfo> list, string resultId)
        {
            var results = from result in list
                          where result.Id == resultId
                          select result;
            bool isNotNull = false;
            foreach (var i in results)
            {
                isNotNull = true;
                break;
            }
            if (isNotNull)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Reloads basic data of single survey result from listOfResults.xml file.
        /// </summary>
        /// <param name="resultId">Id of result you want to reload.</param>
        public void ReloadResult(string resultId)
        {
            int groupIndex = -1;
            int itemIndex = -1;
            ObservableCollection<ResultsFilter.GroupedOC<ResultBasicInfo>> filteredResults = App.AppDictionary["FilteredResults"] as ObservableCollection<ResultsFilter.GroupedOC<ResultBasicInfo>>;
            foreach (var collection in filteredResults)
            {
                var items = from item in collection where item.Id == resultId select item;
                bool isNotNull = false;
                foreach (var i in items)
                {
                    isNotNull = true;
                    break;
                }
                if (isNotNull)
                {
                    ResultBasicInfo basicInfo = items.First<ResultBasicInfo>();
                    XDocument document = LoadListOfResults(basicInfo.ParentId);
                    if (document != null)
                    {
                        XElement root = document.Element("results");
                        var results = from result in root.Elements("result")
                                      where result.Attribute("id").Value == basicInfo.Id
                                      select new ResultBasicInfo()
                                      {
                                          Id = result.Attribute("id").Value,
                                          Title = result.Attribute("title").Value,
                                          IsResultSent = Convert.ToBoolean(result.Attribute("isSent").Value),
                                          IsResultCompleted = Convert.ToBoolean(result.Attribute("isCompleted").Value),
                                          Latitude = result.Element("latitude") != null ? result.Element("latitude").Value : null,
                                          Longitude = result.Element("longitude") != null ? result.Element("longitude").Value : null,
                                          Time = result.Element("time").Value,
                                          ParentId = result.Element("parentId").Value
                                      };
                        ResultBasicInfo resultToInsert = results.First<ResultBasicInfo>();
                        groupIndex = filteredResults.IndexOf(collection);
                        itemIndex = collection.IndexOf(basicInfo);
                        filteredResults[groupIndex].RemoveAt(itemIndex);
                        filteredResults[groupIndex].Insert(itemIndex, resultToInsert);
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Triggers when filtering results is finished.
        /// </summary>
        public event EventHandler SearchingCompletedEventHandler;

        /// <summary>
        /// Runs whole filtration process and creates grouped list to display.
        /// </summary>
        /// <param name="avaiableSurveys">List of surveys currently saved in IsolatedStorage.</param>
        public void Display(ObservableCollection<SurveyBasicInfo> avaiableSurveys)
        {
            ObservableCollection<GroupedOC<ResultBasicInfo>> filteredResults = new ObservableCollection<GroupedOC<ResultBasicInfo>>();
            if (App.AppDictionary.ContainsKey("FilteredResults"))
                App.AppDictionary["FilteredResults"] = filteredResults;
            else
                App.AppDictionary.Add("FilteredResults", filteredResults);
            Busy.IsEnabled = true;
            Thread searchingThread = new Thread(() =>
            {
                ObservableCollection<SurveyBasicInfo> searchList;
                if (SurveyFiltration.IsEnabled)
                {
                    if (SurveyFiltration.SelectedSurveys.Contains(SurveyFiltration.AllSurveysItem))
                    {
                        searchList = avaiableSurveys;
                    }
                    else
                    {
                        searchList = SurveyFiltration.SelectedSurveys;
                    }
                }
                else
                {
                    searchList = avaiableSurveys;
                }
                foreach (var survey in searchList)
                {
                    XDocument document = LoadListOfResults(survey.SurveyId);
                    if (document != null)
                    {
                        GroupedOC<ResultBasicInfo> singleSurveyResults = new GroupedOC<ResultBasicInfo>(survey.Name, survey.SurveyId, survey.IsFavorite.ToString());
                        XElement root = document.Element("results");
                        var results = from result in root.Elements("result")
                                      where DateFiltration.IsMatching(result.Element("time").Value) &&
                                            LocationFiltration.IsMatching(result.Element("latitude") != null ? result.Element("latitude").Value : null,
                                                                          result.Element("longitude") != null ? result.Element("longitude").Value : null)
                                      select new ResultBasicInfo()
                                      {
                                          Id = result.Attribute("id").Value,
                                          Title = result.Attribute("title").Value,
                                          IsResultSent = Convert.ToBoolean(result.Attribute("isSent").Value),
                                          IsResultCompleted = Convert.ToBoolean(result.Attribute("isCompleted").Value),
                                          Latitude = result.Element("latitude") != null ? result.Element("latitude").Value : null,
                                          Longitude = result.Element("longitude") != null ? result.Element("longitude").Value : null,
                                          Time = result.Element("time").Value,
                                          ParentId = result.Element("parentId").Value
                                      };

                        foreach (var basicInfo in results)
                        {
                            singleSurveyResults.Add(basicInfo);
                        }
                        if (singleSurveyResults.Count > 0)
                        {
                            filteredResults.Add(singleSurveyResults);
                        }

                    }
                }
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    EventHandler handler = SearchingCompletedEventHandler;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                });
            });
            searchingThread.Start();
        }
    }
    /// <summary>
    /// Class responsible for filtration by surveys.
    /// </summary>
    public class SurveyFilter : INotifyPropertyChanged
    {
        /// <summary>
        /// Represents list of surveys displayed in list picker.
        /// </summary>
        public ObservableCollection<string> ListPickerSurveys { get; set; }
        /// <summary>
        /// Represents list of surveys selected for search.
        /// </summary>
        public ObservableCollection<SurveyBasicInfo> SelectedSurveys { get; set; }
        /// <summary>
        /// Represents instance of <see cref="SurveyBasicInfo"/> class and all surveys on list. It has unique id and specific privileges.
        /// </summary>
        public SurveyBasicInfo AllSurveysItem { get; set; }
        private string _chosenSurvey;
        /// <summary>
        /// Represents item currently selected by list picker.
        /// </summary>
        /// <value>Gets/Sets _chosenSurvey data member.</value>
        public string ChosenSurvey
        {
            get { return _chosenSurvey; }
            set
            {
                _chosenSurvey = value;
                RaisePropertyChanged("ChosenSurvey");
            }
        }

        private bool _isEnabled;
        /// <summary>
        /// Represents status of survey filter.
        /// </summary>
        /// <value>Gets/Sets _isEnabled data member.</value>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
        }

        /// <summary>
        /// Adds selected survey to list you want to search by.
        /// </summary>
        /// <param name="avaiableSurveys">List of surveys currently saved in IsolatedStorage.</param>
        public void AddChosenSurvey(ObservableCollection<SurveyBasicInfo> avaiableSurveys)
        {
            int selectedIndex = Convert.ToInt32(ChosenSurvey);
            if (selectedIndex != avaiableSurveys.Count)
            {
                SurveyBasicInfo selectedItem = avaiableSurveys[selectedIndex];
                if (!SelectedSurveys.Contains(selectedItem))
                {
                    SelectedSurveys.Add(selectedItem);
                }
                if (SelectedSurveys.Count > 1)
                {
                    SelectedSurveys.Remove(AllSurveysItem);
                }
            }
            else
            {
                SelectedSurveys.Clear();
                SelectedSurveys.Add(AllSurveysItem);
            }
        }

        /// <summary>
        /// Updates survey list picker to current list of surveys.
        /// </summary>
        /// <param name="avaiableSurveys">List of surveys currently saved in IsolatedStorage.</param>
        public void UpdateSurveyPicker(ObservableCollection<SurveyBasicInfo> avaiableSurveys)
        {
            ListPickerSurveys.Clear();
            foreach (var i in avaiableSurveys)
            {
                ListPickerSurveys.Add(i.Name);
            }
            ListPickerSurveys.Add(AllSurveysItem.Name);
        }

        /// <summary>
        /// Removes marked survey from list you want to search by.
        /// </summary>
        /// <param name="item"></param>
        public void RemoveSelectedSurvey(SurveyBasicInfo item)
        {
            if (item.SurveyId != "0")
            {
                SelectedSurveys.Remove(item);
            }
            if (SelectedSurveys.Count == 0)
            {
                SelectedSurveys.Add(AllSurveysItem);
            }
        }

        /// <summary>
        /// Initializes all necessary data objects.
        /// </summary>
        public SurveyFilter()
        {
            AllSurveysItem = new SurveyBasicInfo() { Name = Languages.AppResources.resultsFilter_AllSurveys, SurveyId = "0" };
            ListPickerSurveys = new ObservableCollection<string>();
            SelectedSurveys = new ObservableCollection<SurveyBasicInfo>();
            SelectedSurveys.Add(AllSurveysItem);
        }

        /// <summary>
        /// Triggers when property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    /// <summary>
    /// Class responsible for filtration by date.
    /// </summary>
    public class DateFilter : INotifyPropertyChanged
    {
        /// <summary>
        /// Represents instance of <see cref="DateOptionType"/> enum.
        /// </summary>
        public DateOptionType DateOption { get; set; }
        /// <summary>
        /// Represents list of filter options you can choose from.
        /// </summary>
        public ObservableCollection<string> DateFilterOptions { get; set; }

        /// <summary>
        /// Initializes all necessary object data.
        /// </summary>
        public DateFilter()
        {
            DateFilterOptions = new ObservableCollection<string>();
            DateFilterOptions.Add(Languages.AppResources.resultsFilter_DateTypeAt);
            DateFilterOptions.Add(Languages.AppResources.resultsFilter_DateTypeAfter);
            DateFilterOptions.Add(Languages.AppResources.resultsFilter_DateTypeBefore);
            DateFilterOptions.Add(Languages.AppResources.resultsFilter_DateTypeBetween);
        }
        private string _chosenDateFilter;
        /// <summary>
        /// Represents selected date filter type. Sets date option.
        /// </summary>
        /// <value>Gets/Sets _chosenDateFilter data member.</value>
        public string ChosenDateFilter
        {
            get { return _chosenDateFilter; }
            set
            {
                switch (value)
                {
                    case "0":
                        DateOption = DateOptionType.At;
                        break;
                    case "1":
                        DateOption = DateOptionType.After;
                        break;
                    case "2":
                        DateOption = DateOptionType.Before;
                        break;
                    case "3":
                        DateOption = DateOptionType.Between;
                        break;
                }
                _chosenDateFilter = value;
                RaisePropertyChanged("ChosenDateFilter");
            }
        }

        private string _chosenMaxDate = DateTime.Now.ToLongDateString();
        /// <summary>
        /// Represents max date parameter of filtration.
        /// </summary>
        /// <value>Gets/Sets _chosenMaxDate data member.</value>
        public string ChosenMaxDate
        {
            get { return _chosenMaxDate; }
            set
            {
                _chosenMaxDate = value;
                RaisePropertyChanged("ChosenMaxDate");
            }
        }

        private string _chosenMinDate = DateTime.Now.ToLongDateString();
        /// <summary>
        /// Represents min date parameter of filtration. Also used for single argument filters.
        /// </summary>
        /// <value>Gets/Sets _chosenMinDate data member.</value>
        public string ChosenMinDate
        {
            get { return _chosenMinDate; }
            set
            {
                _chosenMinDate = value;
                RaisePropertyChanged("ChosenMinDate");
            }
        }

        private bool _isEnabled;
        /// <summary>
        /// Represents status of date filter.
        /// </summary>
        /// <value>Gets/Sets _isEnabled data member.</value>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
        }

        /// <summary>
        /// Stores possible options of date filter.
        /// </summary>
        public enum DateOptionType
        {
            /// <summary>
            /// Indicates filtration by results created at the specific date.
            /// </summary>
            At,
            /// <summary>
            /// Indicates filtration by results created after specific date.
            /// </summary>
            After,
            /// <summary>
            /// Indicates filtration by results created before specific date.
            /// </summary>
            Before,
            /// <summary>
            /// Indicates filtration by results created between specific dates.
            /// </summary>
            Between
        }

        /// <summary>
        /// Checks if input date matches selected date filter.
        /// </summary>
        /// <param name="date">String date in miliseconds.</param>
        /// <returns>True if date matches query, false in any other case.</returns>
        public bool IsMatching(string date)
        {
            if (!IsEnabled)
            {
                return true;
            }
            DateOperations operations = new DateOperations();
            DateTime selectedMinDate = DateTime.Parse(ChosenMinDate, new CultureInfo("en-US"));
            DateTime selectedMaxDate = DateTime.Parse(ChosenMaxDate, new CultureInfo("en-US"));

            DateTime resultDate = operations.MilisecondsToDateTime(Convert.ToInt64(date));
            switch (DateOption)
            {
                case DateOptionType.At:
                    if (resultDate.Date == selectedMinDate.Date)
                    {
                        return true;
                    }
                    break;
                case DateOptionType.After:
                    if (resultDate.Date >= selectedMinDate.Date)
                    {
                        return true;
                    }
                    break;
                case DateOptionType.Before:
                    if (resultDate.Date <= selectedMinDate.Date) //ChosenMinDate property is used for single argument filters
                    {
                        return true;
                    }
                    break;
                case DateOptionType.Between:
                    if (selectedMinDate.Date > selectedMaxDate.Date)
                    {
                        var temp = selectedMinDate;
                        selectedMinDate = selectedMaxDate;
                        selectedMaxDate = temp;
                    }
                    if ((resultDate.Date >= selectedMinDate.Date) && (resultDate.Date <= selectedMaxDate.Date))
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Triggers when property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Class responsible for filtration by location.
    /// </summary>
    public class LocationFilter : INotifyPropertyChanged
    {
        private string _latitude = string.Empty;
        /// <summary>
        /// Represents geographic coordinate as string.
        /// </summary>
        /// <value>Gets/Sets _latitude data member.</value>
        public string Latitude
        {
            get { return _latitude; }
            set
            {
                _latitude = value;
                RaisePropertyChanged("Latitude");
            }
        }

        private string _longitude = string.Empty;
        /// <summary>
        /// Represents geographic coordinate as string.
        /// </summary>
        /// <value>Gets/Sets _longitude data member.</value>
        public string Longitude
        {
            get { return _longitude; }
            set
            {
                _longitude = value;
                RaisePropertyChanged("Longitude");
            }
        }

        private string _radius = string.Empty;
        /// <summary>
        /// Represents distance in meters as string.
        /// </summary>
        /// <value>Gets/Sets _radius data member.</value>
        public string Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                RaisePropertyChanged("Radius");
            }
        }

        private bool _radiusIsValid = true;
        /// <summary>
        /// Represents status that indicates whether radius value is valid or not.
        /// </summary>
        /// <value>Gets/Sets _radius data member.</value>
        public bool RadiusIsValid
        {
            get { return _radiusIsValid; }
            set
            {
                _radiusIsValid = value;
                RaisePropertyChanged("RadiusIsValid");
            }
        }

        private bool _isEnabled;
        /// <summary>
        /// Represents status of location filter.
        /// </summary>
        /// <value>Gets/Sets _isEnabled data member.</value>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
        }

        /// <summary>
        /// Triggers when property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Checks if all data necessary for location filter is set.
        /// </summary>
        /// <returns>True if all necessary data values are correct, false in any other case.</returns>
        public bool ValuesAreCorrect()
        {
            if (IsEnabled)
            {
                RadiusIsValid = RadiusIsValid && (Radius.Length != 0);
                if (RadiusIsValid)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if input location matches selected location filter.
        /// </summary>
        /// <param name="latitude">Geographic coordinate as string.</param>
        /// <param name="longitude">Geographic coordinate as string.</param>
        /// <returns>True if date matches query, false in any other case.</returns> 
        public bool IsMatching(string latitude, string longitude)
        {
            if (!IsEnabled)
            {
                return true;
            }
            if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
            {
                return false;
            }
            GeoCoordinate resultCoordinates = new GeoCoordinate(Convert.ToDouble(latitude), Convert.ToDouble(longitude));
            GeoCoordinate areaCoordinates = new GeoCoordinate(Convert.ToDouble(Latitude), Convert.ToDouble(Longitude));

            double distance = areaCoordinates.GetDistanceTo(resultCoordinates);
            if (distance < Convert.ToDouble(Radius))
            {
                return true;
            }
            return false;
        }
    }
}
