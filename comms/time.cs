using System;
using System.IO;
using ProtoBuf;

namespace StandAloneMapView.comms
{
	[ProtoContract]
	public class Time
	{
		public const float updateInterval = 0.05f;

		[ProtoMember(1)]
		public double UniversalTime {get;set;}

		[ProtoMember(2)]
		public float TimeWarp {get;set;}

		public static byte[] MakePacket(double UniversalTime, float TimeWarp)
		{
			var time = new Time() { UniversalTime=UniversalTime, TimeWarp=TimeWarp };

			using(var stream = new MemoryStream())
			{
				Serializer.Serialize<Time>(stream, time);
				return stream.ToArray();
			}
		}

		public static Time ReadPacket(byte[] packet)
		{
			using(var stream = new MemoryStream(packet))
			{
				return Serializer.Deserialize<Time>(stream);
			}
		}
	}
}