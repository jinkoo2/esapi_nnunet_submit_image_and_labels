using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VMSApplication = VMS.TPS.Common.Model.API.Application;
using VMSCourse = VMS.TPS.Common.Model.API.Course;
using VMSHospital = VMS.TPS.Common.Model.API.Hospital;
using VMSImage = VMS.TPS.Common.Model.API.Image;
using VMSPatient = VMS.TPS.Common.Model.API.Patient;
using VMSReferencePoint = VMS.TPS.Common.Model.API.ReferencePoint;
using VMSRegistration = VMS.TPS.Common.Model.API.Registration;
using VMSSeries = VMS.TPS.Common.Model.API.Series;
using VMSStructure = VMS.TPS.Common.Model.API.Structure;
using VMSStructureSet = VMS.TPS.Common.Model.API.StructureSet;
using VMSStudy = VMS.TPS.Common.Model.API.Study;

using System.IO;
using System.Runtime.CompilerServices;

namespace nnunet_client
{



    internal static class global
    {
        /// <summary>
        /// Gets the full path to the directory where the executing assembly (EXE or DLL) is loaded from.
        /// This is the most reliable way to find the application's root folder in a typical deployment.
        /// </summary>
        /// <returns>The directory path as a string.</returns>
        public static string get_execution_folder()
        {
            // 1. Get the path of the assembly that contains the currently executing code.
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // 2. Use Path.GetDirectoryName to extract only the folder path.
            // For example, if assemblyPath is "C:\App\MyApp.exe", this returns "C:\App".
            string directoryPath = Path.GetDirectoryName(assemblyPath);

            // This should not happen in a typical scenario, but good practice to check.
            if (string.IsNullOrEmpty(directoryPath))
            {
                // Fallback: If Assembly.Location is unavailable or empty (e.g., in memory-loaded assembly), 
                // use the current process's base directory.
                return AppContext.BaseDirectory;
            }

            return directoryPath;
        }

        public static void load_config()
        {
            // NOTE: You should adjust this path to where your config.json is located
            string currentDirectory = get_execution_folder();
            string configPath = Path.Combine(currentDirectory, "config.json");


            try
            {
                Console.WriteLine($"Attempting to load config from: {configPath}");
                AppConfig settings = AppConfig.LoadConfig(configPath);

                Console.WriteLine("\n--- Configuration Loaded Successfully ---");
                Console.WriteLine($"Data Root Secure: {settings.data_root_secure}");
                Console.WriteLine($"Export Log Enabled: {settings.make_export_log}");
                Console.WriteLine($"Requester ID: {settings.nnunet_requester_id}");
                Console.WriteLine($"Server URL: {settings.nnunet_server_url}");
                Console.WriteLine($"Server URL: {settings.nnunet_server_auth_email}");
                Console.WriteLine($"Server URL: {settings.nnunet_server_auth_token}");

                global.appConfig = settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nFailed to load configuration. Check file path and format.");
                // In a real application, you'd handle this more gracefully, 
                // potentially using default values.
            }

        }

        public static VMSPatient vmsPatient = null;
        public static VMSApplication vmsApplication = null;

        public static AppConfig appConfig = null;

    }
}

