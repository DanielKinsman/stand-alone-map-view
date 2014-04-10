using System;
using UnityEngine;

namespace StandAloneMapView.utils
{
	public abstract class MonoBehaviourExtended : MonoBehaviour
	{
		public string LogPrefix { get; set; }

		public virtual void Awake(){}
		public virtual void Start(){}
		public virtual void OnDestroy(){}

		public string LogFormat(string message, params object[] formatParams)
		{
			message = string.Format(message, formatParams);
			if(LogPrefix != null && LogPrefix.Length > 0)
				message =  LogPrefix + " " + message;

			return message;
		}

		public void Log(string message, params object[] formatParams)
		{
			UnityEngine.Debug.Log(LogFormat(message, formatParams));
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public void LogDebug(string message, params object[] formatParams)
		{
			UnityEngine.Debug.Log(LogFormat(message, formatParams));
		}

		public void LogWarning(string message, params object[] formatParams)
		{
			UnityEngine.Debug.LogWarning(LogFormat(message, formatParams));
		}

		public void LogException(Exception e)
		{
			LogWarning("exception incoming, trace {0}", e.StackTrace);
			UnityEngine.Debug.LogException(e);
		}
	}
}