using System.Text;
using System.Reflection;
using Blogifier.Core.Extensions;
using Blogifier.Shared;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;

namespace Blogifier.Core.Providers
{
    public class BucketProvider : IStorageProvider
    {
		private const string bucketName = "qvanuncios-bucket";

		private string secretKey;

		private string publicKey;

		private readonly IConfiguration _configuration;
		private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast2;
        private static IAmazonS3 s3Client;
		
		private string _storageRoot;
		private readonly string _slash = Path.DirectorySeparatorChar.ToString();
		public BucketProvider(IConfiguration configuration)
		{
			_configuration = configuration;
			_storageRoot = $"{ContentRoot}{_slash}wwwroot{_slash}data{_slash}";
			var tuple = GetAWSCredentials();
			publicKey = tuple.Item1;
			secretKey = tuple.Item2;
            s3Client = new AmazonS3Client(publicKey, secretKey, bucketRegion);
		}

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

		private (string, string) GetAWSCredentials() {
			var filePath = "../../.env";
			var publicKeyS = "";
			var secretKeyS = "";


			if (!File.Exists(filePath)) {
				return (null, null);
			}

            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split('=',StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                    continue;

                if(parts[0] == "AWS_BUCKET_PUBLIC_KEY") publicKeyS = parts[1];
				else if (parts[0] == "AWS_BUCKET_SECRET_KEY") secretKeyS = parts[1]; 
            }

			return (publicKeyS, secretKeyS);
		}

        public async Task<IList<string>> GetThemes()
        {
            var themes = new List<string>();
			var themesDirectory = Path.Combine(ContentRoot, $"Views{_slash}Themes");
			try
			{
				foreach (string dir in Directory.GetDirectories(themesDirectory))
				{
					themes.Add(Path.GetFileName(dir));
				}
			}
			catch { }
			return await Task.FromResult(themes);
        }

        public async Task<ThemeSettings> GetThemeSettings(string theme)
        {
            var settings = new ThemeSettings();
			var fileName = Path.Combine(ContentRoot, $"wwwroot{_slash}themes{_slash}{theme.ToLower()}{_slash}settings.json");
			if (File.Exists(fileName))
			{
				try
				{
					string jsonString = File.ReadAllText(fileName);
					settings = JsonSerializer.Deserialize<ThemeSettings>(jsonString);
				}
				catch (Exception ex) 
				{
					Serilog.Log.Error($"Error reading theme settings: {ex.Message}");
					return null;
				}
			}

			return await Task.FromResult(settings);
        }

        public async Task<bool> SaveThemeSettings(string theme, ThemeSettings settings)
        {
            var fileName = Path.Combine(ContentRoot, $"wwwroot{_slash}themes{_slash}{theme.ToLower()}{_slash}settings.json");
			try
			{
				if (File.Exists(fileName))
					File.Delete(fileName);

				var options = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true	};

				string jsonString = JsonSerializer.Serialize(settings, options);

				using FileStream createStream = File.Create(fileName);
				await JsonSerializer.SerializeAsync(createStream, settings, options);
			}
			catch (Exception ex)
			{
				Serilog.Log.Error($"Error writing theme settings: {ex.Message}");
				return false;
			}
			return true;
        }

        public Task<string> UploadBase64Image(string baseImg, string root, string path = "")
        {
            throw new NotImplementedException();
        }

		public async Task<GetObjectResponse> DownloadFile(string keyName)
        {
            s3Client = new AmazonS3Client(publicKey, secretKey, bucketRegion);
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };
                return await s3Client.GetObjectAsync(request);
            }
            catch (AmazonS3Exception)
            {
                // If bucket or object does not exist
                return new GetObjectResponse();
            }
        }

        public async Task<bool> UploadFormFile(IFormFile file, string path = "")
        {
            s3Client = new AmazonS3Client(publicKey, secretKey, bucketRegion);

			var res = await UploadFileAsync(file.OpenReadStream(), path);
			
			if (res == "Ok")
				return true;
			else
				return false;
        }

        public Task<string> UploadFromWeb(Uri requestUri, string root, string path = "")
        {
            throw new NotImplementedException();
        }

		private async Task<string> UploadFileAsync(Stream file, string keyName)
        {
            try
            {
				var fileTransferUtility = new TransferUtility(s3Client);
				await fileTransferUtility.UploadAsync(file, bucketName, keyName);
				return "Ok";
            }
            catch (AmazonS3Exception e)
            {
                return e.Message;
            }
        }

		private string ContentRoot
		{
			get
			{
				string path = Directory.GetCurrentDirectory();
				string testsDirectory = $"tests{_slash}Blogifier.Tests";
				string appDirectory = $"src{_slash}Blogifier";

				// development unit test run
				if (path.LastIndexOf(testsDirectory) > 0)
				{
					path = path.Substring(0, path.LastIndexOf(testsDirectory));
					return $"{path}src{_slash}Blogifier";
				}

				// development debug run
				if (path.LastIndexOf(appDirectory) > 0)
				{
					path = path.Substring(0, path.LastIndexOf(appDirectory));
					return $"{path}src{_slash}Blogifier";
				}
				return path;
			}
		}

		string GetFileName(string fileName)
		{
			// some browsers pass uploaded file name as short file name 
			// and others include the path; remove path part if needed
			if (fileName.Contains(_slash))
			{
				fileName = fileName.Substring(fileName.LastIndexOf(_slash));
				fileName = fileName.Replace(_slash, "");
			}
			// when drag-and-drop or copy image to TinyMce editor
			// it uses "mceclip0" as file name; randomize it for multiple uploads
			if (fileName.StartsWith("mceclip0"))
			{
				Random rnd = new Random();
				fileName = fileName.Replace("mceclip0", rnd.Next(100000, 999999).ToString());
			}
			return fileName. SanitizePath();
		}

		void VerifyPath(string path)
		{
			path = path.SanitizePath();

			if (!string.IsNullOrEmpty(path))
			{
				var dir = Path.Combine(_storageRoot, path);

				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
			}
		}

		string TitleFromUri(Uri uri)
		{
			var title = uri.ToString().ToLower();
			title = title.Replace("%2f", "/");

			if (title.EndsWith(".axdx"))
			{
				title = title.Replace(".axdx", "");
			}
			if (title.Contains("image.axd?picture="))
			{
				title = title.Substring(title.IndexOf("image.axd?picture=") + 18);
			}
			if (title.Contains("file.axd?file="))
			{
				title = title.Substring(title.IndexOf("file.axd?file=") + 14);
			}
			if (title.Contains("encrypted-tbn") || title.Contains("base64,"))
			{
				Random rnd = new Random();
				title = string.Format("{0}.png", rnd.Next(1000, 9999));
			}

			if (title.Contains("/"))
			{
				title = title.Substring(title.LastIndexOf("/"));
			}

			title = title.Replace(" ", "-");

			return title.Replace("/", "").SanitizeFileName();
		}

		string PathToUrl(string path)
		{
			string url = path.ReplaceIgnoreCase(_storageRoot, "").Replace(_slash, "/");
			return $"data/{url}";
		}

		string GetImgSrcValue(string imgTag)
		{
			if (!(imgTag.Contains("data:image") && imgTag.Contains("src=")))
				return imgTag;

			int start = imgTag.IndexOf("src=");
			int srcStart = imgTag.IndexOf("\"", start) + 1;

			if (srcStart < 2)
				return imgTag;

			int srcEnd = imgTag.IndexOf("\"", srcStart);

			if (srcEnd < 1 || srcEnd <= srcStart)
				return imgTag;

			return imgTag.Substring(srcStart, srcEnd - srcStart);
		}
    }
}
