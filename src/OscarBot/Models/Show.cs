using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Oscar.Bot
{
	public partial class Show
	{
		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("year")]
		public long? Year { get; set; }

		[JsonProperty("ids")]
		public Ids Ids { get; set; }

		[JsonProperty("overview")]
		public string Overview { get; set; }

		[JsonProperty("first_aired")]
		public DateTimeOffset? FirstAired { get; set; }

		[JsonProperty("runtime")]
		public long Runtime { get; set; }

		[JsonProperty("certification")]
		public string Certification { get; set; }

		[JsonProperty("network")]
		public string Network { get; set; }

		[JsonProperty("country")]
		public string Country { get; set; }

		[JsonProperty("trailer")]
		public string Trailer { get; set; }

		[JsonProperty("homepage")]
		public string Homepage { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("rating")]
		public double Rating { get; set; }

		[JsonProperty("votes")]
		public long Votes { get; set; }

		[JsonProperty("comment_count")]
		public long CommentCount { get; set; }

		[JsonProperty("updated_at")]
		public DateTimeOffset UpdatedAt { get; set; }

		[JsonProperty("language")]
		public string Language { get; set; }

		[JsonProperty("available_translations")]
		public List<string> AvailableTranslations { get; set; }

		[JsonProperty("genres")]
		public List<string> Genres { get; set; }

		[JsonProperty("aired_episodes")]
		public long AiredEpisodes { get; set; }
	}
}
