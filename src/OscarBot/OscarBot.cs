using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Prompts = Microsoft.Bot.Builder.Prompts;
using System;
using Newtonsoft.Json.Linq;
using Microsoft.Recognizers.Text;

namespace Oscar.Bot
{
	public class OscarBot : IBot
	{
		readonly string _greetingMessage = "Hi! My name is Oscar. I can tell you when the next episode of a TV show will air or who plays a character in a show.";
		private const double LUIS_INTENT_THRESHOLD = 0.2d;

		private readonly DialogSet dialogs;

		public OscarBot()
		{
			dialogs = new DialogSet();
			dialogs.Add("None", new WaterfallStep[] { DefaultDialog });
			dialogs.Add("Greeting", new WaterfallStep[] { GreetingDialog });
			dialogs.Add("TV_NextEpisode", new WaterfallStep[] { ResolveEpisodeInquiry, AskProvideAdditionalInfo, ProvideAdditionalEpisodeInfo });
			dialogs.Add("TV_CastInfo", new WaterfallStep[] { ResolveCastInfoInquiry });
			dialogs.Add("ProvideAdditionalInfoPrompt", new ChoicePrompt(Culture.English));
		}

		#region Resolve Episode Inquiry

		private async Task ResolveEpisodeInquiry(DialogContext dialogContext, object args, SkipStepFunction next)
		{
			var inquiry = new EpisodeInquiry(dialogContext.ActiveDialog.State);

			if(args is IDictionary<string, object> hash)
			{
				if(hash["LuisResult"] is RecognizerResult luisResult)
				{
					var title = GetEntity<string>(luisResult, "Media_Title");
					if(!string.IsNullOrEmpty(title))
					{
						inquiry.MediaTitle = title;
						dialogContext.ActiveDialog.State = inquiry;
					}
				}
			}

			var (show, episode) = await TraktService.Instance.GetNextEpisode(inquiry.MediaTitle);

			if(show == null)
			{
				var response = $"Hrmm, it doesn't look like there are any shows called '{inquiry.MediaTitle}'.";
				//TODO: Possibly prompt them for an alternate title

				await dialogContext.Context.SendActivity(response);
				await dialogContext.End();
				return;
			}

			inquiry.Episode = episode;
			inquiry.Show = show;
			dialogContext.ActiveDialog.State = inquiry;

			if(episode == null)
			{
				var response = $"It doesn't look like {show.Title} has any upcoming episodes.";
				await dialogContext.Context.SendActivity(response);
				await dialogContext.End();
			}
			else
			{
				var response = $"The next episode of {show.Title} will air on {episode.FirstAired.LocalDateTime.ToString("dddd, MMMM d 'at' h:mm tt")} on {show.Network}.";
				await dialogContext.Context.SendActivity(response);

				var userContext = dialogContext.Context.GetUserState<UserState>();
				userContext.EpisodeInquiries.Add(inquiry);

				await dialogContext.Continue();
			}
		}

		#endregion

		#region Provide Additional Info

		private async Task AskProvideAdditionalInfo(DialogContext dialogContext, object args, SkipStepFunction next)
		{
			var inquiry = new EpisodeInquiry(dialogContext.ActiveDialog.State);

			if(inquiry.Episode == null)
			{
				await dialogContext.Continue();
				return;
			}

			var item = inquiry.Episode.Overview == null ? "show" : "episode";
			var yes = new Prompts.Choices.Choice { Value = "Yes" };
			var no = new Prompts.Choices.Choice { Value = "No" };
			await dialogContext.Prompt("ProvideAdditionalInfoPrompt", $"Would you like to see additional info about this {item}?", new ChoicePromptOptions() { Choices = new List<Prompts.Choices.Choice> { yes, no } });
		}

		private async Task ProvideAdditionalEpisodeInfo(DialogContext dialogContext, object args, SkipStepFunction next)
		{
			var inquiry = new EpisodeInquiry(dialogContext.ActiveDialog.State);

			if(args is Prompts.ChoiceResult result)
			{
				inquiry.ProvideAdditionalInfo = result.Value.Value == "Yes";
			}

			if(inquiry.ProvideAdditionalInfo)
			{
				if(inquiry.Episode.Overview != null)
				{
					var response = $"Summary: {inquiry.Episode.Overview}";
					await dialogContext.Context.SendActivity(response);
				}

				var imageUrl = await TmdbService.Instance.GetShowImageUrlLandscape(inquiry.Show.Ids.Tmdb);
				var activity = MessageFactory.Attachment(GetInternetAttachment(imageUrl, inquiry.Show.Title));
				await dialogContext.Context.SendActivity(activity);
			}

			await dialogContext.End();
		}

		/*async Task AllowRerunsValidator(ITurnContext context, Prompts.TextResult result)
		{
			if(string.IsNullOrWhiteSpace(result.Value)
				|| (!result.Value.Equals("yes", StringComparison.CurrentCultureIgnoreCase) && !result.Value.Equals("no", StringComparison.CurrentCultureIgnoreCase))
				&& (!result.Value.Equals("y", StringComparison.CurrentCultureIgnoreCase) && !result.Value.Equals("n", StringComparison.CurrentCultureIgnoreCase)))
			{
				result.Status = Prompts.PromptStatus.NotRecognized;
				await context.SendActivity("Please reply with 'yes' or 'no'");
			}
		}
		private Task DefaultDialog(DialogContext dialogContext, object args, SkipStepFunction next)
		{
			return dialogContext.Context.SendActivity("I'm sorry, I'm not quite sure what you mean.");
		}*/

		#endregion

		#region Resolve Cast Info Inquiry

		private async Task ResolveCastInfoInquiry(DialogContext dialogContext, object args, SkipStepFunction next)
		{
			var inquiry = new CastInfoInquiry(dialogContext.ActiveDialog.State);

			if(args is IDictionary<string, object> hash)
			{
				if(hash["LuisResult"] is RecognizerResult luisResult)
				{
					var title = GetEntity<string>(luisResult, "Media_Title");
					if(!string.IsNullOrEmpty(title))
						inquiry.MediaTitle = title;

					var characterName = GetEntity<string>(luisResult, "Media_CharacterName");
					if(!string.IsNullOrEmpty(characterName))
						inquiry.CharacterName = characterName;

					dialogContext.ActiveDialog.State = inquiry;
				}
			}

			var (show, episode) = await TraktService.Instance.GetNextEpisode(inquiry.MediaTitle);

			if(show == null)
			{
				var response = $"Hrmm, it doesn't look like there are any shows called '{inquiry.MediaTitle}'.";
				//TODO: Possibly prompt them for an alternate title

				await dialogContext.Context.SendActivity(response);
				await dialogContext.End();
				return;
			}

			//inquiry.Episode = episode;
			//inquiry.Show = show;
			dialogContext.ActiveDialog.State = inquiry;

			if(episode == null)
			{
				var response = $"It doesn't look like {show.Title} has any upcoming episodes.";
				await dialogContext.Context.SendActivity(response);
				await dialogContext.End();
			}
			else
			{
				var response = $"The next episode of {show.Title} will air on {episode.FirstAired.LocalDateTime.ToString("dddd, MMMM d 'at' h:mm tt")} on {show.Network}.";
				await dialogContext.Context.SendActivity(response);

				var userContext = dialogContext.Context.GetUserState<UserState>();
				//userContext.EpisodeInquiries.Add(inquiry);

				await dialogContext.Continue();
			}
		}

		#endregion

		#region Turn / Utilities

		public async Task OnTurn(ITurnContext context)
		{
			if(context.Activity.Type == ActivityTypes.ConversationUpdate && context.Activity.MembersAdded.FirstOrDefault()?.Id == context.Activity.Recipient.Id)
			{
				await context.SendActivity(_greetingMessage);
			}
			else if(context.Activity.Type == ActivityTypes.Message)
			{
				var userState = context.GetUserState<UserState>();
				if(userState.EpisodeInquiries == null)
					userState.EpisodeInquiries = new List<EpisodeInquiry>();

				var state = context.GetConversationState<Dictionary<string, object>>();
				var dialogContext = dialogs.CreateContext(context, state);

				var utterance = context.Activity.Text.ToLowerInvariant();
				if(utterance == "cancel")
				{
					if(dialogContext.ActiveDialog != null)
					{
						await context.SendActivity("Ok... Cancelled");
						dialogContext.EndAll();
					}
					else
					{
						await context.SendActivity("Nothing to cancel.");
					}
				}

				if(!context.Responded)
				{
					await dialogContext.Continue();

					if(!context.Responded)
					{
						var luisResult = context.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);
						var (intent, score) = luisResult.GetTopScoringIntent();
						var intentResult = score > LUIS_INTENT_THRESHOLD ? intent : "None";

						await dialogContext.Begin(intent, new Dictionary<string, object> { { "LuisResult", luisResult } });
					}
				}
			}
		}

		private Task DefaultDialog(DialogContext dialogContext, object args, SkipStepFunction next)
		{
			return dialogContext.Context.SendActivity("I'm sorry, I'm not quite sure what you mean.");
		}

		private Task GreetingDialog(DialogContext dialogContext, object args, SkipStepFunction next)
		{
			return dialogContext.Context.SendActivity("Howdy");
		}

		T GetEntity<T>(RecognizerResult luisResult, string entityKey)
		{
			var data = luisResult.Entities as IDictionary<string, JToken>;
			if(data.TryGetValue(entityKey, out JToken value))
			{
				return value.First.Value<T>();
			}
			return default(T);
		}

		Attachment GetInternetAttachment(string imageUrl, string name)
		{
			var ext = System.IO.Path.GetExtension(imageUrl);
			return new Attachment
			{
				Name = $"{name}.{ext}",
				ContentType = $"image/{ext}",
				ContentUrl = imageUrl
			};
		}

		#endregion
	}
}