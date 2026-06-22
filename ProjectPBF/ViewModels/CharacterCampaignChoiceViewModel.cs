namespace ProjectPBF.ViewModels
{
    public class CharacterCampaignChoiceViewModel
    {
        public List<CharacterCampaignChoiceItemViewModel> Campaigns { get; set; } = new();
    }

    public class CharacterCampaignChoiceItemViewModel
    {
        public int CampaignId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GameMasterName { get; set; } = string.Empty;
        public int StartingPoints { get; set; }
        public int StatisticPointsPerLevel { get; set; }
        public string StatisticsSummary { get; set; } = string.Empty;
        public bool IsMember { get; set; }
        public bool IsOwner { get; set; }
        public bool HasCharacterInCampaign { get; set; }
    }
}
