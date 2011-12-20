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
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.Phone.Tasks;
using com.comarch.mobile.ndg.MessageDialog;
using com.comarch.mobile.ndg.Settings.Model;
using com.comarch.mobile.ndg.ViewModel;

namespace com.comarch.mobile.ndg.Model.SurveyForms
{
    /// <summary>
    /// Stores image question data.
    /// </summary>
    public class ImageQuestion : Question, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes all data members. Allows to set parent category.
        /// </summary>
        /// <param name="parent"><see cref="Category"/> instance that question belongs to.</param>
        public ImageQuestion(Category parent)
        {
            Parent = parent;
            _isEnabled = true;
            IsCorrectAnswer = true;
            Message = new DialogBox();
            ImageItems = new ObservableCollection<ImageItem>();
            ChooserTask = new PhotoChooserTask();
            CameraTask = new CameraCaptureTask();
            ChooserTask.Completed += TaskCompletedCallback;
            CameraTask.Completed += TaskCompletedCallback;
        }
        /// <summary>
        /// Represents instance of <see cref="Category"/> that question belongs to.
        /// </summary>
        public Category Parent { get; private set; }

        /// <summary>
        /// Represents question descriptions.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Represents identification number of question.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Represenst instance of <see cref="DialogBox"/> class.
        /// </summary>
        public DialogBox Message {get; set;}
        private int _maxCount;
        /// <summary>
        /// Represents max amount of image that question can contain.
        /// </summary>
        /// <value>Gets/Sets _maxCount data member.</value>
        public int MaxCount 
        {
            get
            {
                return _maxCount;
            }
            set
            {
                _maxCount = value;
                UpdateStatus();
            }
        }

        /// <summary>
        /// Represents instance of <see cref="PhotoChooserTask"/> class. Used to browse image gallery.
        /// </summary>
        public PhotoChooserTask ChooserTask { get; set; }
        /// <summary>
        /// Represents instance of <see cref="CameraCaptureTask"/> class. Used to take photos.
        /// </summary>
        public CameraCaptureTask CameraTask { get; set; }

        private string _status;
        /// <summary>
        /// Represents current question image status.
        /// </summary>
        /// <value>Gets/Sets _status data member.</value>
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        private void UpdateStatus()
        {
            Status = string.Format(Languages.AppResources.imageQuestion_Images, ImageItems.Count, MaxCount);
            Parent.Parent.IsResultChanged = true;
        }

        private bool _isEnabled;
        /// <summary>
        /// Represents status that indicates whether answer is correct or not.
        /// </summary>
        /// <value>Gets/Sets _isEnabled data member.</value>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
        }

        /// <summary>
        /// Represents status that indicates whether question answer is correct or not.
        /// </summary>
        public bool IsCorrectAnswer { get; set; }

        /// <summary>
        /// Represents list of images assigned to this question.
        /// </summary>
        public ObservableCollection<ImageItem> ImageItems { get; set; }

        /// <summary>
        /// Removes image from assigned images list.
        /// </summary>
        /// <param name="item">Item that you want to remove.</param>
        public void DeleteImage(ImageItem item)
        {
            ImageItems.Remove(item);
            UpdateStatus();
        }

        /// <summary>
        /// Callback used to process chooser tasks results.
        /// </summary>
        /// <param name="sender">Sender of photo result - chooser task.</param>
        /// <param name="photoResult">Instance of <see cref="PhotoResult"/> class. Contains selected image.</param>
        public void TaskCompletedCallback(object sender, PhotoResult photoResult)
        {
            if (photoResult.TaskResult == TaskResult.OK)
            {
                BitmapImage bitmap = new BitmapImage();
                try
                {
                    bitmap.SetSource(photoResult.ChosenPhoto);
                }
                catch (ArgumentNullException)
                {
                    Message.Show(Languages.AppResources.imageQuestion_ImageIsNull);
                }
                ImageItems.Add(new ImageQuestion.ImageItem() { ChosenImage = bitmap });//_imageOperator.ResizeImage(bitmap, OperationsOnSettings.Instance.PhotoHeight, OperationsOnSettings.Instance.PhotoWidth, 100, true) });
                UpdateStatus();
                Parent.Parent.TakingPhoto = false;
            }
            else if (photoResult.TaskResult == TaskResult.Cancel)
            {
               
            }
        }

        void detector_MediaFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compares max image count with current image count.
        /// </summary>
        /// <returns>True if image count is larger or equal than max image count, in any other case returns false.</returns>
        public bool IsFull()
        {
            if (ImageItems.Count >= MaxCount)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Triggers when property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string arg)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(arg));
            }
        }

        /// <summary>
        /// Adds question result to xml file.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        /// <returns>True if result was added successfully, in any other case false. </returns>
        public bool AddResult(XElement parent)
        {
            if (IsEnabled)
            {
                foreach (ImageItem item in ImageItems)
                {
                    try
                    {
                        AddItemToParent( parent,  item);
                    }
                    catch (UnauthorizedAccessException) // that means operations are in different thread
                    {
                        ManualResetEvent allDone = new ManualResetEvent(false);
                        System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            AddItemToParent( parent,  item);
                            allDone.Set();
                        });
                        allDone.WaitOne();
                    }
                }
            }
            return (ImageItems.Count > 0) || !IsEnabled;
        }
        private void AddItemToParent(XElement parent, ImageItem item)
        {
            ImageOperations operationsOnImage = new ImageOperations();
            BitmapImage image =  operationsOnImage.ResizeImage(item.ChosenImage, OperationsOnSettings.Instance.PhotoWidth, OperationsOnSettings.Instance.PhotoHeight, 100, true);
            byte[] imageByte = operationsOnImage.ConvertImageToBytes(image);
            System.Text.Encoding enc = System.Text.Encoding.Unicode;
            string imageString = System.Convert.ToBase64String(imageByte);
            XElement imageElement = new XElement("img_data");
            imageElement.Value = imageString;
            parent.Add(imageElement);
        }

        /// <summary>
        /// Reads last answer.
        /// </summary>
        /// <param name="parent">Xml node that contains question data.</param>
        public void ReadLastResult(XElement parent)
        {
            foreach (XElement img in parent.Elements("img_data"))
            {
                string strImg = img.Value;
                byte[] encodedDataAsBytes = System.Convert.FromBase64String(strImg);
                ImageItems.Add(new ImageItem() { ChosenImage = ImageFromBuffer(encodedDataAsBytes) });
            }
            UpdateStatus();
        }

        /// <summary>
        /// Converts byte array to <see cref="BitmapImage"/> class.
        /// </summary>
        /// <param name="bytes">Image save as array of bytes.</param>
        /// <returns>Instance of <see cref="BitmapImage"/> class.</returns>
        public BitmapImage ImageFromBuffer(Byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage image = new BitmapImage();
            image.SetSource(stream);
            return image;
        }

        /// <summary>
        /// Stores information of single image item.
        /// </summary>
        public class ImageItem : INotifyPropertyChanged
        {
            private BitmapImage _chosenImage;
            /// <summary>
            /// Represents instance of <see cref="BitmapImage"/> class.
            /// </summary>
            /// <value>Gets/Sets _chosenImage data member.</value>
            public BitmapImage ChosenImage
            {
                get
                {
                    return _chosenImage;
                }
                set
                {
                    _chosenImage = value;
                    RaisePropertyChanged("ChosenImage");
                }
            }

            /// <summary>
            /// Triggers when property value is changed.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;
            private void RaisePropertyChanged(string arg)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(arg));
                }
            }
        }

        /// <summary>
        /// Creates a copy of the question.
        /// </summary>
        /// <param name="parent">Instance of <see cref="Category"/> that question belongs to.</param>
        /// <returns>New instance of question.</returns>
        public Question Copy(Category parent)
        {
            return new ImageQuestion(parent) { Description = this.Description, Id = this.Id, MaxCount = this.MaxCount };
        }
    }
}
