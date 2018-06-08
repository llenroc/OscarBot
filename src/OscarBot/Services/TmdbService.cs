using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Oscar.Bot
{
	public class TmdbService

	{
		HttpClient _client;

		public string ApiKey { get; set; }

		readonly string _baseApiUrl = "https://api.themoviedb.org/3";
		readonly string _baseImageUrl = "http://image.tmdb.org/t/p";
		readonly string _backdropImageSize = "1280";
		static TmdbService _instance;

		public static TmdbService Instance { get => _instance ?? (_instance = new TmdbService()); }


		public void Reset()
		{
			_client = null;
		}

		async Task<T> GetContent<T>(string path)
		{
			if(_client == null)
				_client = new HttpClient();

			var url = $"{_baseApiUrl}{path}";
			Debug.WriteLine($"Requesting JSON from {url}");

			try
			{
				string json;
				using(var response = await _client.GetAsync(url))
				{
					json = await response.Content.ReadAsStringAsync();

					if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					{
						Debug.WriteLine("UNAUTHORIZED");
						return default(T);
					}

					if(response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrWhiteSpace(json))
					{
						Debug.WriteLine("Invalid response from server:\n{0}", json);
						throw new HttpRequestException("Unable to get valid JSON from Trakt server");
					}
				}

				Debug.WriteLine(json);
				return JsonConvert.DeserializeObject<T>(json);
			}
			catch(Exception e)
			{
				Debug.WriteLine(e);
				throw;
			}
		}

		async public Task<string> GetShowImageUrlLandscape(string tmdbShowId)
		{
			var imageSet = await GetContent<TmdbImageSet>($"/tv/{tmdbShowId}/images?api_key={ApiKey}");
			var url = $"{_baseImageUrl}/w{_backdropImageSize}{imageSet.Backdrops.First().FilePath}";
			return url;
		}
	}
}
