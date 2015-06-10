/*
 Licensed to the Apache Software Foundation (ASF) under one
 or more contributor license agreements.  See the NOTICE file
 distributed with this work for additional information
 regarding copyright ownership.  The ASF licenses this file
 to you under the Apache License, Version 2.0 (the
 "License"); you may not use this file except in compliance
 with the License.  You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing,
 software distributed under the License is distributed on an
 "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 KIND, either express or implied.  See the License for the
 specific language governing permissions and limitations
 under the License. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Resources;


namespace com.hexaware.appwithfb.AppWithFB
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.CordovaView.Loaded += CordovaView_Loaded;
            // Register for PushNotifications
            string channelName = "WP8SampleChannelsNew12";
            PushNotifications objPushNotifications = new PushNotifications();
            objPushNotifications.RegisterNotifications(channelName);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            string isMtoM, msg, msgtitle;

            // Redirect to MToMdemo.html for MobileToMobileCommunication
            if (NavigationContext.QueryString.TryGetValue("MtoM", out isMtoM))
            {
                if (isMtoM == "true")
                {
                    NavigationContext.QueryString.TryGetValue("msg", out msg);
                    NavigationContext.QueryString.TryGetValue("title", out msgtitle);
 
                    if (msg != null && msgtitle != null)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            if (MessageBox.Show(msg,msgtitle, MessageBoxButton.OK) == MessageBoxResult.OK)
                            {
                                CordovaView.StartPageUri = (new Uri("www/home.html", UriKind.Relative));
                            }
                        });
                    }
                   // CordovaView.StartPageUri = (new Uri("www/home.html", UriKind.Relative));
                }
            }
        }
        private void CordovaView_Loaded(object sender, RoutedEventArgs e)
        {
            this.CordovaView.Loaded -= CordovaView_Loaded;
            // first time load will have an animation
            Storyboard _storyBoard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = 0,
                Duration = TimeSpan.FromSeconds(0.6),
                To = 90
            };
            Storyboard.SetTarget(animation, SplashProjector);
            Storyboard.SetTargetProperty(animation, new PropertyPath("RotationY"));
            _storyBoard.Children.Add(animation);
            _storyBoard.Begin();
            _storyBoard.Completed += Splash_Completed;
        }
        void Splash_Completed(object sender, EventArgs e)
        {
            (sender as Storyboard).Completed -= Splash_Completed;
            LayoutRoot.Children.Remove(SplashImage);
        }
    }
}
