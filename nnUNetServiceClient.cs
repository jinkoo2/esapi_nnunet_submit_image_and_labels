using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nnunet { 
public class nnUNetServicClient
{
    private readonly string baseUrl;
    private readonly HttpClient client;

    public nnUNetServicClient(string baseUrl, string authToken)
    {
        this.baseUrl = baseUrl.TrimEnd('/');
        // Increased timeout for large file uploads (medical images can be large)
        this.client = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };

        // --- AUTHENTICATION HEADER IMPLEMENTATION ---
        // 1. Check if a token was provided
        if (!string.IsNullOrWhiteSpace(authToken))
        {
            try
            {
                // 2. Set the Authorization header as "Bearer <TOKEN>" for all subsequent requests
                this.client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", authToken);

                Console.WriteLine("Authorization header successfully set for HttpClient.");
            }
            catch (FormatException ex)
            {
                // Handle cases where the token string is malformed
                Console.Error.WriteLine($"Error setting Authorization header: {ex.Message}");
            }
        }
        // --- END AUTHENTICATION HEADER IMPLEMENTATION ---
        }

        public async Task<dynamic> GetPingAsync()
    {
        return await GetJsonAsync("/ping");
    }

    public async Task<dynamic> GetDatasetJsonListAsync()
    {
        return await GetJsonAsync("/datasets/list");
    }

    public async Task<dynamic> GetDatasetJsonIdListAsync()
    {
        return await GetJsonAsync("/datasets/id-list");
    }

    public async Task<dynamic> GetDatasetImageNameListAsync(string datasetId)
    {
        return await GetJsonAsync(string.Format("/datasets/image_name_list?dataset_id={0}", WebUtility.UrlEncode(datasetId)));
    }

    public async Task DownloadImageAndLabelAsync(string datasetId, string imagesFor, int num, string outDir)
    {
        var url = string.Format("/datasets/get_image_and_labels?dataset_id={0}&images_for={1}&num={2}", datasetId, imagesFor, num);
        var meta = await GetJsonAsync(url);

        Directory.CreateDirectory(outDir);

        string baseImageUrl = baseUrl + meta.base_image_url;
        string labelsUrl = baseUrl + meta.labels_url;

        await DownloadFileAsync(baseImageUrl, Path.Combine(outDir, (string)meta.base_image_filename));
        await DownloadFileAsync(labelsUrl, Path.Combine(outDir, (string)meta.labels_filename));
    }

    public async Task<dynamic> PostDatasetJsonAsync(object data)
    {
        return await PostJsonAsync("/datasets/new", data);
    }

    public async Task<dynamic> PostImageAndLabelsAsync(string datasetId, string imagesFor, string imagePath, string labelsPath)
    {
        string url = baseUrl + "/datasets/add_image_and_labels";
        using (var form = new MultipartFormDataContent())
        {
            form.Add(new StringContent(datasetId), "dataset_id");
            form.Add(new StringContent(imagesFor), "images_for");
            form.Add(new StreamContent(File.OpenRead(imagePath)), "base_image", Path.GetFileName(imagePath));
            form.Add(new StreamContent(File.OpenRead(labelsPath)), "labels", Path.GetFileName(labelsPath));
            
            var response = await client.PostAsync(url, form);
            return await ParseResponseAsync(response);
        }
    }

    public async Task<dynamic> UpdateImageAndLabelsAsync(string datasetId, string imagesFor, int num, string imagePath, string labelsPath)
    {
        var url = baseUrl + "/datasets/update_image_and_labels";
        using (var form = new MultipartFormDataContent())
        {
            form.Add(new StringContent(datasetId), "dataset_id");
            form.Add(new StringContent(imagesFor), "images_for");
            form.Add(new StringContent(num.ToString()), "num");
            form.Add(new StreamContent(File.OpenRead(imagePath)), "base_image", Path.GetFileName(imagePath));
            form.Add(new StreamContent(File.OpenRead(labelsPath)), "labels", Path.GetFileName(labelsPath));

            var response = await client.PutAsync(url, form);
            return await ParseResponseAsync(response);
        }
    }


    public async Task<dynamic> DeleteImageAndLabelsAsync(string datasetId, string imagesFor, int num)
    {
        string url = string.Format("{0}/datasets/delete_image_and_labels?dataset_id={1}&images_for={2}&num={3}", baseUrl, datasetId, imagesFor, num);
        var response = await client.DeleteAsync(url);
        return await ParseResponseAsync(response);
    }

    public async Task<dynamic> DeletePredictionRequestAsync(string datasetId, string requestId)
    {
        string url = string.Format("{0}/predictions/delete?dataset_id={1}&request_id={2}", baseUrl, datasetId, requestId);
        var response = await client.DeleteAsync(url);
        return await ParseResponseAsync(response);
    }
    public async Task<dynamic> PostImageForPredictionAsync(string datasetId, string imagePath, string requesterId, string imageId, Dictionary<string, string> metadata)
    {
        
        string url = baseUrl + "/predictions/predict";
        using (var form = new MultipartFormDataContent())
        {
            form.Add(new StringContent(datasetId), "dataset_id");
            form.Add(new StringContent(requesterId), "requester_id");
            form.Add(new StringContent(imageId), "image_id");
            foreach (var pair in metadata)
                form.Add(new StringContent(pair.Value), pair.Key);
            form.Add(new StreamContent(File.OpenRead(imagePath)), "image", Path.GetFileName(imagePath));

            var response = await client.PostAsync(url, form);
            return await ParseResponseAsync(response);
        }
    }


    public async Task<dynamic> GetPredictionListAsync(string datasetId)
    {
        string url = string.Format("/predictions?dataset_id={0}", datasetId);
        return await GetJsonAsync(url);
    }
    public async Task<dynamic> GetPredictionAsync(string datasetId, string reqId)
    {
        string url = string.Format("/predictions/prediction?dataset_id={0}&req_id={1}", Uri.EscapeDataString(datasetId), Uri.EscapeDataString(reqId));
        return await GetJsonAsync(url);
    }


    public async Task<dynamic> DownloadPredictionImagesAndLabelsAsync(string datasetId, string reqId, int imageNumber, string outDir)
    {
        string metaUrl = string.Format("/predictions/image_and_label_metadata?dataset_id={0}&req_id={1}&image_number={2}", datasetId, reqId, imageNumber);
        var metadata = await GetJsonAsync(metaUrl);

        Directory.CreateDirectory(outDir);
        string zipUrl = baseUrl + metadata.download_url;
        string zipPath = Path.Combine(outDir, string.Format("{0}_image_{1}.zip", reqId, imageNumber));
        await DownloadFileAsync(zipUrl, zipPath);

        return new
        {
            image_names = metadata.image_names,
            label_name = metadata.label_name,
            zip_path = zipPath
        };
    }

    private async Task<dynamic> GetJsonAsync(string path)
    {
        var response = await client.GetAsync(baseUrl + path);
        return await ParseResponseAsync(response);
    }

    private async Task<dynamic> PostJsonAsync(string path, object data)
    {
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync(baseUrl + path, content);
        return await ParseResponseAsync(response);
    }

    private async Task<dynamic> PostMultipartAsync(string path, string datasetId, string imagesFor, string imagePath, string labelsPath)
    {
        using (var form = new MultipartFormDataContent())
        {
            form.Add(new StringContent(datasetId), "dataset_id");
            form.Add(new StringContent(imagesFor), "images_for");
            // StreamContent takes ownership of the streams and will dispose them when the form is disposed
            form.Add(new StreamContent(File.OpenRead(imagePath)), "base_image", Path.GetFileName(imagePath));
            form.Add(new StreamContent(File.OpenRead(labelsPath)), "labels", Path.GetFileName(labelsPath));

            var response = await client.PostAsync(baseUrl + path, form);
            return await ParseResponseAsync(response);
        }
    }

    private async Task DownloadFileAsync(string url, string destPath)
    {
        using (var response = await client.GetAsync(url))
        using (var fs = new FileStream(destPath, FileMode.Create))
        {
            await response.Content.CopyToAsync(fs);
        }
    }

    private async Task<dynamic> ParseResponseAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
            return JsonConvert.DeserializeObject<dynamic>(content);
        throw new Exception(string.Format("HTTP {0}: {1}", (int)response.StatusCode, content));
    }

    public async Task<dynamic> GetContourPointsAsync(
    string datasetId,
    string reqId,
    int imageNumber,
    int contourNumber,
    string coordinateSystems = "woI")
        {
            string url = "/predictions/contour_points" +
                string.Format("?dataset_id={0}", Uri.EscapeDataString(datasetId)) +
                string.Format("&req_id={0}", Uri.EscapeDataString(reqId)) +
                string.Format("&image_number={0}", imageNumber) +
                string.Format("&contour_number={0}", contourNumber) +
                string.Format("&coordinate_systems={0}", Uri.EscapeDataString(coordinateSystems));

            return await GetJsonAsync(url);
        }
    }
}

