using com.hexaware.appwithfb.AppWithFB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Windows;
using Windows.Storage;
using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;
using WPCordovaClassLib.Cordova.JSON;

namespace Common
{
    public static class ServerInformation
    {
        public static string ServerIPAddress = "http://115.69.83.116:8080/";
    }

    public static class PushNotificationData
    {
        public static Uri PushChannelUri { get; set; }
    }

    public static class PhoneInformation
    {
        public static string DeviceUniqueId = Convert.ToBase64String((byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId"));
        public static string ANID2 = Convert.ToString(Microsoft.Phone.Info.UserExtendedProperties.GetValue("ANID2"));
    }

    public static class AppInformation
    {
        static string FullName = Assembly.GetExecutingAssembly().FullName;
		static string NameSpace = FullName.Substring(0, FullName.IndexOf(","));
    public static string AppName = NameSpace.Substring(NameSpace.LastIndexOf(".") + 1); 
        public static string AppVersion = FullName.Split('=')[1].Split(',')[0].ToString().Substring(0, 3);
    }


    /// <summary>
    /// Provides access to Properties file generated from Java Eclipse IDE.
    /// </summary>
    public class Properties
    {
        public Dictionary<string, string> AllProperties { get; set; }

        /// <summary>
        /// Loads Property file content from the specified path
        /// </summary>
        /// <param name="PropertyFilePath"></param>
        /// <returns>void</returns>
        public void Load(string PropertyFilePath, string delimiter = "=")
        {
            try
            {
                if (System.IO.File.Exists(PropertyFilePath))
                {
                    StreamReader file = new StreamReader(PropertyFilePath);

                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        // Skip blank lines & unformatted lines
                        if (line.Contains(delimiter))
                        {
                            AllProperties.Add(line.Substring(0, line.IndexOf(delimiter)), line.Substring(line.IndexOf(delimiter) + 1));
                        }
                    }
                    file.Close();
                }
                else
                {
                    throw new FileNotFoundException("Property file not found in the following path. " + PropertyFilePath);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }

        /// <summary>
        /// Returns Value of the given Property Name
        /// </summary>
        /// <param name="PropertyName">Name of the Property</param>
        /// <returns>Value of the Property</returns>
        public string GetProperty(string PropertyName)
        {
            return AllProperties[PropertyName];
        }
    }

    /// <summary>
    /// Provides quick access to IsolatedStorageSettings
    /// </summary>
    public static class AppSettings
    {
        /// <summary>
        /// Adds or Updates an entry in the dictionary.
        /// </summary>
        /// <param name="SettingKey">The key for the entry to be stored</param>
        /// <param name="SettingValue">The value to be stored</param>
        public static void Save(string SettingKey, string SettingValue)
        {
            IsolatedStorageSettings objSettings = IsolatedStorageSettings.ApplicationSettings;
            if (!objSettings.Contains(SettingKey))
            {
                objSettings.Add(SettingKey, SettingValue);
            }
            else
            {
                objSettings[SettingKey] = SettingValue;
            }
            objSettings.Save();
        }

        /// <summary>
        /// Returns the value of the given key
        /// </summary>
        /// <param name="SettingKey">The key of the entry </param>
        /// <returns>value of the given key</returns>
        public static string GetValue(string SettingKey)
        {
            return IsolatedStorageSettings.ApplicationSettings[SettingKey] as string;
        }

        public static void Remove(string SettingKey)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(SettingKey))
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(SettingKey);
            }
        }
    }


    /// <summary>
    ///  All methods in HexPlugins can be invoked from Java script
    /// </summary>
    class HexPlugins : BaseCommand
    {
        /// <summary>
        /// Returns Device Unique ID
        /// </summary>
        /// <param name="options">Arguments not required</param>
        public void GetDeviceUniqueID(string options)
        {
            try
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.OK, PhoneInformation.DeviceUniqueId));
                System.Diagnostics.Debug.WriteLine("   Device UniqueId Dispatched !!");
            }
            catch (Exception ex)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Error occured.! " + ex.Message.ToString()));
            }
        }

        /// <summary>
        /// Returns ChannelUri
        /// </summary>
        /// <param name="options">Arguments not required</param>
        public void GetChannelUri(string options)
        {
            try
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.OK, PushNotificationData.PushChannelUri));
                System.Diagnostics.Debug.WriteLine(" PushChannelUri Dispatched !!");
            }
            catch (Exception ex)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Error occured.! " + ex.Message.ToString()));
            }
        }

        /// <summary>
        /// Sends Mobile to Mobile message
        /// </summary>
        /// <param name="options">  [strMsgTitle, strMsg, strMsgType, RecieverUserID, AppName] </param>
        public void SendMobileToMobileMessge(string options)
        {
            try
            {
                string strMsgTitle = JsonHelper.Deserialize<string[]>(options)[0];
                string strMsg = JsonHelper.Deserialize<string[]>(options)[1];
                string strMsgType = JsonHelper.Deserialize<string[]>(options)[2];
                string RecieverUserID = JsonHelper.Deserialize<string[]>(options)[3];
                string AppName = JsonHelper.Deserialize<string[]>(options)[4];

                MobileToMobileCommunication.SendMessage(strMsgTitle, strMsg, strMsgType, RecieverUserID, AppName);

                DispatchCommandResult(new PluginResult(PluginResult.Status.OK, "Message Sent"));
                System.Diagnostics.Debug.WriteLine("   M2M message Sent via HexPlugin !!");
            }
            catch (Exception ex)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Message not Sent ! Error occured.! " + ex.Message.ToString()));
            }
        }

        /// <summary>
        /// Returns Mobile to Mobile messages from JSON data file
        /// </summary>
        /// <param name="options">Arguments not required</param>
        public void ReadMobileToMobileMessage(string options)
        {
            try
            {
                //    if (IsolatedStorageSettings.ApplicationSettings.Contains("MToM_Title") && IsolatedStorageSettings.ApplicationSettings.Contains("MToM_Message"))
                //    {
                //        string MtomTitle = IsolatedStorageSettings.ApplicationSettings["MToM_Title"].ToString();
                //        string MtomMsg = IsolatedStorageSettings.ApplicationSettings["MToM_Message"].ToString();
                //        DispatchCommandResult(new PluginResult(PluginResult.Status.OK, MtomTitle + "  :  " + MtomMsg));  }

                DispatchCommandResult(new PluginResult(PluginResult.Status.OK, MobileToMobileCommunication.ReadMessages()));
                System.Diagnostics.Debug.WriteLine("Success :  ReadMobileToMobileMessage !!");

            }
            catch (Exception ex)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Error occured.! " + ex.Message.ToString()));
            }
        }

    }


}
