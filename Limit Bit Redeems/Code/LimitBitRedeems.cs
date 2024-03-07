using System;

public class CPHInline
{
	public int? UserRedeemCount
	{
		get
		{
			int? count = null;
			string userId = args["userId"].ToString();
			if(!string.IsNullOrWhiteSpace(userId))
			{
				count = CPH.GetGlobalVar<int>("RedeemCount-" + SourceName + "-" + userId, false);
				if(count == null)
				{
					CPH.SetGlobalVar("RedeemCount-" + SourceName + "-" + userId, 0, false);
					count = 0;
				}
			}
			return count;
		}
		set
		{
			string userId = args["userId"].ToString();
			if(!string.IsNullOrWhiteSpace(userId))
			{
				CPH.SetGlobalVar("RedeemCount-" + SourceName + "-" + userId, value, false);
			}
		}
	}
	
	private int? _userRedeemLimit;
	private int UserRedeemLimit 
	{ 
		get 
		{
			if(_userRedeemLimit == null)
			{
				_userRedeemLimit = int.TryParse(args["UserRedeemLimit"]?.ToString(), out int result) ? result : 5;
			}
			return (int)_userRedeemLimit;
		}
	}
	
	private int? _sourceLengthMilliseconds;
	private int SourceLengthMilliseconds 
	{ 
		get 
		{
			if(_sourceLengthMilliseconds == null)
			{
				_sourceLengthMilliseconds = int.TryParse(args["SourceLengthMilliseconds"]?.ToString(), out int result) ? result : 1000;
			}
			return (int)_sourceLengthMilliseconds;
		}
	}
	
	private string? _sceneName;
	private string SceneName 
	{ 
		get 
		{
			if(_sceneName == null)
			{
				_sceneName = args["SceneName"]?.ToString() ?? string.Empty;
			}
			return _sceneName;
		}
	}
	
	private string? _sourceName;
	private string SourceName 
	{ 
		get 
		{
			if(_sourceName == null)
			{
				_sourceName = args["SourceName"]?.ToString() ?? string.Empty;
			}
			return _sourceName;
		}
	}
	
	private string? _actionName;
	private string ActionName 
	{ 
		get 
		{
			if(_actionName == null)
			{
				_actionName = args["ActionName"]?.ToString() ?? string.Empty;
			}
			return _actionName;
		}
	}
	
	private string? _messageOneLeft;
	private string MessageOneLeft 
	{ 
		get 
		{
			if(_messageOneLeft == null)
			{
				_messageOneLeft = args["MessageOneLeft"]?.ToString() ?? string.Empty;
			}
			return _messageOneLeft;
		}
	}
	
	private string? _messageNoneLeft;
	private string MessageNoneLeft 
	{ 
		get 
		{
			if(_messageNoneLeft == null)
			{
				_messageNoneLeft = args["MessageNoneLeft"]?.ToString() ?? string.Empty;
			}
			return _messageNoneLeft;
		}
	}
	
	public bool Execute()
	{
		if(UserRedeemCount == UserRedeemLimit - 1 && !string.IsNullOrWhiteSpace(MessageOneLeft))
		{
			CPH.SendMessage($"@{args["userName"].ToString()} - {MessageOneLeft}");
		}
		if(UserRedeemCount == UserRedeemLimit && !string.IsNullOrWhiteSpace(MessageNoneLeft))
		{
			CPH.SendMessage($"@{args["userName"].ToString()} - {MessageNoneLeft}");
		}
		if(UserRedeemCount <= UserRedeemLimit)
		{
			if(!string.IsNullOrWhiteSpace(ActionName))
			{
				CPH.RunAction(ActionName);
			}
			else if(!string.IsNullOrWhiteSpace(SceneName) && !string.IsNullOrWhiteSpace(SourceName))
			{
				ShowSource(SceneName, SourceName);
				CPH.Wait(SourceLengthMilliseconds);
				HideSource(SceneName, SourceName);
			}
			else
			{
				CPH.SendMessage("Something went wrong, either no action is associated with this redeem, or SceneName/SourceName do not map to a valid source in OBS");
			}
		}
		UserRedeemCount++;
		return true;
	}
	
	private void ShowSource(string scene, string source)
	{
		CPH.ObsSetSourceVisibility(scene, source, true);
	}
	private void HideSource(string scene, string source)
	{
		CPH.ObsSetSourceVisibility(scene, source, false);
	}
}
