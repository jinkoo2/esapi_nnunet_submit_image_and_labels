using nnunet_client.services;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents.DocumentStructures;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
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

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0.0.1")]
[assembly: ESAPIScript(IsWriteable = false)]

namespace nnunet_client
{
    /// <summary>
    /// Main entry point for the Submit Job Worker program.
    /// This should be compiled as a separate executable that runs continuously,
    /// processing jobs from the queue.
    /// </summary>
    public class SubmitJobWorkerMain
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Add global exception handler for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Console.Error.WriteLine("--- UNHANDLED EXCEPTION ---");
                Console.Error.WriteLine(e.ExceptionObject.ToString());
                if (e.IsTerminating)
                {
                    Console.Error.WriteLine("Application will terminate.");
                }
            };

            try
            {
                using (VMSApplication app = VMSApplication.CreateApplication())
                {
                    // Execute runs synchronously on main thread
                    Execute(app);
                }
            }
            catch (Exception e)
            {
                // This catches exceptions thrown during VMSApplication setup or Execute() call.
                Console.Error.WriteLine("--- ESAPI Main Exception ---");
                Console.Error.WriteLine(e.ToString());
                Environment.Exit(1);
            }
        }

        [STAThread]
        static void Execute(VMSApplication app)
        {
            global.vmsApplication = app;

            try
            {
                // Load configuration
                global.load_config();

                // Create and start worker
                var worker = new SubmitJobWorker();
                
                Console.WriteLine("Submit Job Worker started. Press Ctrl+C to stop.");
                Console.WriteLine($"Queue directory: {worker.QueueDirectory}");
                Console.WriteLine();

                // Handle Ctrl+C gracefully
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    Console.WriteLine("\nStopping worker...");
                    worker.Stop();
                };

                // Start processing jobs - runs synchronously on main thread
                try
                {
                    worker.Start();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("--- Worker Start Exception ---");
                    Console.Error.WriteLine(ex.ToString());
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("--- Worker Main Exception ---");
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }
    }
}
