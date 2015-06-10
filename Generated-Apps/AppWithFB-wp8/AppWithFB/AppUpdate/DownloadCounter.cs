using Common;
using Microsoft.Phone.Net.NetworkInformation;
using System;
using System.Diagnostics;
using System.Net;

namespace com.hexaware.appwithfb.AppWithFB
{
   /* 
     * Developed by     :   Vignesh Raja M 
     * Created date     :   03-12-2014
     * Purpose          :   Download count incrementer
     * Description      :   Increments download count of XAP package (Major version) by invoking web service.
     */

    public class DownloadCounter
    {
        string strDownloadCounterURI = ServerInformation.ServerIPAddress + "WorkQuikr-console/DownloadCounter?appName={0}&version={1}&dummyid={2}";

        /// <summary>
        /// Increments download count for the specified version of the application
        /// </summary>
        /// <param name="strVersion"></param>
        public void UpdateDownloadCounter(string strVersion)
        {
            // Check Network Availablity
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    // Invoke web service to increment download count for the specified version of the application
                    WebClient wcDownloadCounter = new WebClient();
                    wcDownloadCounter.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wcDownloadCounter_DownloadStringCompleted);
                    wcDownloadCounter.DownloadStringAsync(new Uri(String.Format(strDownloadCounterURI, AppInformation.AppName, strVersion, Guid.NewGuid().ToString())));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error occured while updating download counter. \n Details : {0}" + ex.Message);
                }
            }
            else
            {
                Debug.WriteLine("\n Network unavailable. Update Download Counter failed.");
            }
        }

        /// <summary>
        /// Web service invocation completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wcDownloadCounter_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                Debug.WriteLine((e.Error == null) ? "Download Count incremented"+e.Result.ToString() : "Download error : " + e.Error.Message.ToString());   // e.Result contains server response
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error occured on wcDownloadCounter_DownloadStringCompleted() :" + ex.Message);
            }
        }
    }
}
