using Steam.WebApi.AuthenticateTicket;
using Steam.WebApi.UserInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SteamAuthAPI
{
    public static class SteamAuthTicket
    {

        private static string GetSteamAuthTicket(byte[] e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in e)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        private static byte[] StringToByteArray(String hex)
        {
            return Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }

        public static AuthTicket GetInfos(byte[] ticket)
        {
            MemoryStream stream = new MemoryStream(ticket);
            BinaryReader binaryReader = new BinaryReader(stream);
            HttpClient client = new HttpClient();
            int initiallenght = binaryReader.ReadInt32();
            if (initiallenght == 20)
            {
                AuthTicket authTicket = new AuthTicket();
                authTicket.gctoken = binaryReader.ReadUInt64().ToString();
                binaryReader.ReadBytes(8);
                double tokengeneratednotday = (double)binaryReader.ReadUInt32() * 1000;
                authTicket.tokenGenerated = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(tokengeneratednotday).ToLocalTime();
                if (binaryReader.ReadUInt32() != 24)
                {
                    return new AuthTicket();
                }
                binaryReader.ReadBytes(8);
                uint p = binaryReader.ReadUInt32();
                binaryReader.ReadBytes(4);
                authTicket.clientConnectionTime = binaryReader.ReadUInt32();
                authTicket.clientConnectionCount = (int)binaryReader.ReadUInt32();
                long ownershipTicketOffset = binaryReader.BaseStream.Position;
                uint ownershipTicketLength = binaryReader.ReadUInt32();

                binaryReader.ReadBytes(4);
                authTicket.version = (int)binaryReader.ReadUInt32();
                authTicket.AccountID = (int)binaryReader.ReadUInt32();
                binaryReader.ReadBytes(4);
                authTicket.appID = (int)binaryReader.ReadUInt32();
                uint ownershipTicketExternalIP = binaryReader.ReadUInt32();
                uint ownershipTicketInternalIP = binaryReader.ReadUInt32();
                uint ownershipFlags = binaryReader.ReadUInt32();
                double ownershipTicketGeneratednotday = (double)binaryReader.ReadUInt32() * 1000;
                authTicket.ownershipTicketGenerated = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ownershipTicketGeneratednotday).ToLocalTime();
                double ownershipTicketExpiresnotday = (double)binaryReader.ReadUInt32() * 1000;
                authTicket.ownershipTicketExpires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ownershipTicketExpiresnotday).ToLocalTime();

                ushort licenseCount = binaryReader.ReadUInt16();
                int[] license = new int[licenseCount];
                for (int i = 0; i < licenseCount; i++)
                {
                    license[i] = (int)binaryReader.ReadUInt32();
                }
                authTicket.licences = license;
                ushort dlcCount = binaryReader.ReadUInt16();
                AuthTicketDlc[] authTicketDlcs = new AuthTicketDlc[dlcCount];
                for (int i = 0; i < dlcCount; i++)
                {
                    authTicketDlcs[i].appID = (int)binaryReader.ReadUInt32();
                    licenseCount = binaryReader.ReadUInt16();
                    int[] dlclicenses = new int[licenseCount];
                    for (int h = 0; h < licenseCount; h++)
                    {
                        dlclicenses[h] = (int)binaryReader.ReadUInt32();
                    }
                    authTicketDlcs[i].licenses = dlclicenses;
                }
                authTicket.dlcs = authTicketDlcs;
                binaryReader.ReadBytes(2);
                if (binaryReader.BaseStream.Position + 128 == binaryReader.BaseStream.Length)
                {
                    authTicket.signature = binaryReader.ReadBytes((int)(binaryReader.BaseStream.Position + 128));
                }

                String Ticketresponse = client.GetAsync("https://api.steampowered.com/ISteamUserAuth/AuthenticateUserTicket/v1/?key=BB147E811A8CEA5DFEFEA578D7105230&appid=700330&ticket=" + GetSteamAuthTicket(ticket)).Result.Content.ReadAsStringAsync().Result;
                AuthenticateTicket ticketjson = AuthenticateTicket.FromJson(Ticketresponse);
                bool hasValidSignature = true;
                if (ticketjson.Response.Params != null)
                {
                    hasValidSignature = ticketjson.Response.Params.Result == "OK";
                    authTicket.SteamID64 = long.Parse(ticketjson.Response.Params.Ownersteamid);
                }
                else
                {
                    return new AuthTicket();
                }

                String userInfoResponse = client.GetAsync("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=BB147E811A8CEA5DFEFEA578D7105230&steamids=" + ticketjson.Response.Params.Ownersteamid).Result.Content.ReadAsStringAsync().Result;
                UserInfo info = UserInfo.FromJson(userInfoResponse);
                authTicket.nickname = info.Response.Players[0].Personaname;
                authTicket.isExpired = authTicket.ownershipTicketExpires < DateTime.Now;
                authTicket.isValid = !authTicket.isExpired & hasValidSignature;



                return authTicket;
            }
            return new AuthTicket();
        }

        public static AuthTicket GetInfos(string hexTicket)
        {
            byte[] ticket = StringToByteArray(hexTicket);
            MemoryStream stream = new MemoryStream(ticket);
            BinaryReader binaryReader = new BinaryReader(stream);
            HttpClient client = new HttpClient();
            int initiallenght = binaryReader.ReadInt32();
            if (initiallenght == 20)
            {
                AuthTicket authTicket = new AuthTicket();
                authTicket.gctoken = binaryReader.ReadUInt64().ToString();
                binaryReader.ReadBytes(8);
                double tokengeneratednotday = (double)binaryReader.ReadUInt32() * 1000;
                authTicket.tokenGenerated = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(tokengeneratednotday).ToLocalTime();
                if (binaryReader.ReadUInt32() != 24)
                {
                    return new AuthTicket();
                }
                binaryReader.ReadBytes(8);
                uint p = binaryReader.ReadUInt32();
                binaryReader.ReadBytes(4);
                authTicket.clientConnectionTime = binaryReader.ReadUInt32();
                authTicket.clientConnectionCount = (int)binaryReader.ReadUInt32();
                long ownershipTicketOffset = binaryReader.BaseStream.Position;
                uint ownershipTicketLength = binaryReader.ReadUInt32();

                binaryReader.ReadBytes(4);
                authTicket.version = (int)binaryReader.ReadUInt32();
                authTicket.AccountID = (int)binaryReader.ReadUInt32();
                binaryReader.ReadBytes(4);
                authTicket.appID = (int)binaryReader.ReadUInt32();
                uint ownershipTicketExternalIP = binaryReader.ReadUInt32();
                uint ownershipTicketInternalIP = binaryReader.ReadUInt32();
                uint ownershipFlags = binaryReader.ReadUInt32();
                double ownershipTicketGeneratednotday = (double)binaryReader.ReadUInt32() * 1000;
                authTicket.ownershipTicketGenerated = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ownershipTicketGeneratednotday).ToLocalTime();
                double ownershipTicketExpiresnotday = (double)binaryReader.ReadUInt32() * 1000;
                authTicket.ownershipTicketExpires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ownershipTicketExpiresnotday).ToLocalTime();

                ushort licenseCount = binaryReader.ReadUInt16();
                int[] license = new int[licenseCount];
                for (int i = 0; i < licenseCount; i++)
                {
                    license[i] = (int)binaryReader.ReadUInt32();
                }
                authTicket.licences = license;
                ushort dlcCount = binaryReader.ReadUInt16();
                AuthTicketDlc[] authTicketDlcs = new AuthTicketDlc[dlcCount];
                for (int i = 0; i < dlcCount; i++)
                {
                    authTicketDlcs[i].appID = (int)binaryReader.ReadUInt32();
                    licenseCount = binaryReader.ReadUInt16();
                    int[] dlclicenses = new int[licenseCount];
                    for (int h = 0; h < licenseCount; h++)
                    {
                        dlclicenses[h] = (int)binaryReader.ReadUInt32();
                    }
                    authTicketDlcs[i].licenses = dlclicenses;
                }
                authTicket.dlcs = authTicketDlcs;
                binaryReader.ReadBytes(2);
                if (binaryReader.BaseStream.Position + 128 == binaryReader.BaseStream.Length)
                {
                    authTicket.signature = binaryReader.ReadBytes((int)(binaryReader.BaseStream.Position + 128));
                }

                String Ticketresponse = client.GetAsync("https://api.steampowered.com/ISteamUserAuth/AuthenticateUserTicket/v1/?key=BB147E811A8CEA5DFEFEA578D7105230&appid=700330&ticket=" + hexTicket).Result.Content.ReadAsStringAsync().Result;
                AuthenticateTicket ticketjson = AuthenticateTicket.FromJson(Ticketresponse);
                bool hasValidSignature = true;
                if (ticketjson.Response.Params != null)
                {
                    hasValidSignature = ticketjson.Response.Params.Result == "OK";
                    authTicket.SteamID64 = long.Parse(ticketjson.Response.Params.Ownersteamid);
                }
                else
                {
                    return new AuthTicket();
                }

                String userInfoResponse = client.GetAsync("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=BB147E811A8CEA5DFEFEA578D7105230&steamids=" + ticketjson.Response.Params.Ownersteamid).Result.Content.ReadAsStringAsync().Result;
                UserInfo info = UserInfo.FromJson(userInfoResponse);
                authTicket.nickname = info.Response.Players[0].Personaname;
                authTicket.isExpired = authTicket.ownershipTicketExpires < DateTime.Now;
                authTicket.isValid = !authTicket.isExpired & hasValidSignature;



                return authTicket;
            }
            return new AuthTicket();
        }
    }

    public struct AuthTicket
    {
        public string gctoken;
        public DateTime tokenGenerated;
        public long clientConnectionTime;
        public int clientConnectionCount;
        public int version;
        public Int64 AccountID;
        public Int64 SteamID64;
        public string nickname;
        public int appID;
        public DateTime ownershipTicketGenerated;
        public DateTime ownershipTicketExpires;
        public int[] licences;
        public AuthTicketDlc[] dlcs;
        public byte[] signature;
        public bool isExpired;
        public bool isValid;
        public AuthTicket(String gctoken, DateTime tokenGenerated, long clientConnectionTime, int clientConnectionCount, int version, int AccountID, int SteamID64, int AppID, DateTime ownershipTicketGenerated, DateTime ownershipTicketExpires, int[] licences, AuthTicketDlc[] dlcs, byte[] signature, bool isExpired, bool isValid, string nickname)
        {
            this.gctoken = gctoken;
            this.tokenGenerated = tokenGenerated;
            this.clientConnectionTime = clientConnectionTime;
            this.clientConnectionCount = clientConnectionCount;
            this.version = version;
            this.AccountID = AccountID;
            this.SteamID64 = SteamID64;
            this.appID = AppID;
            this.ownershipTicketGenerated = ownershipTicketGenerated;
            this.ownershipTicketExpires = ownershipTicketExpires;
            this.licences = licences;
            this.dlcs = dlcs;
            this.signature = signature;
            this.isExpired = isExpired;
            this.isValid = isValid;
            this.nickname = nickname;
        }


        public override string ToString()
        {
            String str = "GcToken "+gctoken + "TokenGenerated "+tokenGenerated + "clientConnectionTime "+clientConnectionTime + " ClientConnectionCount "+clientConnectionCount+" Version "+version+" Account ID "+AccountID+" SteamID64 "+SteamID64+" AppID "+appID+" OwnerShipTicketGenerated "+ownershipTicketGenerated+" ownershipTicketExpires "+ownershipTicketExpires+" isExpired "+isExpired+" isValid "+isValid+" Nickname "+nickname;
            return str;
        }
    }
    public struct AuthTicketDlc
    {
        public int appID;
        public int[] licenses;

        public AuthTicketDlc(int appid, int[] licenses)
        {
            this.appID = appid;
            this.licenses = licenses;
        }
    }
}
