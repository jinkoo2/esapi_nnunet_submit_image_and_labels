using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace nnunet_client.models
{
    public class SubmitJob
    {
        public string JobId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } // "Pending", "Processing", "Completed", "Failed"
        public string ErrorMessage { get; set; }

        // Dataset information
        public string DatasetId { get; set; }
        public string ImagesFor { get; set; } // "Train" or "Test"

        // Structure set information
        public string PatientId { get; set; }
        public string StructureSetId { get; set; }
        public string StructureSetUID { get; set; }
        public string ImageId { get; set; }
        public string ImageUID { get; set; }
        public string ImageFOR { get; set; }

        // Label mappings
        public List<LabelMapping> LabelMappings { get; set; }

        public class LabelMapping
        {
            public string LabelName { get; set; }
            public int LabelValue { get; set; }
            public string StructureId { get; set; }
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static SubmitJob FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SubmitJob>(json);
        }
    }
}

