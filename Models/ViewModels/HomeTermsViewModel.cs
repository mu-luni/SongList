using System.ComponentModel;

namespace SongList.Models.ViewModels
{
    public class HomeTermsViewModel
    {
        public string pageTitle { get; set; } = string.Empty;
        public List<InformationModel> informationModel { get; set; } = new List<InformationModel>();
    }
    public class InformationModel
    {
        public string title { get; set; } = string.Empty;
        public string contents { get; set; } = string.Empty;
    }
}