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
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using ComponentAce.Compression.Libs.zlib;
using com.comarch.mobile.ndg.Model.SurveyForms;
using com.comarch.mobile.ndg.Settings.Model;

namespace com.comarch.mobile.ndg.Model
{
    /// <summary>
    /// Class responsible for sending survey results to server.
    /// </summary>
    public class SendResult
    {
        /// <summary>
        /// Triggers when sending result WebRequest is finished.
        /// </summary>
        public event EventHandler SendingCompleted;
        private XDocument _resultDocument;
        private string _resultId;
        /// <summary>
        /// Allows you to initialize all necessary data members and triggers sending process.
        /// </summary>
        /// <param name="surveyId">Survey identification number.</param>
        /// <param name="resultInfo">Instance of <see cref="ResultBasicInfo"/> class. Result you want to send.</param>
        public void Send(string surveyId, ResultBasicInfo resultInfo)
        {
            Survey survey = new Survey();
            survey.Id = surveyId;
            survey.ResultInfo = resultInfo;
            _resultId = resultInfo.Id;
            Send(survey.GetSavedDocument());
        }
        /// <summary>
        /// Terminates WebRequest. Stops sending process.
        /// </summary>
        public void AbortSaving()
        {
            if (_webRequest != null)
                _webRequest.Abort();
        }
        private WebRequest _webRequest;
        private void Send(XDocument resultDocument)
        {
            _resultDocument = resultDocument;

            string serverURL = OperationsOnSettings.Instance.ServerURL;
            WebRequest webRequest = WebRequest.Create(string.Format("{0}PostResults", serverURL));
            _webRequest = webRequest;
            webRequest.Method = "POST";
            webRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), webRequest);
        }

        private byte[] CompressResult(string strResult)
        {
            byte[] byteArrayIn = Encoding.UTF8.GetBytes(strResult);

            MemoryStream outStream = new MemoryStream();
            ZOutputStream zOutputStream = new ZOutputStream(outStream, zlibConst.Z_BEST_COMPRESSION);
            zOutputStream.Write(byteArrayIn, 0, byteArrayIn.Length);
            zOutputStream.Close();

            byte[] byteArrayOut = outStream.ToArray();

            byte[] compresedDataLengthBytes = BitConverter.GetBytes(byteArrayOut.Length);
            byte[] dataLengthBytes = BitConverter.GetBytes(strResult.Length);

            byte[] bytesToSend = new byte[byteArrayOut.Length + 8];

            bytesToSend[0] = dataLengthBytes[3];
            bytesToSend[1] = dataLengthBytes[2];
            bytesToSend[2] = dataLengthBytes[1];
            bytesToSend[3] = dataLengthBytes[0];
            bytesToSend[4] = compresedDataLengthBytes[3];
            bytesToSend[5] = compresedDataLengthBytes[2];
            bytesToSend[6] = compresedDataLengthBytes[1];
            bytesToSend[7] = compresedDataLengthBytes[0];
            int index = 8;
            foreach (byte b in byteArrayOut)
            {
                bytesToSend[index] = b;
                index++;
            }
            return bytesToSend;
        }

        private string GetStringFromXDocument()
        {
            string strWithDeclaration = string.Format("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n{0}", _resultDocument.ToString());
            return HTMLEncodeSpecialChars(strWithDeclaration);
        }

        private string HTMLEncodeSpecialChars(string text)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in text)
            {
                if (c > 127) // special chars
                    sb.Append(string.Format("&#{0};", (int)c));
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Callback used to process request stream.
        /// </summary>
        /// <param name="asynchronousResult">Server response state.</param>
        protected void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            WebRequest webRequest = (WebRequest)asynchronousResult.AsyncState;
            Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);
            string postData = GetStringFromXDocument();
            byte[] bytesToSend = CompressResult(postData);
            postStream.Write(bytesToSend, 0, bytesToSend.Length);
            postStream.Close();
            
            webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), webRequest);
        }

        /// <summary>
        /// Callback used to process server response.
        /// </summary>
        /// <param name="asynchronousResult">Server response state.</param>
        protected void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            SendingEventArgs args = new SendingEventArgs();
            args.ResultId = _resultId;
            try
            {
                WebRequest webRequest = (WebRequest)asynchronousResult.AsyncState;
                HttpWebResponse response;

                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);
                Stream streamResponse = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(streamResponse);
                string Response = streamReader.ReadToEnd();

                byte[] bytes = Encoding.UTF8.GetBytes(Response);

                StringBuilder builder = new StringBuilder();
                foreach (var i in bytes)
                    builder.Append(i.ToString());

                string temp = builder.ToString();
                if (temp == "0001")
                {
                    args.Status = SendingEventArgs.SendingStatus.Sent;
                }
                else
                {
                    args.Status = SendingEventArgs.SendingStatus.ServerError;
                }
                streamResponse.Close();
                streamReader.Close();
                response.Close();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.RequestCanceled)
                {
                    args.Status = SendingEventArgs.SendingStatus.Canceled;
                }
                else
                {
                    args.Status = SendingEventArgs.SendingStatus.UnknownError;
                }
            }
            finally
            {
                EventHandler handler = SendingCompleted;
                if (handler != null)
                    handler(this, args);
            }
        }
        /// <summary>
        /// Class stores arguments that you can send by SendingCompleted event.
        /// </summary>
        public class SendingEventArgs : EventArgs
        {
            /// <summary>
            /// Represents result of sending process.
            /// </summary>
            public enum SendingStatus
            {
                /// <summary>
                /// Indicates that sending process was canceled by the user.
                /// </summary>
                Canceled, 
                /// <summary>
                /// Indicates that result has been send successfully.
                /// </summary>
                Sent,
                /// <summary>
                /// Indicates that server error has occured during sending process.
                /// </summary>
                ServerError,
                /// <summary>
                /// Indicates that unknown error has occured during sending process.
                /// </summary>
                UnknownError
            }
            /// <summary>
            /// Represents instance of <see cref="SendingStatus"/> enum.
            /// </summary>
            public SendingStatus Status { get; set; }
            /// <summary>
            /// Represents result id that has been send.
            /// </summary>
            public string ResultId { get; set; }
            /// <summary>
            /// Initializes all necessary data objects.
            /// </summary>
            public SendingEventArgs()
            {
                Status = SendingStatus.UnknownError;
            }
        }

    }
}
