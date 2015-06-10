using Microsoft.Phone.Net.NetworkInformation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace com.hexaware.appwithfb.AppWithFB
{
    public class MobileToMobileCommunication
    {
        static string strMobToMobServiceURL = Common.ServerInformation.ServerIPAddress + "WorkQuikr-console/NotificationApi?message={0}&userId={1}&appName={2}&dummyId={3}";
        public static string MobToMobHistoryPath = "www\\LocalMobToMobHistory.json";


        /// <summary>
        /// Sends Mobile to Mobile Message via WorkQuickr console
        /// </summary>
        public static void SendMessage(string MessageTitle, string Message, string MessageType, string RecieverUserID, string AppName)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Debug.WriteLine(" Sending MobileToMobileMessage....");
                try
                {
                    //Create ChatMessage object.
                     //ToastMessage ObjToastMessage = new ToastMessage()
                    // {
                    //     messageTitle = RecieverUserID,
                    //     message = Message,
                     //    messageType = MessageType
                    // };

                    // string jsonMessage = MobToMobMessageJsonHelper.Serialize(ObjToastMessage);

                    Uri uri = new Uri(string.Format(strMobToMobServiceURL, Message, RecieverUserID, AppName, Guid.NewGuid().ToString()));

                    WebClient wcSendM2M = new WebClient();
                    wcSendM2M.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wcSendM2M_DownloadStringCompleted);
                    wcSendM2M.DownloadStringAsync(uri);
                    Debug.WriteLine("  MobileToMobileMessage.... Sent !!");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error occured on Sending MobileToMobileMessage. \n Details : {0}" + ex.Message);
                }
            }
            else
            {
                Debug.WriteLine("\n Network unavailable");
            }
        }


        /// <summary>
        /// Saves Recieved Message in Local JSON data file by appending the new message to the existing messages.
        /// </summary>
        public static void SaveMessage(string MessageTitle, string RecievedMessage)
        {
            ToastMessage objToastMessage = new ToastMessage();
            objToastMessage.message = RecievedMessage;
            objToastMessage.messageTitle = MessageTitle;
            objToastMessage.messageType = "mtom";

            // Serialize To JSON string
            string Json = MobToMobMessageJsonHelper.Serialize(objToastMessage);

            //  Create a file to write Header text only once to the file. 
            if (!File.Exists(MobToMobHistoryPath))
            {
                using (StreamWriter sw = File.CreateText(MobToMobHistoryPath))
                {
                    sw.WriteLine("{\"ToastMessageList\":[");
                }
            }

            // Append JSON string  (makes the file longer over time if it is not deleted.!) 
            using (StreamWriter sw = File.AppendText(MobToMobHistoryPath))
            {
                sw.WriteLine(Json + ",");
            }
        }


        /// <summary>
        /// Reads Messages from Json data file and returns formatted message conversation
        /// </summary>
        public static string ReadMessages()
        {
            if (File.Exists(MobToMobHistoryPath))
            {
                string strJson, strMessages = string.Empty;

                // Read from Json data file
                StreamReader file = new StreamReader(MobToMobHistoryPath);
                if ((strJson = file.ReadToEnd()) != null)
                {
                    file.Close();

                    strJson = strJson.Remove(strJson.LastIndexOf(","));
                    strJson += "\n]}";

                    // Deserialize JSON string as MobToMobHistory
                    ToastMessageHistory objToastMessageHistory = MobToMobMessageJsonHelper.Deserialize(strJson);

                    // Build Message List
                    foreach (var item in objToastMessageHistory.ToastMessageList)
                    {
                        // Customize your message conversation here
                        strMessages += "<b>" + item.messageTitle + "</b> : " + item.message + " <br />";
                    }
                    return strMessages;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                throw new FileNotFoundException("ToastMessageHistory Json data file not found");
            }
        }

        static void wcSendM2M_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    string strServerResponse = e.Result.ToString();      // e.Result contains server response
                }
                else
                {
                    Debug.WriteLine("Download error : " + e.Error.Message.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error on wcSendM2M_DownloadStringCompleted() :" + ex.Message);
            }
        }



        [DataContract]
        public class ToastMessage
        {
            [DataMember]
            public string messageTitle { get; set; }

            [DataMember]
            public string message { get; set; }

            [DataMember]
            public string messageType { get; set; }
        }

        [DataContract]
        public class ToastMessageHistory
        {
            [DataMember]
            public List<ToastMessage> ToastMessageList { get; set; }

        }

        
        /// <summary>
        ///  Serialize & Deserialize JSON
        ///  Reference URL : http:// msdn.microsoft.com/en-us/library/windowsphone/develop/system.runtime.serialization.json.datacontractjsonserializer(v=vs.105).aspx
        /// </summary>
        public static class MobToMobMessageJsonHelper
        {
            /// <summary>
            /// Serializes ToastMessage object to a JSON string. Returns Serialized JSON string.
            /// </summary>
            public static string Serialize(ToastMessage ObjToastMessage)
            {
                MemoryStream ms = new MemoryStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ToastMessage));
                ser.WriteObject(ms, ObjToastMessage);
                byte[] json = ms.ToArray();
                ms.Close();
                return Encoding.UTF8.GetString(json, 0, json.Length);
            }

            /// <summary>
            /// Deserializes JSON string to ToastMessageHistory object. Returns Deserialized ToastMessageHistory object.
            /// </summary>
            public static ToastMessageHistory Deserialize(string jsonString)
            {
                ToastMessageHistory objToastMessageHistory = new ToastMessageHistory();
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                DataContractJsonSerializer ser = new DataContractJsonSerializer(objToastMessageHistory.GetType());
                objToastMessageHistory = ser.ReadObject(ms) as ToastMessageHistory;
                ms.Close();
                return objToastMessageHistory;
            }
        }

    }
}
