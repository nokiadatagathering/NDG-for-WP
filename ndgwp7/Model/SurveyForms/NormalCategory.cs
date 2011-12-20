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
using System.Linq;
using System.Xml.Linq;

namespace com.comarch.mobile.ndg.Model.SurveyForms
{
    /// <summary>
    /// Stores basic data of survey's normal category.
    /// </summary>
    public class NormalCategory : Category
    {
        /// <summary>
        /// Initializes all data members. Allows to set parent survey.
        /// </summary>
        /// <param name="parent"><see cref="Survey"/> instance that category belongs to.</param>
        public NormalCategory(Survey parent)
        {
            Parent = parent;
            Questions = new List<Question>();
        }
        /// <summary>
        /// Represents instance of <see cref="Survey"/> class that category belongs to.
        /// </summary>
        public Survey Parent { get; private set; }
        /// <summary>
        /// Represents list of questions that category contains.
        /// </summary>
        public List<Question> Questions { get; set; }
        /// <summary>
        /// Represents identification number of category.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Represents name of category.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Represents status that indicates whether category was visited or not.
        /// </summary>
        public bool Visited { get; set; }
        /// <summary>
        /// Takes care of visual state of questions assigned as skip logic.
        /// </summary>
        /// <param name="isShown">Indicates status questions should be changed to.</param>
        /// <param name="skipFrom">Indicates question id to skip from.</param>
        /// <param name="skipTo">Indicates question id to skip to.</param>
        public void HideOrShowQuestions(bool isShown, int skipFrom, int skipTo)
        {
            var questionsToHideOrShow = from question in Questions where question.Id > skipFrom && question.Id < skipTo select question;
            foreach (Question question in questionsToHideOrShow)
            {
                question.IsEnabled = isShown;
            }
        }

        /// <summary>
        /// Updates questions visibility.
        /// </summary>
        public void RefreshQuestionsVisibility()
        {
            var exclusiveChoiceQuestions = from question in Questions where question.GetType() == typeof(ExclusiveChoiceQuestion) select question;
            foreach (ExclusiveChoiceQuestion question in exclusiveChoiceQuestions)
            {
                question.RefershSkipLogic();
            }
        }

        /// <summary>
        /// Adds category result to xml file.
        /// </summary>
        /// <param name="parent">Xml node that contains category data.</param>
        /// <returns>True if result was added successfully, in any other case false. </returns>
        public bool AddResult(XElement parent)
        {
            if (parent.Name == "category")
            {
                XElement newParent = new XElement("subcategory", new XAttribute("subCatId", 1));
                parent.Add(newParent);
                parent = newParent;
            }
            bool isCompleted = Visited;
            foreach (Question question in Questions)
            {
                string type = QuestionType(question);
                string questionId = string.Format("{0}", question.Id);
                XElement questionXElement = new XElement("answer", new XAttribute("type", type), new XAttribute("id", questionId), new XAttribute("visited", Visited.ToString().ToLower()));
                
                if (!question.AddResult(questionXElement))
                    isCompleted = false;

                parent.Add(questionXElement);
            }
            return isCompleted;
        }

        /// <summary>
        /// Reads last category.
        /// </summary>
        /// <param name="parent">Xml node that contains category data.</param>
        public void ReadLastResult(XElement parent)
        {
            if (parent.Name == "category")
            {
                parent = parent.Element("subcategory");
            }
            foreach (XElement questionXML in parent.Elements("answer"))
            {
                if (!Visited)
                {
                    bool visited = Convert.ToBoolean(questionXML.Attribute("visited").Value);
                    if (visited)
                        Visited = visited;
                }
                int id = Convert.ToInt32(questionXML.Attribute("id").Value);
                Question question = Questions[id - 1];
                question.ReadLastResult(questionXML);
            }
        }

        private string QuestionType(Question question)
        {
            if (question is DateQuestion)
                return "_date";
            else if (question is DescriptiveQuestion)
                return "_str";
            else if (question is NumericQuestion)
            {
                if (((NumericQuestion)question).Type == NumericQuestion.Types.DecimalType)
                    return "_decimal";
                else
                    return "_int";
            }
            else if (question is ImageQuestion)
                return "_img";
            else if (question is TimeQuestion)
                return "_time";
            else if (question is ExclusiveChoiceQuestion || question is MultipleChoiceQuestion)
                return "_choice";
            else return string.Empty;
        }
        /// <summary>
        /// Represents status that indicates whether all question in category are answered correctly or not.
        /// </summary>
        public bool IsResultCorrect
        {
            get
            {
                foreach (Question question in Questions)
                {
                    if (question.IsEnabled)
                    {
                        if (!question.IsCorrectAnswer)
                            return false;
                    }
                }
                return true;
            }
        }

    }
}
