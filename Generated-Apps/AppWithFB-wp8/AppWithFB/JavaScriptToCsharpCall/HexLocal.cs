using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;
using WPCordovaClassLib.Cordova.JSON;

/* 
 * Developed by     :   Vignesh Raja M 
 * Created date     :   15-05-2013
 * Purpose          :   Java Script to C# call.
 * Description      :   C# method can be invoked from Java Script.
 *                      This might be helpful for developing a Plugin on Windows Phone.
 * Reference        :   http://cordova.apache.org/docs/en/2.7.0/guide_plugin-development_windows-phone_index.md.html
 * 
 */

namespace hex
{
    class local : BaseCommand
    {
        /// <summary>
        /// Can be invoked from Java script with only one string parameter.
        /// Returns the result to Java script.
        /// The syntax can not be modified.
        /// </summary>
        /// <param name="options"></param>
        public void CustomAdd(string options)
        {
            try
            {
                string strA = JsonHelper.Deserialize<string[]>(options)[0];
                string strB = JsonHelper.Deserialize<string[]>(options)[1];
                int result = Convert.ToInt32(strA) + Convert.ToInt32(strB);
                DispatchCommandResult(new PluginResult(PluginResult.Status.OK, Convert.ToString(result)));

                System.Diagnostics.Debug.WriteLine("Custom Add   :  "+ strA + " + " + strB + " = " + result);
            }
            catch (Exception ex)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Error occured. " + ex.Message.ToString()));
            }
        }
    }
 

}
