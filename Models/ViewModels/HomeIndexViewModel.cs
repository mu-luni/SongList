using System.ComponentModel;

namespace SongList.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public GroupListModel groupList { get; set; } = new();
        public List<MemberListModel> memberList { get; set; } = new List<MemberListModel>();
        public List<GenreListModel> genreList { get; set; } = new List<GenreListModel>();
        public SearchCondition searchCondition { get; set; } = new();
        public long ListCount { get; set; }
        public List<SongListModel> songList { get; set; } = new List<SongListModel>();
    }
    public class GroupListModel
    {
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    public class MemberListModel
    {
        public string MemberCode { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
    }
    public class GenreListModel
    {
        public string GenreCode { get; set; } = string.Empty;
        public string GenreName { get; set; } = string.Empty;
    }

    public class SearchCondition
    {
        public string ArtistName { get; set; } = string.Empty;
        public string SongName { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Tieup { get; set; } = string.Empty;
        public string Member { get; set; } = "01";
    }
    public class SongListModel
    {
        [DisplayName("曲ID")]
        public long SongId { get; set; }
        [DisplayName("メンバーコード")]
        public string MemberCode { get; set; } = string.Empty;
        [DisplayName("曲名")]
        public string SongName { get; set; } = string.Empty;
        [DisplayName("アーティスト名")]
        public string ArtistName { get; set; } = string.Empty;
        [DisplayName("最終歌唱日")]
        public string LastSungDate { get; set; } = string.Empty;
        [DisplayName("歌唱回数")]
        public int SungCount { get; set; }
    }
}