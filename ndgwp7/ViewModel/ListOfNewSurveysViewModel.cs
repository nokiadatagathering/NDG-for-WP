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
using System.Collections.ObjectModel;
using com.comarch.mobile.ndg.Model;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Class stores methods used during communication between model and view on ListOfNewSurvey.
    /// </summary>
    public class ListOfNewSurveysViewModel
    {
        /// <summary>
        /// Represents all surveys available for user (saved in IsolatedStorage).
        /// </summary>
        public ObservableCollection<SurveyBasicInfo> List { get; set; }
        /// <summary>
        /// Represents instance of <see cref="DownloadListStatus"/> class.
        /// </summary>
        public DownloadListStatus DownloadListStatus { get; set; }
        /// <summary>
        /// Represents instance of <see cref="DownloadSurveysStatus"/> class.
        /// </summary>
        public DownloadSurveysStatus DownloadSurveysStatus { get; set; }
        /// <summary>
        /// Represents instance of <see cref="OperationsOnListOfSurveys"/> class.
        /// </summary>
        public OperationsOnListOfSurveys Operations { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ListOfNewSurveysViewModel()
        {
            List = new ObservableCollection<SurveyBasicInfo>();
            DownloadListStatus = new DownloadListStatus();
            Operations = new OperationsOnListOfSurveys(List, DownloadListStatus);
            DownloadSurveysStatus = new DownloadSurveysStatus();
        }

    }
}
