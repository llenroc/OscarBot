using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Oscar.Bot
{
	public partial class Ids
	{
		[JsonProperty("trakt")]
		public long Trakt { get; set; }

		[JsonProperty("slug")]
		public string Slug { get; set; }

		[JsonProperty("tvdb")]
		public string Tvdb { get; set; }

		[JsonProperty("imdb")]
		public string Imdb { get; set; }

		[JsonProperty("tmdb")]
		public string Tmdb { get; set; }
	}
}
