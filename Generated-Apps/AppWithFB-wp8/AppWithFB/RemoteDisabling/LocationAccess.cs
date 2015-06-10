using Common;
using Microsoft.Phone.Net.NetworkInformation;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using Windows.Devices.Geolocation;

namespace com.hexaware.appwithfb.AppWithFB
{
    /* 
     * Developed by     :   Vignesh Raja M 
     * Created date     :   12-12-2014
     * Purpose          :   Location access verification
     * Description      :   Verifies location access permission for the current location by invoking web service if network is available.
                            Displays alert message and close the application if access is denied.
     * Reference URL     :  http://developer.nokia.com/community/wiki/Get_Phone_Location_with_Windows_Phone_8 
     */

    public class LocationAccess
    {
        string strLocationAccessURI = ServerInformation.ServerIPAddress + "WorkQuikr-console/LocationAccessPermission?appName=" + AppInformation.AppName + "&latitude={0}&longitude={1}&dummyid={2}";
        Geolocator geolocator;

        /// <summary>
        /// Verifies location access permission for the current location by invoking web service if network is available.
        /// Display alert message and close the application if access is denied for the current location
        /// </summary>
        public async void VerifyLocationAccessAsync()
        {
            // Check Network Availablity
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    // Retrieve the current location of the phone
                    geolocator = new Geolocator();
                    geolocator.DesiredAccuracy = PositionAccuracy.High;
                    geolocator.MovementThreshold = 100;

                    // Raised when the location is updated.
                    geolocator.PositionChanged += geolocator_PositionChanged;

                    // Initializes an asynchronous operation to retrieve the location of the phone.
                    await geolocator.GetGeopositionAsync(maximumAge: TimeSpan.FromMinutes(5), timeout: TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    if ((uint)ex.HResult == 0x80004004)
                    {
                        Debug.WriteLine("location is disabled in phone settings.");
                    }
                    else
                    {
                        Debug.WriteLine("An error occured while getting location information for verifying Location Access. \n Details : " + ex.Message);
                    }
                }
            }
            else
            {
                Debug.WriteLine("\n Network unavailable");
            }
        }

        /// <summary>
        /// Raised when the location is updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            // Current location of the phone
            Geoposition Currentlocation = args.Position;

            // Stop tracking position by removing the event handler and to set the object Geolocator to null
            geolocator.PositionChanged -= geolocator_PositionChanged;
            geolocator = null;

            // Verify Location Access By Geoposition
            VerifyLocationAccessByGeoposition(Currentlocation);
        }

        /// <summary>
        /// Invokes web service to Verify the Location Access for the current location
        /// </summary>
        /// <param name="geoposition"></param>
        private void VerifyLocationAccessByGeoposition(Geoposition geoposition)
        {
            string strCompleteLocationAccessURI = String.Format(strLocationAccessURI, geoposition.Coordinate.Latitude.ToString("0.00000"), geoposition.Coordinate.Longitude.ToString("0.00000"), Guid.NewGuid().ToString());

            // Invoke web service to Verify the Location Access for the current location
            WebClient wcVerifyLocationAccess = new WebClient();
            wcVerifyLocationAccess.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wcVerifyLocationAccess_DownloadStringCompleted);
            wcVerifyLocationAccess.DownloadStringAsync(new Uri(strCompleteLocationAccessURI));
        }

        /// <summary>
        /// Displays alert message and terminate the application if access is denied for the current location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wcVerifyLocationAccess_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                Debug.WriteLine((e.Error == null) ? "VerifyLocationAccess Response : " + e.Result.ToString() : "Download error : " + e.Error.Message.ToString());   // e.Result contains server response

                if ((e.Error != null) || e.Result.ToString().ToLower() == "false")
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        // Display alert message 
                        MessageBox.Show("This application is not allowed in your current location. Please Contact Admin. \nApplication Will be closed");

                        // Terminate the application 
                        Application.Current.Terminate();
                    });
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occured while verifying Location Access Permission information from server. \n Details : {0}" + ex.Message);
            }
        }

    }
}
