/*

Copyright 2014 Daniel Kinsman.

This file is part of Stand Alone Map View.

Stand Alone Map View is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Stand Alone Map View is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Stand Alone Map View.  If not, see <http://www.gnu.org/licenses/>.

*/

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