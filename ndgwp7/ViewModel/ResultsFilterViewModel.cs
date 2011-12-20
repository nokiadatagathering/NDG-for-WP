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
using com.comarch.mobile.ndg.BusyIndicator;
using com.comarch.mobile.ndg.MessageDialog;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Class stores methods used during communication between model and view on ResultsFilter.
    /// </summary>
    public class ResultsFilterViewModel
    {
        /// <summary>
        /// Represents instance of <see cref="ResultsFilter"/> class.
        /// </summary>
        public ResultsFilter Filter { get; set; }
        /// <summary>
        /// Represents instance of <see cref="ProcessingBar"/> class.
        /// </summary>
        public ProcessingBar ProgressBar { get; set; }
        /// <summary>
        /// Represents information (true/false) about sending result on server.
        /// </summary>
        public bool IsSending { get; set; }
        /// <summary>
        /// Represents instance of <see cref="SendResult"/> class.
        /// </summary>
        public SendResult ResultSender { get; set; }
        /// <summary>
        /// Represents instance of <see cref="OperationsOnListOfResults"/> class.
        /// </summary>
        public OperationsOnListOfResults Operations { get; set; }

        /// <summary>
        /// Constructor which initializes Filter property using input field.
        /// </summary>
        /// <param name="list">List of surveys which are used during filtering operation.</param>
        public ResultsFilterViewModel(ObservableCollection<SurveyBasicInfo> list)
        {
            Filter = new ResultsFilter(list);
        }

        /// <summary>
        /// Aborts sending result to server.
        /// </summary>
        public void AbortSending()
        {
            if (IsSending)
            {
                ResultSender.AbortSaving();
            }
        }

        /// <summary>
        /// Sends result to server.
        /// </summary>
        /// <param name="selectedListBoxItem">Contains information of result that was selected by user and will be send on server.</param>
        public void SendResult(ResultBasicInfo selectedListBoxItem)
        {
            ProgressBar.Information = Languages.AppResources.surveyViewModel_sendingProgressTitle;
            ProgressBar.IsEnabled = true;
            Operations = new OperationsOnListOfResults(selectedListBoxItem.ParentId);
            IsSending = true;
            ResultSender.Send(selectedListBoxItem.ParentId, selectedListBoxItem);
        }

        /// <summary>
        /// Method to unmark result as sent. After unmark result as sent user can again modify the result.
        /// </summary>
        /// <param name="result">Information about which result was selected by user.</param>
        public void UnmarkSent(ResultBasicInfo result)
        {
            Operations = new OperationsOnListOfResults(result.ParentId);
            Operations.UnmarkSentResult(result.Id);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ResultsFilterViewModel()
        {
            Filter = new ResultsFilter();
            ProgressBar = new ProcessingBar();
            ResultSender = new SendResult();
        }
    }
}
