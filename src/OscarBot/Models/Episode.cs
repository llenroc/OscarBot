using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Oscar.Bot
{
	public partial class Episode
	{
		[JsonProperty("season")]
		public long Season { get; set; }

		[JsonProperty("number")]
		public long Number { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("ids")]
		public Ids Ids { get; set; }

		[JsonProperty("number_abs")]
		public object NumberAbs { get; set; }

		[JsonProperty("overview")]
		public string Overview { get; set; }

		[JsonProperty("rating")]
		public long Rating { get; set; }

		[JsonProperty("votes")]
		public long Votes { get; set; }

		[JsonProperty("comment_count")]
		public long CommentCount { get; set; }

		[JsonProperty("first_aired")]
		public DateTimeOffset FirstAired { get; set; }

		[JsonProperty("updated_at")]
		public DateTimeOffset UpdatedAt { get; set; }

		//[JsonProperty("available_translations")]
		//public List<string> AvailableTranslations { get; set; }

		[JsonProperty("runtime")]
		public long Runtime { get; set; }

		public long ShowTitle { get; set; }
	}
}
