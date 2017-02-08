using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermaDeadstiny.BLL.Model.Character
{
    public class CharacterEndpoint
    {
        public CharacterResponse Response { get; set; }
        public int ErrorCode { get; set; }
        public int ThrottleSeconds { get; set; }
        public string ErrorStatus { get; set; }
        public string Message { get; set; }
    }

    public class CharacterResponse : Response
    {
        public Data data { get; set; }
    }

    public class Equipment
    {
        public long itemHash { get; set; }
        public List<object> dyes { get; set; }
    }

    public class PeerView
    {
        public List<Equipment> equipment { get; set; }
    }

    public class CharacterBase
    {
        public string membershipId { get; set; }
        public int membershipType { get; set; }
        public string characterId { get; set; }
    }

    public class LevelProgression
    {
        public int dailyProgress { get; set; }
        public int weeklyProgress { get; set; }
        public int currentProgress { get; set; }
        public int level { get; set; }
        public int step { get; set; }
        public int progressToNextLevel { get; set; }
        public int nextLevelAt { get; set; }
        public long progressionHash { get; set; }
    }

    public class Character
    {
        public CharacterBase characterBase { get; set; }
    }

    public class PrimaryStat
    {
        public long statHash { get; set; }
        public int value { get; set; }
        public int maximumValue { get; set; }
    }

    public class Item
    {
        public long itemHash { get; set; }
    }

    public class Currency
    {
        public long itemHash { get; set; }
        public int value { get; set; }
    }

    public class Inventory
    {
        public List<Item> items { get; set; }
        public List<Currency> currencies { get; set; }
    }

    public class Data
    {
        public string membershipId { get; set; }
        public int membershipType { get; set; }
        public List<Character> characters { get; set; }
    }
}

