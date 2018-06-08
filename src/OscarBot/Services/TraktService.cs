using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Oscar.Bot
{
	public class TraktService
	{
		HttpClient _client;

		public string AccessToken { get; set; }
		public string ClientID { get; set; }
		public string ClientSecret { get; set; }

		public string RedirectUrl
		{
			get;
			private set;
		}

		static TraktService _instance;

		public static TraktService Instance { get => _instance ?? (_instance = new TraktService { RedirectUrl = "https://www.google.com" }); }

		public void Reset()
		{
			_client = null;
		}

		async Task<T> GetContent<T>(string path)
		{
			if(_client == null)
			{
				_client = new HttpClient();
				_client.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-version", "2");
				_client.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-key", ClientID);
				_client.DefaultRequestHeaders.TryAddWithoutValidation("authorization", $"Bearer {AccessToken}");
			}

			if(!path.Contains("?"))
				path += "?a=b";

			var url = $"https://api.trakt.tv{path}";// + "&t=" + DateTime.Now.Ticks.ToString();
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

					if(response.StatusCode == System.Net.HttpStatusCode.NoContent)
						return default(T);

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

		async public Task<(Show, Episode)> GetNextEpisode(string showTitle)
		{
			var show = await GetShow(showTitle);

			if(show == null)
				return (null, null);

			var episode = await GetContent<Episode>($"/shows/{show.Ids.Trakt}/next_episode?extended=full,images");
			return (show, episode);
		}

		async public Task<(Show, Episode)> GetLastEpisode(string showTitle)
		{
			var show = await GetShow(showTitle);

			if(show == null)
				return (null, null);

			var episode = await GetContent<Episode>($"/shows/{show.Ids.Trakt}/last_episode?extended=full,images");
			return (show, episode);
		}

		async Task<Show> GetShow(string showTitle)
		{
			var results = await GetContent<List<SearchResult>>($"/search/show?query={showTitle}&extended=full");

			if(results.Count == 0)
				return null;

			return results.First().Show;
		}


		/*
		public Task<OAuthToken> GetTokenForCode(string code)
		{
			return new Task<OAuthToken>(() =>
			{
				//var handler = new NativeMessageHandler() {
				//	DisableCaching = true,
				//	Proxy = CoreFoundation.CFNetwork.GetDefaultProxy(),
				//	UseProxy = true,
				//};

				using(var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					var authCode = new OAuthCode
					{
						Code = code,
						ClientId = _traktClientID,
						ClientSecret = _traktClientSecret,
						RedirectUri = TraktService.Instance.RedirectUrl,
					};
					var codeJson = JsonConvert.SerializeObject(authCode);
					var content = new StringContent(codeJson, Encoding.UTF8, "application/json");
					var response = client.PostAsync("https://api-v2launch.trakt.tv/oauth/token", content).Result;
					var body = response.Content.ReadAsStringAsync().Result;

					var token = JsonConvert.DeserializeObject<OAuthToken>(body);
					return token;
				}
			});
		}


		public void ClearHistory(string keyFilter = null)
		{
			if(keyFilter != null)
			{
				_history.Keys.Where(k => k.ContainsNoCase(keyFilter)).ToList().ForEach(k => _history.Remove(k));
			}
			else
			{
				_history.Clear();
			}
		}

		public Task<Show> GetShow(int traktId)
		{
			return new Task<Show>(() => {
				var show = GetContent<Show>($"shows/{traktId}?extended=full,images").Result;
				return show;
			});
		}

		public Task<List<SearchResult>> SearchShows(string query)
		{
			return new Task<List<SearchResult>>(() =>
			{
				var keys = new Dictionary<string, object>();
				keys.Add("X-Pagination-Page:", 1);
				keys.Add("X-Pagination-Limit", 100);
				var list = GetContent<List<SearchResult>>("search?query={0}&type=show".Fmt(query), false, true, keys).Result;
				return list.OrderByDescending(r => r.Score).ToList();
			});
		}

		public Task<List<Season>> GetSeasonsForShow(Show show)
		{
			return new Task<List<Season>>(() =>
			{
				var list = GetContent<List<Season>>("shows/{0}/seasons?extended=full,images".Fmt(show.Identifiers.Trakt)).Result;

				if(list == null)
					return null;

				list = list.OrderBy(s => s.Number).ToList();
				return list;
			});
		}

		public Task<List<Episode>> GetEpisodesForSeason(Show show, Season season)
		{
			return new Task<List<Episode>>(() =>
			{
				var list = GetContent<List<Episode>>("shows/{0}/seasons/{1}?extended=full,images".Fmt(show.Identifiers.Trakt, season.Number)).Result;

				if(list == null)
					return null;

				list = list.OrderBy(e => e.InitialBroadcastDate).ToList();
				return list;
			});
		}

		public Task<ShowCast> GetCastAndCrew(Show show)
		{
			return new Task<ShowCast>(() => {
				var cast = GetContent<ShowCast>($"shows/{show.Identifiers.Trakt}/people?extended=images").Result;
				return cast;
			});
		}

		public Task<List<Show>> GetUpdatedShowsSince(DateTime time)
		{
			return new Task<List<Show>>(() =>
			{
				var list = GetContent<List<ShowUpdate>>("shows/updates/{0}/?limit=1000000".Fmt(time.ToString("O"))).Result;

				if(list == null)
					return null;

				var shows = list.OrderBy(s => s.Show.SortTitle).Select(s => s.Show).ToList();

				Debug.WriteLine("Count: " + list.Count);
				return shows;
			});
		}

		public Task<UserProfile> GetUserProfile()
		{
			return new Task<UserProfile>(() => {
				var profile = GetContent<UserProfile>("users/me").Result;
				return profile;
			});
		}

		public Task<List<ShowRating>> GetFavoriteShows()
		{
			return new Task<List<ShowRating>>(() => {
				var list = GetContent<List<ShowRating>>("sync/watchlist/shows?extended=full,images".Fmt(Settings.Instance.TraktUsername)).Result;
				return list;
			});
		}

		public Task AddShowToFavorites(Show show)
		{
			return new Task(() => {
				var client = new HttpClient();
				client.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-version", "2");
				client.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-key", _traktClientID);
				client.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer {0}".Fmt(Settings.Instance.AuthToken));

				var favorite = new FavoriteShow
				{
					Ids = new Ids { Trakt = show.Identifiers.Trakt }
				};

				var root = new FavoriteList();
				root.Shows.Add(favorite);

				var codeJson = JsonConvert.SerializeObject(root);
				var content = new StringContent(codeJson, Encoding.UTF8, "application/json");
				var response = client.PostAsync($"https://api-v2launch.trakt.tv/sync/watchlist", content).Result;
				var body = response.Content.ReadAsStringAsync().Result;

				Console.WriteLine(body);
			});
		}

		public Task RemoveShowFromFavorites(Show show)
		{
			return new Task(() => {
				var client = new HttpClient();
				client.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-version", "2");
				client.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-key", _traktClientID);
				client.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer {0}".Fmt(Settings.Instance.AuthToken));

				var favorite = new FavoriteShow
				{
					Ids = new Ids { Trakt = show.Identifiers.Trakt }
				};

				var root = new FavoriteList();
				root.Shows.Add(favorite);

				var codeJson = JsonConvert.SerializeObject(root);
				var content = new StringContent(codeJson, Encoding.UTF8, "application/json");
				var response = client.PostAsync($"https://api-v2launch.trakt.tv/sync/watchlist/remove", content).Result;
				var body = response.Content.ReadAsStringAsync().Result;

				Console.WriteLine(body);
			});
		}

		public Task RateShow(Show show, int rating)
		{
			return new Task(() => {
				var client = new HttpClient();
				client.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-version", "2");
				client.DefaultRequestHeaders.TryAddWithoutValidation("trakt-api-key", _traktClientID);
				client.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer {0}".Fmt(Settings.Instance.AuthToken));

				var showRating = new RatedShow
				{
					Title = show.Title,
					Year = show.Year.Value,
					Identifiers = show.Identifiers,
					Rating = rating,
				};

				var root = new RatedShowRoot();
				root.Shows.Add(showRating);

				var codeJson = JsonConvert.SerializeObject(root);
				var content = new StringContent(codeJson, Encoding.UTF8, "application/json");
				var response = client.PostAsync("https://api-v2launch.trakt.tv/sync/ratings", content).Result;
				var body = response.Content.ReadAsStringAsync().Result;

				Console.WriteLine(body);
			});
		}*/
	}
}
