using System;
using System.IO;
using Newtonsoft.Json;

namespace nnunet_client
{
    /// <summary>
    /// Represents the application configuration structure.
    /// The property names are capitalized according to C# conventions 
    /// but will be automatically mapped to the lowercase JSON keys.
    /// </summary>
    public class AppConfig
    {
        public string data_root_secure { get; set; }
        public bool make_export_log { get; set; }

        // nnU-Net related settings
        public string nnunet_requester_id { get; set; }
        public string nnunet_request_user_name { get; set; }
        public string nnunet_request_user_email { get; set; }
        public string nnunet_request_user_institution { get; set; }

        public string app_data_dir { get; set; }
        public string nnunet_server_url { get; set; }
        public string nnunet_server_auth_email { get; set; }
        public string nnunet_server_auth_token { get; set; }


        /// <summary>
        /// Loads and deserializes the configuration from the specified JSON file path.
        /// </summary>
        /// <param name="configFilePath">The full path to the config.json file.</param>
        /// <returns>A fully populated AppConfig object.</returns>
        public static AppConfig LoadConfig(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"Configuration file not found at: {configFilePath}");
            }

            try
            {
                // 1. Read the entire JSON file content into a string.
                string jsonContent = File.ReadAllText(configFilePath);

                // 2. Use Newtonsoft.Json (JsonConvert) to deserialize the JSON string 
                // into an instance of the AppConfig class.
                AppConfig config = JsonConvert.DeserializeObject<AppConfig>(jsonContent);

                return config;
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing errors (e.g., malformed JSON syntax)
                Console.WriteLine($"Error deserializing configuration file: {ex.Message}");
                throw;
            }
            catch (IOException ex)
            {
                // Handle file reading errors (e.g., access denied)
                Console.WriteLine($"Error reading configuration file: {ex.Message}");
                throw;
            }
        }

    }
}

