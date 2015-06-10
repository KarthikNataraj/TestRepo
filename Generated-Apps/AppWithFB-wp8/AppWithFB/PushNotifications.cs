using Common;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Notification;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Threading;

namespace com.hexaware.appwithfb.AppWithFB
{
    class PushNotifications : PhoneApplicationPage
    {

        // Reference :  http:// msdn.microsoft.com/en-us/library/windowsphone/develop/hh202945%28v=vs.105%29.aspx

        #region Register For Notifications


        public void RegisterNotifications(string channelName)
        {
            /// Holds the push channel that is created or found.
            HttpNotificationChannel pushChannel;

            // Try to find the push channel.
            pushChannel = HttpNotificationChannel.Find(channelName);
            try
            {
                if (Microsoft.Phone.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {

                    if (pushChannel == null)
                    {
                        #region If the channel was not found, then create a new connection to the push service.

                        pushChannel = new HttpNotificationChannel(channelName);

                        // Register for all the events before attempting to open the channel.
                        pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                        pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);

                        // Register Toast Notification : use only if you need to receive the notifications while your application is running.
                        pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);
                        // Register Raw notification
                        pushChannel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(PushChannel_HttpNotificationReceived);

                        pushChannel.Open();
                        // Bind this new channel for Tile events.
                        pushChannel.BindToShellTile();

                        // Bind this new channel for toast events.
                        pushChannel.BindToShellToast();
                        #endregion

                    }
                    else
                    {
                        #region The channel was already open, so just register for all the events.

                        pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                        pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);

                        // Register Toast Notification : use only if you need to receive the notifications while your application is running.
                        pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);
                        // Register Raw notification
                        pushChannel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(PushChannel_HttpNotificationReceived);


                        // Display the URI for testing purposes. Normally, the URI would be passed back to your web service at this point.
                        UpdateChannelURI(pushChannel.ChannelUri);


                        //MessageBox.Show(String.Format("Channel Uri is {0}", pushChannel.ChannelUri.ToString()));

                        #endregion
                    }
                }
                else
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Please check the internet connectivity. Could not connect to the Notification server "));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("^^^^^^ " + ex.ToString());
            }
        }

        #endregion

        #region Push Channel Events ( ChannelUriUpdated , ErrorOccurred )

        /// <summary>
        /// Event handler for when the push channel Uri is updated.
        /// </summary>
        void PushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            UpdateChannelURI(e.ChannelUri);
        }

        private void UpdateChannelURI(Uri NewChannelUri)
        {
            // Normally, the URI would be passed back to your web service at this point.

            string strChannelUri = Convert.ToString(NewChannelUri);
            if (strChannelUri != string.Empty)
            {
                PushNotificationData.PushChannelUri = NewChannelUri;

                //Dispatcher.BeginInvoke(() => MessageBox.Show(strChannelUri));
                Debug.WriteLine("Channel Uri :  " + strChannelUri);
                Debug.WriteLine("Device Unique ID :  " + PhoneInformation.DeviceUniqueId);

                //++++++++++++++++++++++++++++++++++++++++++++++++++++++
                String serlvetURL = ServerInformation.ServerIPAddress + "/WorkQuikr-console/RegisterAndPull?act=2" +
                                    "&deviceToken=" + NewChannelUri + "&deviceId=" + PhoneInformation.DeviceUniqueId + "&appname=" + AppInformation.AppName +
                                    "&version=" + AppInformation.AppVersion + "&userid=" + PhoneInformation.ANID2;
                Debug.WriteLine("Register For Notification Process : \n" + serlvetURL);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(serlvetURL));
                request.BeginGetResponse(new AsyncCallback(callbacker), request);

                //++++++++++++++++++++++++++++++++++++++++++++++++++++++
            }

        }


        void callbacker(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                try
                {
                    WebResponse response = request.EndGetResponse(result);
                    Debug.WriteLine(response.ToString());
                }
                catch (Exception ett)
                {
                    Debug.WriteLine(ett.ToString());
                }
            }
        }


        /// <summary>
        /// Event handler for when a push notification error occurs.
        /// </summary>
        void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            // Error handling logic for your particular application would be here.
            Dispatcher.BeginInvoke(() =>
                          MessageBox.Show(String.Format("A push notification {0} error occurred.  {1} ({2}) {3}",
                              e.ErrorType, e.Message, e.ErrorCode, e.ErrorAdditionalData))
                              );
        }

        #endregion

        #region Toast notification Received

        /// <summary>
        /// Event handler for when a toast notification arrives while your application is running.  
        /// The toast will not display if your application is running so you must add this
        /// event handler if you want to do something with the toast notification.
        /// </summary>
        void PushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
	 try
            {
            string strRecievedMessageTitle = e.Collection["wp:Text1"].ToString();
            string strRecievedMessage = e.Collection["wp:Text2"].ToString();

            MobileToMobileCommunication.SaveMessage(strRecievedMessageTitle, strRecievedMessage);
           
                Dispatcher.BeginInvoke(() =>
                          {
                              if (MessageBox.Show(strRecievedMessage, strRecievedMessageTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                              {
                                  App.RootFrame.Navigate(new Uri("/MainPage.xaml?MtoM=true", System.UriKind.Relative));
                              }
                          });
            }
            catch (Exception ex)
            {

                Debug.WriteLine("---------" + ex.Message.ToString());
            }


            //StringBuilder message = new StringBuilder();
            //string relativeUri = string.Empty;

            //message.AppendFormat("Received Toast {0}:\n", DateTime.Now.ToShortTimeString());

            //// Parse out the information that was part of the message.
            //foreach (string key in e.Collection.Keys)
            //{
            //    message.AppendFormat("{0}: {1}\n", key, e.Collection[key]);

            //    if (string.Compare(
            //        key,
            //        "wp:Param",
            //        System.Globalization.CultureInfo.InvariantCulture,
            //        System.Globalization.CompareOptions.IgnoreCase) == 0)
            //    {
            //        relativeUri = e.Collection[key];
            //    }
            //}

            //// Display a dialog of all the fields in the toast.
            //Dispatcher.BeginInvoke(() => MessageBox.Show(message.ToString()));

        }

        #endregion

        #region Raw notification Received

        /// <summary>
        /// Event handler for when a raw notification arrives.  For this sample, the raw 
        /// data is simply displayed in a MessageBox.
        /// </summary>
        void PushChannel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            string message;

            using (System.IO.StreamReader reader = new System.IO.StreamReader(e.Notification.Body))
            {
                message = reader.ReadToEnd();
            }

            Dispatcher.BeginInvoke(() => MessageBox.Show(String.Format("Received Raw Notification {0}:\n{1}",
                                            DateTime.Now.ToShortTimeString(), message)));

            System.Diagnostics.Debug.WriteLine(String.Format("Received Notification {0}:\n{1}", DateTime.Now.ToShortTimeString(), message));

        }

        #endregion

    }
}
