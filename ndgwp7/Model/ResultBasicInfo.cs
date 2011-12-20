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

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Stores basic data of survey result.
    /// </summary>
    public class ResultBasicInfo
    {
        /// <summary>
        /// Represents name of result.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Represents identification number of result.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Represents time when result was created (in miliseconds).
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// Represents geographic coordinate.
        /// </summary>
        public string Latitude { get; set; }
        /// <summary>
        /// Represents geographic coordinate.
        /// </summary>
        public string Longitude { get; set; }
        /// <summary>
        /// Represents identification number of survey that this result belongs to.
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        /// Represents whether all questions in results are answered correctly or not.
        /// </summary>
        public bool IsResultCompleted { get; set; }
        /// <summary>
        /// Represents whether result was already sent or not.
        /// </summary>
        public bool IsResultSent { get; set; }

        /// <summary>
        /// Represents current status of result.
        /// </summary>
        /// <value>Gets <see cref="ResultStatus"/> based on <see cref="IsResultCompleted"/> and <see cref="IsResultSent"/> values.</value>
        public ResultStatus Status
        {
            get
            {
                if (IsResultSent)
                {
                    return ResultStatus.Sent;
                }
                else
                {
                    return ResultStatus.ReadyToSend;
                }
            }
        }

        /// <summary>
        /// Represents current status of result.
        /// </summary>
        public enum ResultStatus
        {
            /// <summary>
            /// Indicates that result has already been sent.
            /// </summary>
            Sent,
            /// <summary>
            /// Indicates that result is correct and can be send.
            /// </summary>
            ReadyToSend,
            /// <summary>
            /// Indicates that result is not correct and cannot be send.
            /// </summary>
            NotReadyToSend
        }
    }
}
