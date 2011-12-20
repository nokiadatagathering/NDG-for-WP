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
    /// Stores question data.
    /// </summary>
    public interface Question
    {
        /// <summary>
        /// Represents instance of <see cref="Category"/> that question belongs to.
        /// </summary>
        Category Parent { get; }
        /// <summary>
        /// Represents question descriptions.
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// Represents identification number of question.
        /// </summary>
        int Id { get; set; }
        /// <summary>
        /// Represents status that indicates whether question is enabled or not.
        /// </summary>
        bool IsEnabled { get; set; }
        /// <summary>
        /// Represents status that indicates whether answer is correct or not.
        /// </summary>
        bool IsCorrectAnswer { get; set; }

        /// <summary>
        /// Creates a copy of the question.
        /// </summary>
        /// <param name="parent">Instance of <see cref="Category"/> that question belongs to.</param>
        /// <returns>New instance of question.</returns>
        Question Copy(Category parent);
        /// <summary>
        /// Adds question result to xml file.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        /// <returns>True if result was added successfully, in any other case false. </returns>
        bool AddResult(XElement parent);
        /// <summary>
        /// Reads last answer.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        void ReadLastResult(XElement parent);
    }
}
