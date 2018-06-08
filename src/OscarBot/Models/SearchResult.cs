using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Oscar.Bot
{
	public partial class SearchResult
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("score")]
		public float Score { get; set; }

		[JsonProperty("show")]
		public Show Show { get; set; }
	}
}
