﻿#pragma checksum "E:\WorkQuikrEnv4.7\Workspace\AppWithFB\Generated-Apps\AppWithFB-wp8\AppWithFB\Plugins\org.apache.cordova.media-capture\VideoRecorder.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "276E614938DA42D07339D3BA932DA7B6"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18051
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace WPCordovaClassLib.Cordova.UI {
    
    
    public partial class VideoRecorder : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Canvas LayoutRoot;
        
        internal System.Windows.Shapes.Rectangle viewfinderRectangle;
        
        internal Microsoft.Phone.Shell.ApplicationBar PhoneAppBar;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton btnStartRecording;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton btnTakeVideo;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/com.hexaware.appwithfb.AppWithFB;component/Plugins/org.apache.cordova.media-capt" +
                        "ure/VideoRecorder.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Canvas)(this.FindName("LayoutRoot")));
            this.viewfinderRectangle = ((System.Windows.Shapes.Rectangle)(this.FindName("viewfinderRectangle")));
            this.PhoneAppBar = ((Microsoft.Phone.Shell.ApplicationBar)(this.FindName("PhoneAppBar")));
            this.btnStartRecording = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("btnStartRecording")));
            this.btnTakeVideo = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("btnTakeVideo")));
        }
    }
}

