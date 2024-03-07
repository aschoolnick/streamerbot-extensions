//////////////////////////////////////////////////////////////////////////////////////////////
// DO NOT MAKE ANY CHANGES TO THIS FILE UNLESS YOU KNOW WHAT YOU ARE DOING!!!!!!!!!!!!!!!!!!//
//////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
public class CPHInline
{
	/* PROPERTIES */
    private int UserEnteredAdDuration {
		get 
		{
			return CPH.GetGlobalVar<int?>("AD_DURATION_IN_SECONDS") ?? 60;
		}
    }
    
    private string UserEnteredChatAlertMessage {
		get 
		{
			return CPH.GetGlobalVar<string>("LOCKDOWN_MESSAGE") ?? "Stream is now on lockdown. Chat user requirements have been temporarily elevated.";
		}
    }
    
    private bool ShowChatAlert {
		get 
		{
			return CPH.GetGlobalVar<bool?>("SHOW_LOCKDOWN_MESSAGE") ?? false;
		}
    }
    
    private bool ShowAd {
		get 
		{
			return CPH.GetGlobalVar<bool?>("SHOW_AD_ON_LOCKDOWN") ?? false;
		}
    }
    
    private bool CreateStreamMarkerOnLockdown {
		get 
		{
			return CPH.GetGlobalVar<bool?>("CREATE_STREAM_MARKER_ON_LOCKDOWN") ?? false;
		}
    }
    
    private string StreamElementsJWTToken {
		get 
		{
			return CPH.GetGlobalVar<string>("STREAMELEMENTS_JWT_TOKEN") ?? string.Empty;
		}
    }
    
    private List<int> ValidAdDurations = new List<int>() {30, 60, 90, 120, 150, 180};
    private int? _adDurationInSeconds;
	private int AdDuration_InSeconds
	{ 
		get 
		{
			if(_adDurationInSeconds == null)
			{
				_adDurationInSeconds = ValidAdDurations.OrderBy(i => Math.Abs(UserEnteredAdDuration - i)).First();
			}
			return (int)_adDurationInSeconds;
		}
	}
	
	private string _username;
	private string Username
	{
		get 
		{
			if(string.IsNullOrWhiteSpace(_username))
			{
				_username = args["userName"]?.ToString();
			}
			return _username;
		}
	}
	
	private HttpClient _client;
	private HttpClient Client
	{
		get
		{
			if(_client == null)
			{
				_client = new HttpClient();
				_client.DefaultRequestHeaders.Accept.Clear();
				_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", StreamElementsJWTToken);
			}
			return _client;
		}
	}

	/* BEGIN EXECUTION */
	
    public bool Execute()
    {
        return true;
    }
    
    public bool BeginLockdown()
    {
    	CPH.RunAction("_CONFIGURATIONS_FOR_LOCKDOWN");
    	return Lockdown().Result;
    }
    
    public bool TerminateLockdown()
    {
    	return EndLockdown().Result;
    }
    
    // HANDLES REPEATING LOCKDOWN BEHAVIORS
    public bool LockdownBehaviorOnRepeatedInterval()
    {
    	if(ShowChatAlert)
    	{
			CPH.TwitchAnnounce(UserEnteredChatAlertMessage);
    	}
    	CPH.RunAction("_OTHER_ACTIONS_TO_RUN_WHILE_LOCKDOWN_IS_ACTIVE");
    	return true;
    }
    
    // HANDLES BEHAVIORS OF WHEN LOCKDOWN IS STARTED
    public async Task<bool> Lockdown()
    {
    	CPH.EnableTimer("LOCKDOWN_EndTimer");
    	CPH.EnableTimer("LOCKDOWN_RepeatingActionInterval");
    	string channelId = await GetStreamElementsChannelID();
    	SetStreamElementsAlertsState(channelId, false);
    	ClearRecentStreamElementsSessionData(channelId);
    	
    	if(ShowAd)
    	{
			CPH.TwitchRunCommercial(AdDuration_InSeconds);
    	}
    	if(CreateStreamMarkerOnLockdown)
    	{
    		CPH.CreateStreamMarker("Lockdown");
    	}
    	CPH.RunAction("_OTHER_ACTIONS_TO_RUN_ON_LOCKDOWN_START");
    	LockdownBehaviorOnRepeatedInterval();
    	return true;
    }
    
    // HANDLES BEHAVIORS OF WHEN LOCKDOWN IS ENDED
    public async Task<bool> EndLockdown()
    {
    	CPH.DisableTimer("LOCKDOWN_EndTimer");
    	CPH.DisableTimer("LOCKDOWN_RepeatingActionInterval");
    	string channelId = await GetStreamElementsChannelID();
    	SetStreamElementsAlertsState(channelId, true);
    	CPH.RunAction("_OTHER_ACTIONS_TO_RUN_ON_LOCKDOWN_END");
    	return true;
    }
    
    
    
    
    // Pause/Unpause StreamElements alerts
    private async Task<bool> SetStreamElementsAlertsState(string channelId, bool active)
    {
    	if(!string.IsNullOrWhiteSpace(channelId))
    	{
			string state = active ? "unpause" : "pause";
			var content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("action", state)
			});
			HttpResponseMessage response = await Client.PutAsync($"https://api.streamelements.com/kappa/v3/overlays/{channelId}/action", content);
			return response.StatusCode == HttpStatusCode.Created;
    	}
    	return false;
    }
    
    // Clear StreamElements data to remove any bad usernames from display on screen
    private async Task<bool> ClearRecentStreamElementsSessionData(string channelId)
    {
    	if(!string.IsNullOrWhiteSpace(channelId))
    	{
    		StreamElementsSessionRequestPayload payload = new StreamElementsSessionRequestPayload() 
    		{
    			FollowerLatest = new ActivityInfo(),
				SubscriberLatest = new ActivityInfo(),
				TipLatest = new ActivityInfo(),
				CheerLatest = new ActivityInfo(),
				HostLatest = new ActivityInfo(),
				TipRecent = new object[]{},
				SubscriberRecent = new object[]{},
				SubscriberNewRecent = new object[]{},
				SubscriberGiftedRecent = new object[]{},
				SubscriberResubRecent = new object[]{},
				FollowerRecent = new object[]{},
				CheerRecent = new object[]{},
				HostRecent = new object[]{}
    		};
    		var stringContent = new StringContent(JsonConvert.SerializeObject(payload), UnicodeEncoding.UTF8, "application/json");
			HttpResponseMessage response = await Client.PutAsync($"https://api.streamelements.com/kappa/v2/sessions/{channelId}", stringContent);
			return response.StatusCode == HttpStatusCode.OK;
    	}
    	return false;
    }
    
    // Get connected StreamElements twitch channel ID (need this for API calls)
    private async Task<string> GetStreamElementsChannelID()
    {
    	if(!string.IsNullOrWhiteSpace(StreamElementsJWTToken))
    	{
			StreamElementsChannelInfo channel = null;
			
			HttpResponseMessage response = await Client.GetAsync($"https://api.streamelements.com/kappa/v2/channels/me");
			
			if(response.IsSuccessStatusCode)
			{
				var responseContent = await response.Content.ReadAsStringAsync();
				channel = JsonConvert.DeserializeObject<StreamElementsChannelInfo>(responseContent);
				
			}
			return channel?._Id;
    	}
    	return string.Empty;
    }
}


// Required payload models


public class StreamElementsChannelInfo
{
	public object Profile {get; set;}
	public string Provider {get; set;}
	public bool Suspended {get; set;}
	public bool NullChannel {get; set;}
	public object[] ProviderEmails {get; set;}
	public string LastJWTToken {get; set;}
	public string _Id {get; set;}
	public string Email {get; set;}
	public string Avatar {get; set;}
	public bool Verified {get; set;}
	public string Username {get; set;}
	public string Alias {get; set;}
	public string DisplayName {get; set;}
	public string ProviderId {get; set;}
	public string AccessToken {get; set;}
	public string ApiToken {get; set;}
	public bool IsPartner {get; set;}
	public string BroadcasterType {get; set;}
	public object[] Users {get; set;}
	public object Ab {get; set;}
	public string CreatedAt {get; set;}
	public string UpdatedAt {get; set;}
	public string LastLogin {get; set;}
	public string Country {get; set;}
	public object ProviderTotals {get; set;}
	public object Features {get; set;}
	public string Geo {get; set;}
	public string Type {get; set;}
}

public class StreamElementsSessionRequestPayload
{
	[JsonProperty("follower-latest")]
	public ActivityInfo FollowerLatest {get; set;}
	[JsonProperty("subscriber-latest")]
	public ActivityInfo SubscriberLatest {get; set;}
	[JsonProperty("tip-latest")]
	public ActivityInfo TipLatest {get; set;}
	[JsonProperty("cheer-latest")]
	public ActivityInfo CheerLatest {get; set;}
	[JsonProperty("host-latest")]
	public ActivityInfo HostLatest {get; set;}
	[JsonProperty("tip-recent")]
	public object[] TipRecent {get; set;}
	[JsonProperty("subscriber-recent")]
	public object[] SubscriberRecent {get; set;}
	[JsonProperty("subscriber-new-recent")]
	public object[] SubscriberNewRecent {get; set;}
	[JsonProperty("subscriber-gifted-recent")]
	public object[] SubscriberGiftedRecent {get; set;}
	[JsonProperty("subscriber-resub-recent")]
	public object[] SubscriberResubRecent {get; set;}
	[JsonProperty("follower-recent")]
	public object[] FollowerRecent {get; set;}
	[JsonProperty("cheer-recent")]
	public object[] CheerRecent {get; set;}
	[JsonProperty("host-recent")]
	public object[] HostRecent {get; set;}
}

public class ActivityInfo
{
	public string Name {get; set;} = string.Empty;
	public int Amount {get; set;} = 0;
	public int Count {get; set;} = 0;
}
