using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class DateManager: StuffObject<DateManager>
{
	private int timezone = 8;

	[ShowInInspector]
	public int Timezone
	{
		get
		{
			return this.timezone;
		}
		set
		{
			this.timezone = value;
			date = null;
			lastDate = null;
		}
	}

	private long timestampMs;

	[ShowInInspector]
	public long TimestampMs
	{
		get
		{
			return this.timestampMs;
		}
		set
		{
			this.timestampMs = value;
			date = null;
			Debug.Log("[DateManager] timestamp set to: " + value);
		}
	}

	public long TimestampSec
	{
		get
		{
			var ms = this.timestampMs;
			var sec = ms / 1000;
			return sec;
		}
	}
	
	public event Action HourChanged;

	/// <summary>
    /// 服务器时区下现在的日期
    /// </summary>
	public DateTime Now
	{
		get
		{
			var zeroTimestampDate = new DateTime(1970, 1, 1, 0, 0, 0);
			var offset = TimeSpan.FromHours(timezone);
			zeroTimestampDate += offset;
			//var zeroTimestampDate = new DateTime(1970, 1, 1, 8, 0, 0);
			var now = zeroTimestampDate.AddMilliseconds(timestampMs);
			return now;
		}
	}

	[ShowInInspector]
	public void EditorAdd1Hour()
    {
		var ms = 60 * 60 * 1000;
		this.timestampMs += ms;
    }

	[ShowInInspector]
	public void EditorAdd1Day()
	{
		var ms = 24 * 60 * 60 * 1000;
		this.timestampMs += ms;
	}

	DateTime? lastDate;
	DateTime? date;
	void Update()
	{
		var delta = Time.deltaTime;
		timestampMs += (long)(delta * 1000);

		if(date == null)
		{
			date = this.Now;
		}
		else
		{
			date = date.Value.AddSeconds(delta);
		}

		if(HourChanged != null)
		{
			if(lastDate != null)
			{
				var lastMonth = lastDate.Value.Month;
				var lastDay = lastDate.Value.Day;
				var lastHour = lastDate.Value.Hour;
				
				var nowMonth = date.Value.Month;
				var nowDay = date.Value.Day;
				var nowHour = date.Value.Hour;
				if(lastMonth != nowMonth || lastDay != nowDay || lastHour != nowHour)
				{
					HourChanged?.Invoke();
				}
			}
		}


		lastDate = date;
	}

	public DateTime TimestampToDateTime(long timestampMs)
	{
		var ret = DateUtil.TimestampToDateTime(timestampMs, this.timezone);
		return ret;
	}


	public bool IsNowBetween(DateTime start, DateTime end)
	{
		var nowDate = this.Now; 
		if (nowDate >= start && nowDate < end)
		{
			return true;
		}
		return false;
	}
}

