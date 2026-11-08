namespace Moba.Core
{
    public enum Team
    {
        None = 0,
        Blue = 1,
        Red = 2
    }

    public static class TeamExtensions
    {
        public static Team Opponent(this Team team)
        {
            return team switch
            {
                Team.Blue => Team.Red,
                Team.Red => Team.Blue,
                _ => Team.None
            };
        }
    }
}
