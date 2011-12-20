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
using System.Xml.Linq;

namespace com.comarch.mobile.ndg.Model.SurveyForms
{
    /// <summary>
    /// Stores basic data of survey's category.
    /// </summary>
    public interface Category
    {
        /// <summary>
        /// Represents instance of <see cref="Survey"/> class. Holds data of survey that category belongs to.
        /// </summary>
        Survey Parent { get; }
        /// <summary>
        /// Represents category identification number.
        /// </summary>
        string Id { get; set; }
        /// <summary>
        /// Represents category name.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Represents status that indicates whether category was visited by the user or not.
        /// </summary>
        bool Visited { get; set; }

        /// <summary>
        /// Represents status that indicates whether all question in category are answered correctly or not.
        /// </summary>
        bool IsResultCorrect { get; }
        /// <summary>
        /// Adds category result to xml file.
        /// </summary>
        /// <param name="parent">Xml node that contains category data.</param>
        /// <returns>True if result was added successfully, in any other case false. </returns>
        bool AddResult(XElement parent);
        /// <summary>
        /// Reads last category.
        /// </summary>
        /// <param name="parent">Xml node that contains category data.</param>
        void ReadLastResult(XElement parent);
    }
}
