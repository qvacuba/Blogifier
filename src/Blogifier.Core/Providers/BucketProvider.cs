using System;
using StorageProvider;

namespace Blogifier.Core.Providers
{
    public class BucketProvider : IStorageProvider
    {
		
		public BucketProvider()
		{
			throw new NotImplementedException();
		}

		public bool FileExists(string path)
		{
			throw new NotImplementedException();
		}

		public async Task<IList<string>> GetThemes()
		{
			throw new NotImplementedException();
		}

		public async Task<ThemeSettings> GetThemeSettings(string theme)
		{
			throw new NotImplementedException();
		}

		public async Task<bool> SaveThemeSettings(string theme, ThemeSettings settings)
		{
			throw new NotImplementedException();
		}

		public async Task<bool> UploadFormFile(IFormFile file, string path = "")
		{
			throw new NotImplementedException();
		}

		public async Task<string> UploadFromWeb(Uri requestUri, string root, string path = "")
		{
			throw new NotImplementedException();
		}

		public async Task<string> UploadBase64Image(string baseImg, string root, string path = "")
		{
			throw new NotImplementedException();
		}

		string GetFileName(string fileName)
		{
			throw new NotImplementedException();
		}

		void VerifyPath(string path)
		{
			throw new NotImplementedException();
		}

		string TitleFromUri(Uri uri)
		{
			throw new NotImplementedException();
		}

		string PathToUrl(string path)
		{
			throw new NotImplementedException();
		}

		string GetImgSrcValue(string imgTag)
		{
			throw new NotImplementedException();
		}
    }
}
