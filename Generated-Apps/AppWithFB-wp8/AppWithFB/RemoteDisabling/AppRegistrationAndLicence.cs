using Common;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;

/* 
 * Developed by     :   Vignesh Raja M 
 * Created date     :   14-05-2013
 * Purpose          :   Remote Disabling
 * Description      :   Enable / Disable Application from server at any point of time 
 */

namespace com.hexaware.appwithfb.AppWithFB
{


    class AppRegistrationAndLicence : PhoneApplicationPage
    {
        #region Variable declaration

        string strAuthenticationURL = ServerInformation.ServerIPAddress + "WorkQuikr-console/RemoteDisableCheck?act=2&token={0}&dummyid={1}";
        string strRegisterURL = ServerInformation.ServerIPAddress + "WorkQuikr-console/RemoteDisableCheck?act=1&VersionID={0}&appName={1}&username={2}&deviceId={3}";
        bool blnIsRemoteDisableEnabled = false;
        WebClient wcAuthentication;

        #endregion



        #region Check For Authentication

        public void CheckForAuthentication()
        {
            try
            {
                if (blnIsRemoteDisableEnabled)
                {
                    bool blnIsNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("TokenSetting"))
                    {
                        //Already Registered  - (Token is available)
                        if (blnIsNetworkAvailable)
                        {
                            UpdateAuthenticationStatus();           //Update the Authentication status with live data.
                        }
                        else
                        {
                            AuthenticateAndOpenApplication();   // Open or Close Application based on saved Authentication status from IsolatedStorageSettings 
                        }
                    }
                    else
                    {
                        //Register - Token is not available
                        if (blnIsNetworkAvailable)
                        {
                            RegisterApplication();                          // Register the device using App and Device details                    
                        }
                        else
                        {
                            // Network unavailable , Show Notification and Exit
                            Dispatcher.BeginInvoke(() =>
                            {
                                MessageBox.Show("Please check the internet connectivity. Could not connect to the server to register the application. \nThe application will be closed.");
                                Application.Current.Terminate();
                            });


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Current.Terminate();
                Debug.WriteLine("** CheckForAuthentication() :  " + ex.ToString());
            }
        }


        #endregion

        #region Register Application

        private void RegisterApplication()
        {
            //  Register the app & Get Token  ( Call web service with parameters act, VersionID, appName, username , deviceId )
            Uri RegisterURI = new Uri(String.Format(strRegisterURL, AppInformation.AppVersion, AppInformation.AppName, PhoneInformation.ANID2, PhoneInformation.DeviceUniqueId));

            try
            {
                WebClient wcRegisterApp = new WebClient();
                wcRegisterApp.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wcRegisterApp_DownloadStringCompleted);
                wcRegisterApp.DownloadStringAsync(RegisterURI);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n*** Error occured on RegisterApplication() " + ex.Message);
            }
        }




        void wcRegisterApp_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                try
                {
                    // save the Token in IsolatedStorageSettings 
                    AppSettings.Save("TokenSetting", e.Result.ToString());

                    //Update the Authentication status with live data.
                    UpdateAuthenticationStatus();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("\n*** Error occured on wcRegisterApp_DownloadStringCompleted() " + ex.Message);
                }
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
               {
                   MessageBox.Show("Can not register the application due to communication failure.\nThe application will be closed.");
                   Application.Current.Terminate();
               });
            }

        }

        #endregion

        #region Update Authentication Status

        private void UpdateAuthenticationStatus()
        {
            string strToken = IsolatedStorageSettings.ApplicationSettings["TokenSetting"].ToString();

            // Update the Authentication status using using act, token (call web service)
            try
            {
                wcAuthentication = new WebClient();
                wcAuthentication.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wcAuthentication_DownloadStringCompleted);
                wcAuthentication.DownloadStringAsync(new Uri(String.Format(strAuthenticationURL, strToken, Guid.NewGuid().ToString())));  // To avoid caching problem in WP8
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n*** Error occured on UpdateAuthenticationStatus() " + ex.Message);
            }
        }

        void wcAuthentication_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                try
                {
                    // save the Authentication status in IsolatedStorageSettings 

                    AppSettings.Save("IsAuthenticatedSetting", e.Result.ToString());

                    // Open or Close Application based on saved Authentication status from IsolatedStorageSettings 
                    AuthenticateAndOpenApplication();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("\n*** Error occured on wcAuthentication_DownloadStringCompleted() " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Can not authenticate the application due to communication failure.");

                // Authenticate the application using Isolated Storage then Open Application
                AuthenticateAndOpenApplication();
            }
        }

        #endregion

        #region  Authentication of application

        public void AuthenticateAndOpenApplication()
        {
            // Authenticate application using IsolatedStorageSettings
            if (IsolatedStorageSettings.ApplicationSettings.Contains("IsAuthenticatedSetting"))
            {
                if (IsolatedStorageSettings.ApplicationSettings["IsAuthenticatedSetting"].ToString() == "true")
                {
                    // Open Application
                    //MessageBox.Show("Authentication Successfull..");
                }
                else
                {
                    Dispatcher.BeginInvoke(() =>
                   {
                       MessageBox.Show("Please Contact Admin. \nApplication Will be closed");
                       Application.Current.Terminate();
                   });
                }
            }
            else
            {
                RegisterApplication();
            }
        }

        #endregion
    }

}
