using Common;
using Microsoft.Phone.Net.NetworkInformation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Phone.Management.Deployment;
using Windows.Storage;

/* 
 * Developed by     :   Vignesh Raja M 
 * Created date     :   11-06-2013
 * Purpose          :   Auto Update
 * Description      :   Checks for new updates by geting Latest Version information from server.
 *                      If Major version change then Downloads Major updates else Downloads Minior updates.
 */

namespace com.hexaware.appwithfb.AppWithFB
{
    public class AutoUpdate
    {
        #region Variable Declaration
        static bool blnIsAutoUpdateEnabled = true;

        string strGetMajorVersionURI = ServerInformation.ServerIPAddress + "WorkQuikr-console/RegisterAndPull?act=3&appName=" + AppInformation.AppName + "&dummyid={0}";
        string strGetMinorVersionURI = ServerInformation.ServerIPAddress + "WorkQuikr-console/RegisterAndPull?act=4&appName=" + AppInformation.AppName + "&currentVersion={0}&dummyid={1}";
        string strMinorUpdatePackageURI = ServerInformation.ServerIPAddress + "WorkQuikr-Download/" + AppInformation.AppName + "/{0}/update.zip?dummyid={1}";
        string strMajorUpdatePackageURI = ServerInformation.ServerIPAddress + "WorkQuikr-Download/" + AppInformation.AppName + "/{0}/WP8/" + AppInformation.AppName + ".xap?dummyid={1}";
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        public bool isLaunching { get; set; }
        string strLatestMinorVersion;
        #endregion



        /// <summary>
        /// During App launch event Set "isLaunching= true" else set "isLaunching= false"
        /// </summary>
        /// <param name="isLaunching"></param>
        public AutoUpdate(bool isLaunching)
        {
            this.isLaunching = isLaunching;
        }


        #region Major Update
        /// <summary>
        /// Checks for new updates by geting Latest Version information from server
        /// Triggers "wcGetLatestVersion_DownloadStringCompleted" after download completed.
        /// </summary>
        public void CheckForMajorUpdates()
        {
            // Copy the latest UI files to local folder in order to Completed the Major version installation 
            if (settings.Contains("IsMajorCompleted") && Convert.ToString(settings["IsMajorCompleted"]) == "False")
            {
                string strInstallationWWWfolder = System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "www");
                string strLocalWWWfolder = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "www");
                DirectoryCopy(strInstallationWWWfolder, strLocalWWWfolder);
                AppSettings.Save("AppVersion", AppInformation.AppVersion);
                AppSettings.Save("IsMajorCompleted", "True");
            }

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                //Get Latest Version from server
                try
                {
                    Debug.WriteLine(String.Format(strGetMajorVersionURI, Guid.NewGuid().ToString()));
                    WebClient wcGetMajorVersion = new WebClient();
                    wcGetMajorVersion.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wcGetMajorVersion_DownloadStringCompleted);
                    wcGetMajorVersion.DownloadStringAsync(new Uri(String.Format(strGetMajorVersionURI, Guid.NewGuid().ToString())));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error occured on getting latest version information from server. \n Details : {0}" + ex.Message);
                }
            }
            else
            {
                Debug.WriteLine("\n Network unavailable");
            }

        }

        /// <summary>
        /// Compares current version with latest version
        /// If Major version change then Downloads Major updates else Downloads Minior updates
        /// </summary>
        void wcGetMajorVersion_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    string strServerResponse = e.Result.ToString();      // e.Result contains server response

                    #region Save the AppVersion in IsolatedStorageSettings during very first time

                    if (!settings.Contains("AppVersion"))
                    {
                        settings.Add("AppVersion", AppInformation.AppVersion);
                        settings.Save();
                    }
                    //settings.Save();

                    #endregion

                    // Get App Version from Isolated settings
                    string strAppVersion = Convert.ToString(settings["AppVersion"]); //1.0
                    string strLatestVersion = strServerResponse.Substring(strServerResponse.IndexOf("=") + 2); //2.0

                    // MessageBox.Show("Latest Version   : " + e.Result.ToString() + " \n Current Version : " + strAppVersion);

                    if (Convert.ToInt32(strAppVersion.Substring(0, 1)) < Convert.ToInt32(strLatestVersion.Substring(0, 1)))
                    {
                        Debug.WriteLine("Download And Install Major Update ");
                        MessageBoxResult m = MessageBox.Show("Latest Version   : " + strLatestVersion + " \nCurrent Version : " + strAppVersion + "\nDo you want to download ?", "New Version available", MessageBoxButton.OKCancel);

                        if (m == MessageBoxResult.OK)
                        {
                            // Download the XAP file => install 
                            DownloadAndInstallMajorUpdate(strLatestVersion);
                        }
                        else
                        {
                            if (blnIsAutoUpdateEnabled && isLaunching)
                            {
                                CheckForMinorUpdates(strAppVersion.Substring(0, 1));
                            }
                        }
                    }
                    else
                    {

                        if (blnIsAutoUpdateEnabled && isLaunching)
                        {
                            CheckForMinorUpdates(strAppVersion.Substring(0, 1));
                        }
                    }
                }

                else
                {
                    Debug.WriteLine("Download error : " + e.Error.Message.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error on wcGetLatestVersion_DownloadStringCompleted() :" + ex.Message);
            }

        }

        /// <summary>
        /// Major Update : Downloads latest version of .XAP file and initiates an Installation request.
        /// </summary>
        private async void DownloadAndInstallMajorUpdate(string strVersion)
        {
            try
            {
                Uri AppUri = new Uri(String.Format(strMajorUpdatePackageURI, strVersion, Guid.NewGuid().ToString()));

                Debug.WriteLine(AppUri.ToString());
                await Windows.System.Launcher.LaunchUriAsync(AppUri);

		  // Increments download count of XAP package 
                DownloadCounter objDownloadCounter = new DownloadCounter();
                objDownloadCounter.UpdateDownloadCounter(strVersion);

                AppSettings.Save("IsMajorCompleted", "False");

            }
            catch (Exception ex)
            {
                Debug.WriteLine(" Installation failed! \n Exception Details : {0} \n {1}", ex.Message, ex.ToString());
            }
        }

        #region DirectoryCopy
        public static void DirectoryCopy(string sourceDirName, string destDirName)
        {

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location. 

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath);
            }

        }
        #endregion

        #endregion

        #region Minor Update

        public void CheckForMinorUpdates(string strCurrentMajorVersion)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                //Get Latest Minor Version from server
                try
                {
                    Debug.WriteLine("CheckForMinorUpdates() ::: Getting latest minor version..... " + String.Format(strGetMinorVersionURI, strCurrentMajorVersion, Guid.NewGuid().ToString()));
                    WebClient wcGetMinorVersion = new WebClient();
                    wcGetMinorVersion.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wcGetMinorVersion_DownloadStringCompleted);
                    wcGetMinorVersion.DownloadStringAsync(new Uri(String.Format(strGetMinorVersionURI, strCurrentMajorVersion, Guid.NewGuid().ToString())));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error occured :  Could't get Latest minor version information from server. \n Details : {0}" + ex.Message);
                }
            }
            else
            {
                Debug.WriteLine("\n Network unavailable");
            }

        }

        void wcGetMinorVersion_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    // Get App Version from Isolated settings
                    string strAppVersion = Convert.ToString(settings["AppVersion"]);
                    string strServerResponse = e.Result.ToString();      // e.Result contains server response
                    strLatestMinorVersion = strServerResponse.Substring(strServerResponse.IndexOf("=") + 2);
                    Debug.WriteLine("Latest Minor Version   : " + strLatestMinorVersion + " \n Current Minor Version : " + strAppVersion);
                    //Compare minor part
                    if (Convert.ToInt32(strAppVersion.Substring(2)) < Convert.ToInt32(strLatestMinorVersion.Substring(2)))
                    {
                        // Download the ZIP file => Extract to www folder in IsolatedStorage LocalFolder (Over-write) 
                        DownloadMinorUpdates();
                    }
                    else
                    {
                        Debug.WriteLine("No change in minor version. !!!  ");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Minor Update : Downloads latest update file(.ZIP) from the server.
        /// Triggers "wcDownloadMinorUpdates_OpenReadCompleted" after download completed.
        /// </summary>
        /// 
        public void DownloadMinorUpdates()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Debug.WriteLine("Downloading Minor Updates .....  \n" + String.Format(strMinorUpdatePackageURI, strLatestMinorVersion, Guid.NewGuid().ToString()));
                WebClient wcDownloadMinorUpdates = new WebClient();
                wcDownloadMinorUpdates.OpenReadCompleted += new OpenReadCompletedEventHandler(wcDownloadMinorUpdates_OpenReadCompleted);
                wcDownloadMinorUpdates.OpenReadAsync(new Uri(String.Format(strMinorUpdatePackageURI, strLatestMinorVersion, Guid.NewGuid().ToString())));

            }
            else
            {
                System.Windows.MessageBox.Show("Network unavailable !");
            }
        }

        /// <summary>
        /// Initiates the Unzip process after successful download
        /// </summary>
        void wcDownloadMinorUpdates_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                System.Diagnostics.Debug.WriteLine("Could not download minor update from Server !!! \n Could not complete minor update process !" + e.Error);
                return;
            }

            // Unzip the file contents to local folder (folder name : www - This folder is default for Cordova view)
            Stream stream = e.Result;
            UnzipFiles(stream, "www");

        }

        /// <summary>
        /// Unzip And Save Files
        /// </summary>
        /// <param name="stream">Input zip file stream</param>
        /// <param name="strFolderToExtract">Target folder to extract the zip file</param>
        public async void UnzipFiles(Stream stream, string strFolderToExtract)
        {
            string strAppRoot = "/app/";

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Get zipStream using SharpGIS.UnZipper
                using (var zipStream = new SharpGIS.UnZipper(stream))
                {
                    string strDestFolder = string.Empty;
                    StorageFolder LocalFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                    // Create Target Folder To Extract the zip file
                    await LocalFolder.CreateFolderAsync(strFolderToExtract, CreationCollisionOption.OpenIfExists);

                    #region Extract each file in the zip file

                    foreach (string file in zipStream.FileNamesInZip)
                    {
                        string fileName = Path.GetFileName(file);
                        try
                        {
                            #region Create folder if needed
                            if (file.Contains(@"/"))
                            {
                                string strFolderName = (file.Substring(0, file.IndexOf(@"/")));
                                // Create sub folder
                                await LocalFolder.CreateFolderAsync(Path.Combine(strFolderToExtract, strFolderName), CreationCollisionOption.OpenIfExists);
                                strDestFolder = Path.Combine(LocalFolder.Path, strFolderToExtract, strFolderName);
                            }
                            else
                            {
                                strDestFolder = Path.Combine(LocalFolder.Path, strFolderToExtract);
                            }
                            #endregion

                            if (!string.IsNullOrEmpty(fileName))
                            {

                                #region Save file entry to isolated storage
                                using (var streamWriter = new BinaryWriter(new IsolatedStorageFileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write, isoStore)))
                                {
                                    Stream fileStream = zipStream.GetFileStream(file);

                                    var buffer = new byte[2048];
                                    int size;
                                    while ((size = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        streamWriter.Write(buffer, 0, size);
                                    }
                                }
                                #endregion

                                // Debug.WriteLine(fileName + "\n -- File Extracted");
                                #region Move the file to Specified folder

                                try
                                {
                                    //  Copy & Delete files because move file does not have the option to over write
                                    isoStore.CopyFile(fileName, Path.Combine(strAppRoot, strDestFolder, fileName), true);
                                    isoStore.DeleteFile(fileName);
                                    //   Debug.WriteLine(" -- Moved to  :   " + Path.Combine(strAppRoot, strDestFolder, fileName));

                                }
                                catch (Exception exx)
                                {
                                    Debug.WriteLine("\n== Error occured when moving file :  " + fileName + "  - to -  " + strDestFolder + "\n Details : " + exx.Message.ToString());
                                }
                                #endregion

                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("\n== Error occured when processing the file : " + fileName + "\n == Details : " + ex.Message.ToString());
                        }
                    }

                    #endregion

                    AppSettings.Save("AppVersion", strLatestMinorVersion);
                    // Debug.WriteLine("Minor update " + strLatestMinorVersion + "  is completed successfully. \nPlease restart the application in order to apply the changes.");

                    // Reload MainPage.xaml to apply the minor update
                    App.RootFrame.Navigate(new Uri("//MainPage.xaml", UriKind.Relative));

                    // RemoveBackEntry to avoid displaying old UI if back button is pressed.
                    App.RootFrame.RemoveBackEntry();

                }
            }

        }

        #endregion
    }

}
