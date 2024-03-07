using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

public class CPHInline
{
	private bool? _isRecording;
	private bool IsRecording 
	{ 
		get 
		{
			if(_isRecording == null)
			{
				_isRecording = bool.TryParse(args.TryGetValue("obs.isRecording", out object isRecording) ? isRecording?.ToString() : string.Empty, out bool result) ? result : false;
			}
			return (bool)_isRecording;
		}
	}
	
	private bool? _isStreaming;
	private bool IsStreaming 
	{ 
		get 
		{ 
			if(_isStreaming == null)
			{
				_isStreaming = bool.TryParse(args.TryGetValue("obs.isStreaming", out object isStreaming) ? isStreaming?.ToString() : string.Empty, out bool result) ? result : false;
			}
			return (bool)_isStreaming;
		}
	}
	
	private string StreamdeckButtonID {
		get 
		{
			return CPH.GetGlobalVar<string>("Streamdeck-ButtonID", false) ?? string.Empty;
		}
    }
	
	
	public bool Execute()
	{
		if(!string.IsNullOrWhiteSpace(StreamdeckButtonID))
		{
			GeneralStats stats = JsonConvert.DeserializeObject<GeneralStats>(CPH.ObsSendRaw("GetStats", "{}", 0));
			int fps = Convert.ToInt32(Math.Round(stats.ActiveFps));
			if(IsStreaming)
			{
				StreamStatus streamStatus = JsonConvert.DeserializeObject<StreamStatus>(CPH.ObsSendRaw("GetStreamStatus", "{}", 0));
				CPH.StreamDeckSetTitle(StreamdeckButtonID, $"Live{Environment.NewLine}{streamStatus.OutputTimecode}");
				CPH.StreamDeckSetValue(StreamdeckButtonID, $"{fps} fps");
				if(fps < 30)
				{
					CPH.StreamDeckSetBackgroundColor(StreamdeckButtonID, "#c63030");
				}
				else if(fps >= 30 && fps <= 45)
				{
					CPH.StreamDeckSetBackgroundColor(StreamdeckButtonID, "#dddd00");
				}
				else if(fps > 45)
				{
					CPH.StreamDeckSetBackgroundColor(StreamdeckButtonID, "#00aa00");
				}
				
				
			}
			else if(IsRecording)
			{
				RecordStatus recordStatus = JsonConvert.DeserializeObject<RecordStatus>(CPH.ObsSendRaw("GetRecordStatus", "{}", 0));
				if(recordStatus.OutputPaused)
				{
					CPH.StreamDeckSetTitle(StreamdeckButtonID, $"Recording{Environment.NewLine}{recordStatus.OutputTimecode}");
					CPH.StreamDeckSetValue(StreamdeckButtonID, $"PAUSED");
					CPH.StreamDeckSetBackgroundColor(StreamdeckButtonID, "#c63030");
				}
				else
				{
					CPH.StreamDeckSetTitle(StreamdeckButtonID, $"Recording{Environment.NewLine}{recordStatus.OutputTimecode}");
					CPH.StreamDeckSetValue(StreamdeckButtonID, $"{fps} fps");
					CPH.StreamDeckSetBackgroundColor(StreamdeckButtonID, "#00aa00");
				}
			}
			else
			{
				CPH.StreamDeckSetTitle(StreamdeckButtonID, "Offline");
				CPH.StreamDeckSetValue(StreamdeckButtonID, string.Empty);
				CPH.StreamDeckSetBackgroundColor(StreamdeckButtonID, "#9c9c9c");
			}
		}
		else
		{
			CPH.LogInfo("Streamdeck button ID is empty, you need to specify a Streamdeck button ID for this action to work.");
		}
		return true;
	}
}


public class GeneralStats
{
	public double ActiveFps {get; set;}
	public double AvailableDiskSpace {get; set;}
	public double AverageFrameRenderTime {get; set;}
	public double CpuUsage {get; set;}
	public double MemoryUsage {get; set;}
	public int OutputSkippedFrames {get; set;}
	public int OutputTotalFrames {get; set;}
	public int RenderSkippedFrames {get; set;}
	public int RenderTotalFrames {get; set;}
	public int WebSocketSessionIncomingMessages {get; set;}
	public int WebSocketSessionOutgoingMessages {get; set;}
}

public class StreamStatus
{
	public bool OutputActive {get; set;}
	public int OutputBytes {get; set;}
	public double OutputCongestion {get; set;}
	public int OutputDuration {get; set;}
	public bool OutputReconnecting {get; set;}
	public int OutputSkippedFrames {get; set;}
	public string OutputTimecode {get; set;}
	public int OutputTotalFrames {get; set;}
}

public class RecordStatus
{
	public bool OutputActive {get; set;}
	public int OutputBytes {get; set;}
	public int OutputDuration {get; set;}
	public bool OutputPaused {get; set;}
	public string OutputTimecode {get; set;}
}
