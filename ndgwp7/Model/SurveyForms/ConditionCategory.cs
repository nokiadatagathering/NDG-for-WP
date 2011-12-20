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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Linq;

namespace com.comarch.mobile.ndg.Model.SurveyForms
{
    /// <summary>
    /// Stores basic data of survey's condition category.
    /// </summary>
    public class ConditionCategory : Category
    {
        /// <summary>
        /// Initializes all data members. Allows to set parent survey.
        /// </summary>
        /// <param name="parent"><see cref="Survey"/> instance that category belongs to.</param>
        public ConditionCategory(Survey parent)
        {
            Parent = parent;
            QuestionsTemplate = new List<Question>();
            SubCategories = new ObservableCollection<NormalCategory>();
        }
        /// <summary>
        /// Represents instance of <see cref="Survey"/> class that category belongs to.
        /// </summary>
        public Survey Parent { get; private set; }

        /// <summary>
        /// Represents identification number of category.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Represents name of category.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Represents condition question displayed in condition category view.
        /// </summary>
        public string ConditionQuestion { get; set; }
        /// <summary>
        /// Represents status that indicates whether category was visited or not.
        /// </summary>
        public bool Visited { get; set; }

        /// <summary>
        /// Represents list of subcategories.
        /// </summary>
        public ObservableCollection<NormalCategory> SubCategories { get; private set; }
        /// <summary>
        /// Represents template used to create questions in subcategories.
        /// </summary>
        public List<Question> QuestionsTemplate { get; private set; }

        /// <summary>
        /// Creates subcategories.
        /// </summary>
        /// <param name="from">Start index.</param>
        /// <param name="to">End index.</param>
        public void MakeSubCategories(int from, int to)
        {
            for (int i = from; i < to; ++i)
            {
                NormalCategory newCategory = new NormalCategory(Parent) { Id = string.Format("{0}_{1}", this.Id, i + 1), Name = string.Format("No. {0}", i + 1) };
                List<Question> copyOfQuestions = PrepareQuestionsCopy(newCategory);
                newCategory.Questions = copyOfQuestions;
                SubCategories.Add(newCategory);
                newCategory.RefreshQuestionsVisibility();
            }
            Parent.IsResultChanged = true;
        }

        private List<Question> PrepareQuestionsCopy(NormalCategory owner)
        {
            List<Question> newList = new List<Question>(QuestionsTemplate.Count);
            foreach (Question question in QuestionsTemplate)
            {
                newList.Add(question.Copy(owner));
            }
            return newList;
        }

        /// <summary>
        /// Adds category result to xml file.
        /// </summary>
        /// <param name="parent">Xml node that contains category data.</param>
        /// <returns>True if result was added successfully, in any other case false. </returns>
        public bool AddResult(XElement parent)
        {
            bool isCompleted = Visited;
            foreach (Category category in SubCategories)
            {
                string catIdAnsSubCatId = category.Id;
                string[] tokens = catIdAnsSubCatId.Split('_');
                string subCatId = tokens[1];
                XElement subCategory = new XElement("subcategory", new XAttribute("subCatId", subCatId));
                

                if (!category.AddResult(subCategory))
                {
                    isCompleted = false;
                }
                parent.Add(subCategory);
            }
            return isCompleted;
        }

        /// <summary>
        /// Reads last category.
        /// </summary>
        /// <param name="parent">Xml node that contains category data.</param>
        public void ReadLastResult(XElement parent)
        {
            var elements = parent.Elements("subcategory");
            int howMany = 0;
            foreach (XElement element in elements)
            {
                ++howMany;
            }
            if (howMany != 0)
            {
                MakeSubCategories(0, howMany);
                Visited = true;
                foreach (XElement subCategory in elements)
                {
                    string subCatIdStr = subCategory.Attribute("subCatId").Value;
                    int subCatId = Convert.ToInt32(subCatIdStr);
                    SubCategories[subCatId - 1].ReadLastResult(subCategory);
                    if (!SubCategories[subCatId - 1].Visited)
                        Visited = false;
                }
            }
        }

        /// <summary>
        /// Represents status that indicates whether all question in category are answered correctly or not.
        /// </summary>
        public bool IsResultCorrect 
        {
            get
            {
                if (SubCategories != null)
                {
                    foreach (NormalCategory category in SubCategories)
                    {
                        if (!category.IsResultCorrect)
                            return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Removes top x subcategories, where x is defined by input parameter.
        /// </summary>
        /// <param name="howMany">Indicates how many subcategories you want to remove.</param>
        public void RemoveLastSubcategories(int howMany)
        {
            int count = SubCategories.Count;
            for (int i = count - 1; i > count - howMany - 1; --i)
            {
                SubCategories.RemoveAt(i);
            }
            Parent.IsResultChanged = true;
        }
    }
}
