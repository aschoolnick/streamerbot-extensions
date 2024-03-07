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
	public string CurrentWord
	{
		get 
		{
			string currentWord = CPH.GetGlobalVar<string>("WordleWord", true);
			if(string.IsNullOrWhiteSpace(currentWord))
			{
				CPH.SetGlobalVar("WordleWord", currentWord, true);
			}
			return currentWord;
		}
		set
		{
			CPH.SetGlobalVar("WordleWord", value, true);
		}
	}
	
	private string WordleGuess 
	{
		get 
		{
			return args.TryGetValue("rawInput", out object rawInput) ? rawInput?.ToString().ToLower() : string.Empty;
		}
    }
    
    private string? WordleWordList_FileLocation {
		get 
		{
			return CPH.GetGlobalVar<string?>("WORD_LIST_FILE_LOCATION");
		}
    }
    
    private string NewWordle_Msg {
		get 
		{
			return CPH.GetGlobalVar<string?>("NEW_WORDLE_MSG");
		}
    }
    
    private string NoActiveWordle_Msg {
		get 
		{
			return CPH.GetGlobalVar<string?>("NO_ACTIVE_WORDLE_MSG");
		}
    }
    
    private string WordleEnded_Msg {
		get 
		{
			return CPH.GetGlobalVar<string?>("WORDLE_ENDED_MSG");
		}
    }
    
    private int RandomWord_MinLength 
    {
		get 
		{
			return CPH.GetGlobalVar<int?>("RANDOMWORD_MINLENGTH") ?? 4;
		}
    }
    
    private int RandomWord_MaxLength 
    {
		get 
		{
			return CPH.GetGlobalVar<int?>("RANDOMWORD_MAXLENGTH") ?? 6;
		}
    }
    
    private string PreviousWordleWinner {
		get 
		{
			return CPH.GetGlobalVar<string?>("PreviousWordleWinner") ?? string.Empty;
		}
		set
		{
			CPH.SetGlobalVar("PreviousWordleWinner", Username, true);
		}
    }
    
    private string _username;
	public string Username
	{
		get 
		{
			if(string.IsNullOrWhiteSpace(_username))
			{
				_username = args.TryGetValue("userName", out object userName) ? userName?.ToString() : string.Empty;
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
				_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
			}
			return _client;
		}
	}
	
	private Random _random;
	private Random RandomGen
	{
		get
		{
			if(_random == null)
			{
				_random = new Random();
			}
			return _random;
		}
	}
	
	
	public List<LetterTile> LetterTiles;
	public bool Execute()
	{
		return true;
	}
	
	public bool NewWordle()
	{
		CPH.RunAction("_WORDLE_CONFIGURATIONS", true);
		if(string.IsNullOrWhiteSpace(CurrentWord))
		{
			StartNewWordle();
		}
		else
		{
			CPH.SendMessage("There is already an active Wordle game. It must end first before starting a new one", true);
		}
		return true;
	}
	
	public bool EndWordle()
	{
		if(!string.IsNullOrWhiteSpace(WordleEnded_Msg))
		{
			CPH.SendMessage($"{WordleEnded_Msg} The word was: {CurrentWord}", true);
		}
		CurrentWord = null;
		CPH.DisableTimer("WordleRoundTimer");
		return true;
	}
	
	public void StartNewWordle()
	{
		
		if(!string.IsNullOrWhiteSpace(WordleWordList_FileLocation) && File.Exists(WordleWordList_FileLocation))
		{
			var lines = File.ReadAllLines(WordleWordList_FileLocation);
			if(lines != null && lines?.Length > 0)
			{
				CurrentWord = lines[RandomGen.Next(0, lines.Length - 1)].ToLower();
				
			}
			
			if(string.IsNullOrWhiteSpace(CurrentWord))
			{
				CurrentWord = GetNewRandomWordleWord(RandomGen.Next(RandomWord_MinLength, RandomWord_MaxLength)).Result.ToLower();
			}
		}
		else
		{
			CurrentWord = GetNewRandomWordleWord(RandomGen.Next(RandomWord_MinLength, RandomWord_MaxLength)).Result.ToLower();
		}
		
		if(!string.IsNullOrWhiteSpace(NewWordle_Msg) && !string.IsNullOrWhiteSpace(CurrentWord))
		{
			string blankSpacesAppend = string.Empty;
			foreach(string s in CurrentWord.Select(x => x.ToString()).ToList())
			{
				blankSpacesAppend += "🔲";
			}
			CPH.SendMessage($"{NewWordle_Msg} {CurrentWord.Length} letters. {blankSpacesAppend}", true);
		}
		if(!string.IsNullOrWhiteSpace(CurrentWord))
		{
			CPH.EnableTimer("WordleRoundTimer");
		}
	}
	
	public bool GuessWordle()
	{
		if(!string.IsNullOrWhiteSpace(CurrentWord))
		{
			string userGuess = CleanseUserGuess(WordleGuess).Result;
			if(userGuess.Length == CurrentWord.Length && !userGuess.Contains("*"))
			{
				List<string> answerLetters = CurrentWord.Select(x => x.ToString()).ToList();
				LetterTiles = userGuess.Select(x => new LetterTile()
				{
					Letter = x.ToString(),
					Letterstate = Letterstate.Blank
				}).ToList();
				
				for(var x = 0; x < LetterTiles.Count; x++)
				{
					if(answerLetters[x] == LetterTiles[x].Letter)
					{
						LetterTiles[x].Letterstate = Letterstate.Correct;
						answerLetters[x] = null;
					}
				}
				answerLetters.RemoveAll(x => x == null);
				foreach(LetterTile L in LetterTiles)
				{
					if(L.Letterstate == Letterstate.Blank && answerLetters.Contains(L.Letter))
					{
						L.Letterstate = Letterstate.InWordWrongSpot;
						answerLetters[answerLetters.IndexOf(L.Letter)] = null;
					}
				}
				answerLetters.RemoveAll(x => x == null);
				_ = LetterTiles.Where(x => x.Letterstate == Letterstate.Blank).All(x => {
					x.Letterstate = Letterstate.NotInWord;
					return true;
				});
				
				int numberRight = LetterTiles.Where(x => x.Letterstate == Letterstate.Correct).ToList().Count();
				bool isCorrect = numberRight == CurrentWord.Length;
				string wordGuessMsgLetters = string.Empty;
				string wordGuessMsgSymbols = string.Empty;
				string chatResponse = isCorrect ?
										$"Congratulations @{Username}! You got it right!" : $"@{Username} got {numberRight} / {CurrentWord.Length} correct. ";
				if(numberRight != CurrentWord.Length)
				{
					foreach(LetterTile L in LetterTiles)
					{
						switch(L.Letterstate)
						{
							case Letterstate.Correct:
								wordGuessMsgSymbols += $"🟩";
								wordGuessMsgLetters += L.Letter;
								break;
							case Letterstate.InWordWrongSpot:
								wordGuessMsgSymbols += "🟨";
								wordGuessMsgLetters += "▫";
								break;
							default:
								wordGuessMsgSymbols += "⬛️";
								wordGuessMsgLetters += "▫";
								break;
						}
					}
				}
				string chatResponseAppend = numberRight != CurrentWord.Length ? $"⠀⠀⠀⠀⠀⠀⠀⠀⠀{wordGuessMsgSymbols}︱{wordGuessMsgLetters}" : string.Empty;
				CPH.SendMessage($"{chatResponse}{chatResponseAppend}");
				if(isCorrect)
				{
					//PreviousWordleWinner = Username;
					EndWordle();
				}
			}
		}
		else
		{
			if(!string.IsNullOrWhiteSpace(NoActiveWordle_Msg))
			{
				//string previousWordleWinnerAddon = !string.IsNullOrWhiteSpace(PreviousWordleWinner) ? $" 🎉 @{PreviousWordleWinner} won last round!" : string.Empty;
				string previousWordleWinnerAddon = string.Empty;
				CPH.SendMessage($"{NoActiveWordle_Msg}{previousWordleWinnerAddon}", true);
			}
		}
		return true;
	}
	
	private async Task<string> CleanseUserGuess(string guess)
	{
		HttpResponseMessage response = await Client.GetAsync($"https://www.purgomalum.com/service/plain?text={guess}", HttpCompletionOption.ResponseHeadersRead);
		if(response.IsSuccessStatusCode)
		{
			return await response.Content.ReadAsStringAsync();
		}
		return string.Empty;
	}
	
	private async Task<string> GetNewRandomWordleWord(int characterLength)
    {
    	string newWord = string.Empty;
		HttpResponseMessage response = await Client.GetAsync($"https://random-word-api.herokuapp.com/word?length={characterLength}", HttpCompletionOption.ResponseHeadersRead);
		if(response.IsSuccessStatusCode)
		{
			newWord = JsonConvert.DeserializeObject<List<string>>(await response.Content.ReadAsStringAsync())?.FirstOrDefault();
		}
		if(!string.IsNullOrWhiteSpace(newWord))
		{
			response = await Client.GetAsync($"https://www.purgomalum.com/service/plain?text={newWord}", HttpCompletionOption.ResponseHeadersRead);
		}
		else
		{
			return string.Empty;
		}
		if(response.IsSuccessStatusCode)
		{
			newWord = await response.Content.ReadAsStringAsync();
		}
		if(newWord.Contains("*"))
		{
			newWord = string.Empty;
			CPH.SendMessage("Error: Wordle word could not be generated for this round. Please try to run action again.");
		}
		return newWord;
    }
}

public class LetterTile
{
	public string Letter { get; set; }
	public Letterstate Letterstate { get; set; }
}

public enum Letterstate
{
	Blank,
	NotInWord,
	InWordWrongSpot,
	Correct
}