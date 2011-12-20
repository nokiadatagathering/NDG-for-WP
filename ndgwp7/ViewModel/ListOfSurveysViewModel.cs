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
    /// Class stores methods used during communication between model and view on ListOfSurvey.
    /// </summary>
    public class ListOfSurveysViewModel
    {
        /// <summary>
        /// Represents all survey available for user (saved in IsolatedStorage).
        /// </summary>
        public ObservableCollection<SurveyBasicInfo> List { get; set; }
        /// <summary>
        /// Represents instance of <see cref="DownloadListStatus"/> class.
        /// </summary>
        public DownloadListStatus DownloadStatus { get; set; }
        /// <summary>
        /// Represents instance of <see cref="TestConnection"/> class.
        /// </summary>
        public TestConnection Connection { get; set; }
        private OperationsOnListOfSurveys _operations;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ListOfSurveysViewModel()
        {
            List = new ObservableCollection<SurveyBasicInfo>();
            DownloadStatus = new DownloadListStatus();
            Connection = new TestConnection();
            _operations = new OperationsOnListOfSurveys(List, DownloadStatus);
        }

        /// <summary>
        /// Reads information about saved/available surveys.
        /// </summary>
        public void Read()
        {
            _operations.Read();
        }


    }
}
