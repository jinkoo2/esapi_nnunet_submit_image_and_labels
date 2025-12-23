using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using nnunet_client.models;
using Newtonsoft.Json;

namespace nnunet_client.services
{
    public class JobQueueService
    {
        private readonly string _queueDirectory;

        public JobQueueService(string queueDirectory = null)
        {
            if (string.IsNullOrEmpty(queueDirectory))
            {
                // Default to a queue directory in the app data directory
                string dataDir = global.appConfig?.app_data_dir ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "nnunet_client");
                _queueDirectory = Path.Combine(dataDir, "submit_queue");
            }
            else
            {
                _queueDirectory = queueDirectory;
            }

            // Ensure queue directory exists
            if (!Directory.Exists(_queueDirectory))
            {
                Directory.CreateDirectory(_queueDirectory);
            }
        }

        public string EnqueueJob(SubmitJob job)
        {
            // Generate unique job ID if not provided
            if (string.IsNullOrEmpty(job.JobId))
            {
                job.JobId = Guid.NewGuid().ToString();
            }

            job.CreatedAt = DateTime.Now;
            job.Status = "Pending";

            // Save job to queue directory
            string jobFileName = $"job_{job.JobId}.json";
            string jobFilePath = Path.Combine(_queueDirectory, jobFileName);

            File.WriteAllText(jobFilePath, job.ToJson());

            Console.WriteLine($"Job enqueued: {job.JobId} (file: {jobFilePath})");

            return job.JobId;
        }

        public List<SubmitJob> GetPendingJobs()
        {
            var jobs = new List<SubmitJob>();

            if (!Directory.Exists(_queueDirectory))
                return jobs;

            foreach (var filePath in Directory.GetFiles(_queueDirectory, "job_*.json"))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var job = SubmitJob.FromJson(json);
                    if (job.Status == "Pending")
                    {
                        jobs.Add(job);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading job file {filePath}: {ex.Message}");
                }
            }

            return jobs.OrderBy(j => j.CreatedAt).ToList();
        }

        public SubmitJob GetJob(string jobId)
        {
            string jobFileName = $"job_{jobId}.json";
            string jobFilePath = Path.Combine(_queueDirectory, jobFileName);

            if (!File.Exists(jobFilePath))
                return null;

            try
            {
                string json = File.ReadAllText(jobFilePath);
                return SubmitJob.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading job file {jobFilePath}: {ex.Message}");
                return null;
            }
        }

        public void UpdateJobStatus(string jobId, string status, string errorMessage = null)
        {
            var job = GetJob(jobId);
            if (job == null)
                return;

            job.Status = status;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                job.ErrorMessage = errorMessage;
            }

            string jobFileName = $"job_{job.JobId}.json";
            string jobFilePath = Path.Combine(_queueDirectory, jobFileName);
            File.WriteAllText(jobFilePath, job.ToJson());
        }

        public void DeleteJob(string jobId)
        {
            string jobFileName = $"job_{jobId}.json";
            string jobFilePath = Path.Combine(_queueDirectory, jobFileName);

            if (File.Exists(jobFilePath))
            {
                File.Delete(jobFilePath);
                Console.WriteLine($"Job deleted: {jobId}");
            }
        }

        public string QueueDirectory => _queueDirectory;
    }
}

