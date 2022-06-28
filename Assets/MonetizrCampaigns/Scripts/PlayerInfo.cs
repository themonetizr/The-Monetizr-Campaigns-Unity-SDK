namespace Monetizr.Campaigns
{
    internal class PlayerInfo
    {
        //public string location { get; }
        //public int age { get; }
        //public string gameType { get; }
        public string playerId { get; }

        public PlayerInfo(/*string location, int age, string gameType,*/ string playerId)
        {
            //this.location = location;
            //this.age = age;
            //this.gameType = gameType;
            this.playerId = playerId;
        }
    }
}