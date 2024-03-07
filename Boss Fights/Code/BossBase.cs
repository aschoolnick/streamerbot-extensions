using System;
using System.IO;

public class CPHInline
{
	private const string BOSS_FIGHT_SCENE = "BossFight";
	private Boss _newBoss;
	public bool Execute()
	{
		Func<Boss>[] bosses = new Func<Boss>[] {
			() => new InsertNameBoss(CPH),
		};
		Random rand = new Random();
		_newBoss = bosses[rand.Next(bosses.Length)]();
		ConfigureBossFightStart();
		return true;
	}
	
	private void ConfigureBossFightStart()
	{
		CPH.SetGlobalVar("BattleCancelled", false);
		CPH.ObsSetMediaSourceFile(BOSS_FIGHT_SCENE, "SpawnSound", _newBoss.BossSpawnSound);
		// SHOW AURA
		ShowSource("SpawnAura");
		ShowSource("SpawnSound");
		CPH.Wait(_newBoss.BossSpawnSoundLength);
		HideSource("SpawnAura");
		HideSource("SpawnSound");
		
		// SETUP TEXT
		SetText("HealthName", _newBoss.BossName);
		SetText("HealthLeft", $"{_newBoss.BossHealth}/{_newBoss.BossHealth}");
		
		SetText("BossSubmitName", _newBoss.BossSubmittedByName);
		SetText("ArtByName", _newBoss.BossArtByName);
		SetText("BossMusicByName", _newBoss.BossMusicSongAndArtistName);
		
		// SET CHAT'S HEALTH
		int chatHealth = 10;
		CPH.SetGlobalVar("ChatHealthStart", chatHealth);
		CPH.SetGlobalVar("ChatHealth", chatHealth);
		SetText("ChatHealthLeft", $"{chatHealth}/{chatHealth}");
		
		// CHANGE MUSIC SOURCE
		CPH.ObsSetMediaSourceFile(BOSS_FIGHT_SCENE, "BossMusicSource", _newBoss.BossMusicFileURL);
		ShowSource("BossMusicSource");
		
		// CHANGE IMAGE SOURCE
		CPH.ObsSetImageSourceFile(BOSS_FIGHT_SCENE, "Boss00", _newBoss.BossImageFileURL);
		ShowSource("Boss00");
		
		// SET VICTORY/DEATH SOUNDS
		CPH.ObsSetMediaSourceFile(BOSS_FIGHT_SCENE, "Victory", _newBoss.BossVictorySound);
		CPH.ObsSetMediaSourceFile(BOSS_FIGHT_SCENE, "Loss", _newBoss.BossDefeatSound);
		
		// SET HP BAR BG
		CPH.ObsSetImageSourceFile(BOSS_FIGHT_SCENE, "Health3", $"{_newBoss.BossHealthBarDirectory}/Health3.png");
		CPH.ObsSetImageSourceFile(BOSS_FIGHT_SCENE, "Health2", $"{_newBoss.BossHealthBarDirectory}/Health2.png");
		CPH.ObsSetImageSourceFile(BOSS_FIGHT_SCENE, "Health1", $"{_newBoss.BossHealthBarDirectory}/Health1.png");
		
		// SHOW ADDITIONAL SOURCES
		ShowSource("BossSubmittedBy");
		ShowSource("BossSubmitName");
		ShowSource("ArtBy");
		ShowSource("ArtByName");
		
		ShowSource("Health");
		ShowSource("Health3");
		ShowSource("HealthName");
		ShowSource("HealthLeft");
		
		ShowSource("BossMusicPlate");
		ShowSource("BossMusicByName");
		ShowSource("BossMusicBy");
		ShowSource("BossMusicByPlate");
		ShowSource("AuraGif");
		
		// UPDATE CHANNEL POINT REWARD
		CPH.UnPauseReward("afbb5fdb-009a-43e8-89f2-c49d48a2ef07");
		CPH.UpdateRewardTitle("afbb5fdb-009a-43e8-89f2-c49d48a2ef07", "BOSS FIGHT - ATTACK");
		CPH.UpdateRewardPrompt("afbb5fdb-009a-43e8-89f2-c49d48a2ef07", "Help take down the current raid boss!! Attack!!");
		
		CPH.Wait(5000);
		var battleCancelled = CPH.GetGlobalVar<bool>("BattleCancelled");
		if(!battleCancelled)
		{
			// PAUSE/ENABLE TIMERS
			switch(_newBoss.BossAttackSpeed)
			{
				case 1:
					CPH.EnableTimer("00_BossAttackFAST");
					break;
				case 2:
					CPH.EnableTimer("00_BossAttackMID");
					break;
				case 3:
					CPH.EnableTimer("00_BossAttackSLOW");
					break;
				default:
					CPH.EnableTimer("00_BossAttackMID");
					break;
			}
		}
		
	}
	
	private void EndBoss()
	{
		CPH.SetGlobalVar("BattleCancelled", true);
		// Hide Boss IMG source and music
		HideSource("Boss00");
		HideSource("BossMusicSource");
		
		// Hide other boss components
		HideSource("Health1");
		HideSource("Health2");
		HideSource("Health3");
		HideSource("HealthName");
		HideSource("HealthLeft");
		HideSource("PartyPopper");
		HideSource("SpawnSound");
		HideSource("Congrats");
		HideSource("Victory");
		HideSource("AttackHit");
		HideSource("DmgMarkerText");
		HideSource("HitMarker");
		HideSource("BossSubmitName");
		HideSource("BossSubmittedBy");
		HideSource("AuraGif");
		HideSource("SpawnAura");
		HideSource("ArtByName");
		HideSource("ArtBy");
		HideSource("BossMusicByPlate");
		
		// UPDATE CHANNEL POINT REWARD
		CPH.UnPauseReward("afbb5fdb-009a-43e8-89f2-c49d48a2ef07");
		CPH.UpdateRewardTitle("afbb5fdb-009a-43e8-89f2-c49d48a2ef07", "BOSS FIGHT - No new boss");
		CPH.UpdateRewardPrompt("afbb5fdb-009a-43e8-89f2-c49d48a2ef07", $"There is no boss yet. Keep an eye out for the next raid boss and use this redeem to help take it down! **Cancelled last boss fight against: {_currentBoss.BossName}**");
		
		// PAUSE/ENABLE TIMERS
		CPH.DisableTimer("00_BossAttackFAST");
		CPH.DisableTimer("00_BossAttackMID");
		CPH.DisableTimer("00_BossAttackSLOW");
		
	}
	
		private void Defeat()
	{
		CPH.SetGlobalVar("BattleCancelled", true);
		// PAUSE/ENABLE TIMERS
		CPH.DisableTimer("00_BossAttackFAST");
		CPH.DisableTimer("00_BossAttackMID");
		CPH.DisableTimer("00_BossAttackSLOW");
		
		CPH.PauseReward("afbb5fdb-009a-43e8-89f2-c49d48a2ef07");
		CPH.UpdateRewardTitle("afbb5fdb-009a-43e8-89f2-c49d48a2ef07", "BOSS FIGHT - No new boss");
		CPH.UpdateRewardPrompt("afbb5fdb-009a-43e8-89f2-c49d48a2ef07", $"There is no boss yet. Keep an eye out for the next raid boss and use this redeem to help take it down! **Chat lost to the last boss: {_currentBoss.BossName}**");
		
		
		SetText("FinalBlow", "Better luck next time.");
		SetText("Congratulations", $"YOU LOST TO\n{_currentBoss.BossName}");
		
		// Hide Boss IMG source and music
		HideSource("Boss00");
		HideSource("BossMusicSource");
		
		HideSource("Health1");
		HideSource("Health2");
		HideSource("Health3");
		HideSource("HealthName");
		HideSource("HealthLeft");
		HideSource("AuraVid");
		HideSource("ArtByName");
		HideSource("ArtBy");
		HideSource("BossMusicByPlate");
		HideSource("BossSubmitName");
		HideSource("BossSubmittedBy");
		HideSource("AuraGif");
		
		ShowSource("Loss");
		ShowSource("Congrats");
		
		CPH.Wait(_currentBoss.BossDefeatSoundLength);
		
		HideSource("Loss");
		
		CPH.Wait(1500);
		
		HideSource("Congrats");
		
	}
	
		private void BossAttacksBack()
	{
		int randomChanceHits = rand.Next(10);
		int bossDamageDealt = _currentBoss.BossDamagePerSecond;
		// Will the attack hit, based on BossAccuracy...
		if(randomChanceHits < _currentBoss.BossAccuracy)
		{
			int randomChanceDoesExtraDmg = rand.Next(10);
			// 1/3 chance to do extra dmg
			if(randomChanceDoesExtraDmg < 3)
			{
				bossDamageDealt++;
			}
			_chatHealth -= bossDamageDealt;
			if(_chatHealth < 0)
			{
				_chatHealth = 0;
			}
			CPH.SetGlobalVar("ChatHealth", _chatHealth);
			SetText("ChatHealthLeft", $"{_chatHealth}/{_chatHealthStart}");
			SetText("ChatHealth", $"CHAT HEALTH");
			TriggerRandomHitVisual();
			ShowSource("DamagedBorder");
			ShowSource("ChatHealthGroup");
			CPH.Wait(1000);
			HideSource("DamagedBorder");
			HideSource("ChatHealthGroup");
			HideSource("BossAttacksSFX0");
			HideSource("BossAttackIndicator0");
			
			if(_chatHealth == 0)
			{
				CPH.RunAction("BossFight_Defeat");
			}
			
		}
		// Boss's attack missed
		else
		{
			SetText("ChatHealth", $"BOSS MISSED");
			ShowSource("ChatHealthGroup");
			CPH.Wait(1000);
			HideSource("ChatHealthGroup");
		}
	}
	
	private void TriggerRandomHitVisual()
	{
		string hitSource = string.Empty;
		string hitSoundSource = string.Empty;
		
		int selection = rand.Next(2);
		switch(selection)
		{
			case 0:
				hitSource = "E:/TRACKERS/Bosses/damaged01.png";
				hitSoundSource = "E:/TRACKERS/Bosses/SFX/Minecraft Leg Break Fall Damage Sound Effect.mp3";
				break;
			case 1:
				hitSource = "E:/TRACKERS/Bosses/damaged02.png";
				hitSoundSource = "E:/TRACKERS/Bosses/SFX/Minecraft Damage (Oof) - Sound Effect (HD).mp3";
				break;
			default:
				hitSource = "E:/TRACKERS/Bosses/damaged03.png";
				hitSoundSource = "E:/TRACKERS/Bosses/SFX/Roblox Death Sound - Sound Effect (HD).mp3";
				break;
		}
		
		// CHANGE MUSIC SOURCE
		CPH.ObsSetMediaSourceFile(BOSS_FIGHT_SCENE, "BossAttacksSFX0", hitSoundSource);
		ShowSource("BossAttacksSFX0");
		
		// CHANGE IMAGE SOURCE
		CPH.ObsSetImageSourceFile(BOSS_FIGHT_SCENE, "BossAttackIndicator0", hitSource);
		ShowSource("BossAttackIndicator0");
	}
	
		private void AttackHits()
	{
		_chatDamage = rand.Next(1,4);
		_currentBoss.UpdateBossHealth(_chatDamage);
		
		SetText("DmgMarkerText", _chatDamage.ToString());
		DisableFilter("HitMarker", "Missed");
		ShowSource("HitMarker");
		CPH.Wait(500);
		ShowSource("AttackHit");
		ShowSource("DmgMarkerText");
		CPH.Wait(500);
		HideSource("AttackHit");
		HideSource("DmgMarkerText");
		HideSource("HitMarker");
		SetText("HealthLeft", $"{_currentBoss.BossHealthLeft}/{_currentBoss.BossHealth}");
		
		// BOSS DEFEATED
		if(_currentBoss.BossHealthLeft <= 0)
		{
			HideSource("Health3");
			HideSource("Health2");
			HideSource("Health1");
			
			CPH.RunAction("BossFight_Victory");
		}
		
		// BOSS AT 1/3 HP
		else if(_currentBoss.BossHealthLeft <= _currentBoss.BossHealthTick1)
		{
			HideSource("Health3");
			HideSource("Health2");
			ShowSource("Health1");
		}
		
		// BOSS AT 2/3 HP
		else if(_currentBoss.BossHealthLeft <= _currentBoss.BossHealthTick2 && _currentBoss.BossHealthLeft > _currentBoss.BossHealthTick1)
		{
			HideSource("Health3");
			ShowSource("Health2");
		}
		
		var battleCancelled = CPH.GetGlobalVar<bool>("BattleCancelled");
		if(!battleCancelled)
		{
			CPH.UpdateRewardPrompt("afbb5fdb-009a-43e8-89f2-c49d48a2ef07", $"Help take down the current raid boss!! Attack!! {_user} dealt {_chatDamage} damage!");
		}
	}
	
	private void AttackMisses()
	{
		SetText("DmgMarkerText", "0");
		EnableFilter("HitMarker", "Missed");
		ShowSource("HitMarker");
		CPH.Wait(500);
		ShowSource("DmgMarkerText");
		CPH.Wait(500);
		HideSource("DmgMarkerText");
		HideSource("HitMarker");
		CPH.Wait(500);
		
		var battleCancelled = CPH.GetGlobalVar<bool>("BattleCancelled");
		if(!battleCancelled)
		{
			CPH.UpdateRewardPrompt("afbb5fdb-009a-43e8-89f2-c49d48a2ef07", $"Help take down the current raid boss!! Attack!! {_user} missed!");
		}
	}
	
		private void Victory()
	{
		CPH.SetGlobalVar("BattleCancelled", true);
		// PAUSE/ENABLE TIMERS
		CPH.DisableTimer("00_BossAttackFAST");
		CPH.DisableTimer("00_BossAttackMID");
		CPH.DisableTimer("00_BossAttackSLOW");
		
		CPH.PauseReward("afbb5fdb-009a-43e8-89f2-c49d48a2ef07");
		CPH.UpdateRewardTitle("afbb5fdb-009a-43e8-89f2-c49d48a2ef07", "BOSS FIGHT - No new boss");
		CPH.UpdateRewardPrompt("afbb5fdb-009a-43e8-89f2-c49d48a2ef07", $"There is no boss yet. Keep an eye out for the next raid boss and use this redeem to help take it down! **{_user}** defeated the last boss: {_currentBoss.BossName}");
		
		
		SetText("FinalBlow", $"{_user} Dealt the final blow!");
		SetText("Congratulations", $"CONGRATULATIONS YOU BEAT\n{_currentBoss.BossName}");
		ShowSource("Victory");
		ShowSource("Congrats");
		ShowSource("PartyPopper");
		HideSource("HealthName");
		HideSource("HealthLeft");
		HideSource("AuraGif");
		HideSource("ArtByName");
		HideSource("ArtBy");
		HideSource("BossMusicByPlate");
		HideSource("BossSubmitName");
		HideSource("BossSubmittedBy");
		
		// Hide Boss IMG source and music
		HideSource("Boss00");
		HideSource("BossMusicSource");
		
		CPH.Wait(_currentBoss.BossVictorySoundLength);
		HideSource("Victory");
		
		CPH.Wait(1500);
		
		HideSource("Congrats");
		HideSource("PartyPopper");
		
		
	}
	
	private void ShowSource(string source)
	{
		CPH.ObsSetSourceVisibility(BOSS_FIGHT_SCENE, source, true);
	}
	private void HideSource(string source)
	{
		CPH.ObsSetSourceVisibility(BOSS_FIGHT_SCENE, source, false);
	}
	private void SetText(string source, string text)
	{
		CPH.ObsSetGdiText(BOSS_FIGHT_SCENE, source, text);
	}
	private void EnableFilter(string source, string filterName)
	{
		CPH.ObsSetFilterState(BOSS_FIGHT_SCENE, source, filterName, 1);
	}
	private void DisableFilter(string source, string filterName)
	{
		CPH.ObsSetFilterState(BOSS_FIGHT_SCENE, source, filterName, 0);
	}
}

public class Boss
{
	public string BossFileLocation
	{
		get
		{
			return @"E:\TRACKERS\Bosses\Boss00-CURRENTBOSS.txt";
		}
	}
	public string BossName { get; set;}
	public string BossSubmittedByName { get; set; }
	public string BossArtByName { get; set; }
	public string BossMusicSongAndArtistName { get; set; }
	public string BossImageFileURL { get; set; }
	public string BossMusicFileURL { get; set; }
	public int BossHealth { get; set; }
	public int BossHealthLeft { get; set; }
	public int BossDamagePerSecond { get; set; }
	public int BossAccuracy { get; set; }
	public int BossAttackSpeed { get; set; }

	public string BossSpawnSound { get; set; }
	public int BossSpawnSoundLength { get; set; }

	public string BossVictorySound { get; set; }
	public int BossVictorySoundLength { get; set; }

	public string BossDefeatSound { get; set; }
	public int BossDefeatSoundLength { get; set; }
	
	public string BossHealthBarDirectory { get; set; }
	
	public virtual string BossFileTemp
	{
		get 
		{
			return @"E:\TRACKERS\Bosses\Boss01.txt";
		}
	}
	
	
	public Boss(IInlineInvokeProxy cph)
	{
		OverrideCurrentBoss(BossFileTemp);
		int lineNumber = 0;
		foreach(string line in System.IO.File.ReadLines(BossFileLocation))
		{
			switch(lineNumber)
			{
				case 0:
					BossName = line;
					break;
				case 1:
					BossSubmittedByName = line;
					break;
				case 2:
					BossArtByName = line;
					break;
				case 3:
					BossMusicSongAndArtistName = line;
					break;
				case 4:
					BossImageFileURL = line;
					break;
				case 5:
					BossMusicFileURL = line;
					break;
				case 6:
					Int32.TryParse(line, out int BossHP);
					BossHealth = BossHP;
					break;
				case 7:
					Int32.TryParse(line, out int BossHPLeft);
					BossHealthLeft = BossHPLeft;
					break;
				case 8:
					Int32.TryParse(line, out int BossDPS);
					BossDamagePerSecond = BossDPS;
					break;
				case 9:
					Int32.TryParse(line, out int BossAccuracyVar);
					BossAccuracy = BossAccuracyVar;
					break;
				case 10:
					Int32.TryParse(line, out int BossSpeedVar);
					BossAttackSpeed = BossSpeedVar;
					break;
				case 11:
					BossSpawnSound = line;
					break;
				case 12:
					Int32.TryParse(line, out int BossSpawnSoundLengthVar);
					BossSpawnSoundLength = BossSpawnSoundLengthVar;
					break;
				case 13:
					BossVictorySound = line;
					break;
				case 14:
					Int32.TryParse(line, out int BossVictorySoundLengthVar);
					BossVictorySoundLength = BossVictorySoundLengthVar;
					break;
				case 15:
					BossDefeatSound = line;
					break;
				case 16:
					Int32.TryParse(line, out int BossDefeatSoundLengthVar);
					BossDefeatSoundLength = BossDefeatSoundLengthVar;
					break;
				case 17:
					BossHealthBarDirectory = line;
					break;
			}
			lineNumber++;
		}
	}
	
	public void OverrideCurrentBoss(string newBossURL)
	{
		using (FileStream stream = File.OpenRead(newBossURL))
		using (FileStream writeStream = File.OpenWrite(BossFileLocation))
		{
			writeStream.SetLength(0);
			BinaryReader reader = new BinaryReader(stream);
			BinaryWriter writer = new BinaryWriter(writeStream);

			// create a buffer to hold the bytes 
			byte[] buffer = new Byte[1024];
			int bytesRead;

			// while the read method returns bytes
			// keep writing them to the output stream
			while ((bytesRead =
					stream.Read(buffer, 0, 1024)) > 0)
			{
				writeStream.Write(buffer, 0, bytesRead);
			}
		}
	}	
}

public class InsertNameBoss : Boss
{
	public override string BossFileTemp
	{
		get 
		{
			return @"E:\TRACKERS\Bosses\Boss01.txt";
		}
	}
	public InsertNameBoss(IInlineInvokeProxy cph) : base(cph)
	{
	}
}