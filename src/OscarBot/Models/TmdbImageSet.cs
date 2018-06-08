using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Oscar.Bot
{
	public partial class TmdbImageSet
	{
		[JsonProperty("backdrops")]
		public List<TmdbImage> Backdrops { get; set; }

		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("posters")]
		public List<TmdbImage> Posters { get; set; }
	}

	public partial class TmdbImage
	{
		[JsonProperty("aspect_ratio")]
		public double AspectRatio { get; set; }

		[JsonProperty("file_path")]
		public string FilePath { get; set; }

		[JsonProperty("height")]
		public long Height { get; set; }

		[JsonProperty("iso_639_1")]
		public string Iso639_1 { get; set; }

		[JsonProperty("vote_average")]
		public double VoteAverage { get; set; }

		[JsonProperty("vote_count")]
		public long VoteCount { get; set; }

		[JsonProperty("width")]
		public long Width { get; set; }
	}
}
