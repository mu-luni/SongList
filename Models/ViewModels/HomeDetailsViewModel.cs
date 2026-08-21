using System.ComponentModel;

namespace SongList.Models.ViewModels
{
    public class HomeDetailsViewModel
    {
        public string DtlMemberName { get; set; } = string.Empty;
        public string DtlSongName { get; set; } = string.Empty;
        public string DtlArtistName { get; set; } = string.Empty;
        public string DtlGenreCode { get; set; } = string.Empty;
        public string DtlGenreName { get; set; } = string.Empty;
        public string DtlTieupName { get; set; } = string.Empty;
        public long ListCount { get; set; }
        public List<StreamListModel> StreamListModel { get; set; } = new List<StreamListModel>();
    }

    public class StreamListModel
    {
        [DisplayName("配信URL")]
        public string StreamUrl { get; set; } = string.Empty;
        [DisplayName("配信日")]
        public string ReleaseDate { get; set; } = string.Empty;
        [DisplayName("配信名")]
        public string StreamTitle { get; set; } = string.Empty;
        [DisplayName("備考")]
        public string SetlistNotes { get; set; } = string.Empty;
        [DisplayName("限定コード")]
        public string LimitedCode { get; set; } = string.Empty;
        [DisplayName("限定")]
        public string LimitedName { get; set; } = string.Empty;
    }
}