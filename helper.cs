using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace nnunet_client
{
    public static class helper
    {

        // Logger is optional - only used in UI projects
        public static object Logger = null;

        public static void log(string message)
        {
            Console.WriteLine(message);

            // Logger is optional - only used in UI projects
            // if (Logger != null)
            // {
            //     Logger.AppendLine(message);
            // }
        }

        public static void log_for_debug(string message)
        {
            Console.WriteLine(message);
        }


        public static async Task log_and_yield(string message)
        {
            log(message);
            await Task.Delay(1); // Let UI thread catch up
        }


        public static void print(string message)
        {
            Console.WriteLine(message);
        }

        public static void error(string message)
        {
            Console.WriteLine(message);
            
            // Logger is optional - only used in UI projects
            // if (Logger != null)
            // {
            //     Logger.AppendLine(message);
            // }
        }

        public static void show_error_msg_box(string message)
        {
             MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void show_warning_msg_box(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void show_info_msg_box(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        public static bool show_yes_no_msg_box(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "Yes/No", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return (result == MessageBoxResult.Yes);
        }


        public static string join(string path1, string path2)
        {
            return System.IO.Path.Combine(path1, path2);
        }


        public static Dictionary<string, object> json2dict(string jsonString)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
        }

        public static Dictionary<string, string> json2strstrdict(string jsonString)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
        }

        public static List<object> json2list(string jsonString)
        {
            return JsonConvert.DeserializeObject<List<object>>(jsonString);
        }


    }
}

