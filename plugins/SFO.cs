using PS4UpdateTools.sys;
using System.Text;

// https://www.psdevwiki.com/ps4/Param.sfo

namespace PS4UpdateTools.plugins
{
    internal class SFO
    {
        public static void ViewPSOInformation(string inputFile)
        {
            using (FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                Logger.LogMsg($"[+] Magic: {new(reader.ReadChars(4))}");
                Logger.LogMsg($"[+] Version: {Utils.ToHex(reader.ReadInt32())}");
                Logger.LogMsg($"[+] Key-Table Start Offset: {Utils.ToHex(reader.ReadInt32())}");
                Logger.LogMsg($"[+] Data-Table Start Offset: {Utils.ToHex(reader.ReadInt32())}");

                int entries = reader.ReadInt32();
                Logger.LogMsg($"[+] Table Entries: {Utils.ToHex(entries)}");

                for (int i = 0; i < entries; i++)
                {
                    Console.WriteLine();

                    Logger.LogMsg($"[{i}] Key-Table Start Offset: {Utils.ToHex(reader.ReadInt16())}");
                    Logger.LogMsg($"[{i}] Fmt: {Utils.ToHex(reader.ReadInt16())}");
                    Logger.LogMsg($"[{i}] Length: {Utils.ToHex(reader.ReadInt32())}");
                    Logger.LogMsg($"[{i}] MaxLength: {Utils.ToHex(reader.ReadInt32())}");
                    Logger.LogMsg($"[{i}] DataOffset: {Utils.ToHex(reader.ReadInt32())}");
                }

                List<string> appInfo = new List<string>();
                StringBuilder sb = new StringBuilder();
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    byte b = reader.ReadByte();
                    if (b == 0x00)
                    {
                        appInfo.Add(sb.ToString());
                        if (sb.ToString() == "VERSION") break;
                        sb.Clear();

                    }
                    else
                    {
                        sb.Append((char)b);
                    }
                }
                sb.Clear();

                for (int i = 0; i < appInfo.Count; i++)
                {
                    switch(appInfo[i])
                    {
                        case "APP_VER":
                            Logger.LogMsg($"{appInfo[i]}: {new(reader.ReadChars(8))}");
                            break;

                        case "CATEGORY":
                            Logger.LogMsg($"{appInfo[i]}: {new(reader.ReadChars(4))}");
                            break;

                        case "CONTENT_ID":
                            Logger.LogMsg($"{appInfo[i]}: {new(reader.ReadChars(37))}");
                            break;

                        case "VERSION":
                        case "CONTENT_VER":
                            Logger.LogMsg($"{appInfo[i]}: {new(reader.ReadChars(8))}");
                            break;

                        case "FORMAT":
                            reader.ReadBytes(7);
                            Logger.LogMsg($"{appInfo[i]}: {new(reader.ReadChars(4))}");
                            break;

                        case "DISP_LOCATION_2":
                            Logger.LogMsg($"{appInfo[i]}: {Utils.ToHex(reader.ReadInt32())}");
                            reader.ReadBytes(3);
                            break;

                        case "TITLE":
                        case "TITLE_01":
                        case "TITLE_02":
                        case "TITLE_03":
                        case "TITLE_04":
                        case "TITLE_05":
                        case "TITLE_06":
                        case "TITLE_07":
                        case "TITLE_08":
                        case "TITLE_09":
                        case "TITLE_10":
                        case "TITLE_11":
                        case "TITLE_12":
                        case "TITLE_13":
                        case "TITLE_14":
                        case "TITLE_15":
                        case "TITLE_16":
                        case "TITLE_17":
                        case "TITLE_18":
                        case "TITLE_19":
                        case "TITLE_20":
                        case "TITLE_21":
                        case "TITLE_22":
                        case "TITLE_23":
                        case "TITLE_24":
                        case "TITLE_25":
                        case "TITLE_26":
                        case "TITLE_27":
                        case "TITLE_28":
                        case "TITLE_29":
                        case "TITLE_30":
                            Logger.LogMsg($"{appInfo[i]}: {new(reader.ReadChars(128))}");
                            break;

                        case "TITLE_ID":
                            Logger.LogMsg($"{appInfo[i]}: {new(reader.ReadChars(12))}");
                            break;

                        default:
                            Logger.LogMsg($"{appInfo[i]}: {Utils.ToHex(reader.ReadInt32())}");
                            break;
                    }
                }
            }
        }
    }
}