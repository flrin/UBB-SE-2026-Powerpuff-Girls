namespace Tests_and_Interviews.ViewModels
{
    public class LeaderboardEntryViewModel
    {
        public int RankPosition { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}