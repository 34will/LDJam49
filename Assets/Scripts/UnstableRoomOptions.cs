
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using ExitGames.Client.Photon;
using Photon.Pun;

namespace Unstable
{
    public enum MapType
    {
        Map = 0,
        Random
    }

    public enum Gamemode
    {
        LastPersonStanding = 0,
        Tag,
        CaptureTheFlag,
        KingOfTheHill
    }

    [Serializable]
    public class UnstableRoomOptions
    {
        public static readonly string RoomOptionsKey = "m";

        private const string LastPersonStandingString = "L";
        private const string TagString = "T";
        private const string CaptureTheFlagString = "C";
        private const string KingOfTheHillString = "K";
        private const string YesString = "Y";
        private const string NoString = "N";
        private const string MapTypeString = "M";
        private const string RandomTypeString = "R";
        private const string TimesString = "X";
        private readonly static Regex optionsRegex = new Regex($"^([{LastPersonStandingString}{TagString}{CaptureTheFlagString}{KingOfTheHillString}])([{YesString}{NoString}])([{YesString}{NoString}])(\\d+){TimesString}(\\d+)([{MapTypeString}{RandomTypeString}])(.*)$");

        public MapType MapType;

        public int Width = 5;

        public int Height = 5;

        public string MapString = "S-S-S| | |S-D-S| | |S-S-S";

        public Gamemode Gamemode;

        public bool Debris;

        public bool Droppers;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            switch (Gamemode)
            {
                case Gamemode.LastPersonStanding:
                    builder.Append(LastPersonStandingString);
                    break;
                case Gamemode.Tag:
                    builder.Append(TagString);
                    break;
                case Gamemode.CaptureTheFlag:
                    builder.Append(CaptureTheFlagString);
                    break;
                case Gamemode.KingOfTheHill:
                    builder.Append(KingOfTheHillString);
                    break;
                default:
                    builder.Append("@");
                    break;
            }
            builder.Append(Debris ? YesString : NoString);
            builder.Append(Droppers ? YesString : NoString);
            builder.Append(Width);
            builder.Append(TimesString);
            builder.Append(Height);
            switch (MapType)
            {
                case MapType.Map:
                    builder.Append(MapTypeString);
                    break;
                case MapType.Random:
                    builder.Append(RandomTypeString);
                    break;
                default:
                    builder.Append("@");
                    break;
            }
            builder.Append(MapString);
            return builder.ToString();
        }

        public static UnstableRoomOptions Parse(string options)
        {
            if (string.IsNullOrWhiteSpace(options))
                return null;

            options = options.ToUpper();

            Match match = optionsRegex.Match(options);
            if (!match.Success)
                return null;

            UnstableRoomOptions result = new UnstableRoomOptions();

            switch (match.Groups[1].Value)
            {
                case LastPersonStandingString:
                    result.Gamemode = Gamemode.LastPersonStanding;
                    break;
                case TagString:
                    result.Gamemode = Gamemode.Tag;
                    break;
                case CaptureTheFlagString:
                    result.Gamemode = Gamemode.CaptureTheFlag;
                    break;
                case KingOfTheHillString:
                    result.Gamemode = Gamemode.KingOfTheHill;
                    break;
                default:
                    return null;
            }

            switch (match.Groups[2].Value)
            {
                case YesString:
                    result.Debris = true;
                    break;
                case NoString:
                    result.Debris = false;
                    break;
                default:
                    return null;
            }

            switch (match.Groups[3].Value)
            {
                case YesString:
                    result.Droppers = true;
                    break;
                case NoString:
                    result.Droppers = false;
                    break;
                default:
                    return null;
            }

            string widthString = match.Groups[4].Value;
            if (!int.TryParse(widthString, out int width))
                return null;
            result.Width = width;

            string heightString = match.Groups[5].Value;
            if (!int.TryParse(heightString, out int height))
                return null;
            result.Height = height;

            switch (match.Groups[6].Value)
            {
                case MapTypeString:
                    string mapTilesString = match.Groups[7].Value;
                    int size = width * height;
                    if (mapTilesString.Length != size)
                        return null;
                    result.MapString = mapTilesString;
                    break;
                case RandomTypeString:
                    break;
                default:
                    return null;
            }

            return result;
        }

        public static byte[] Serialize(object toSerialize)
        {
            if (!(toSerialize is UnstableRoomOptions unstableRoomOptions))
                return new byte[0];

            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, unstableRoomOptions);
                return stream.ToArray();
            }
        }

        public static object Deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length <= 0)
                return new byte[0];

            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(bytes))
                return formatter.Deserialize(stream) as UnstableRoomOptions;
        }

        public static UnstableRoomOptions Current
        {
            get => PhotonNetwork.CurrentRoom.CustomProperties[RoomOptionsKey] as UnstableRoomOptions;
            set => PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { RoomOptionsKey, value } });
        }
    }
}
