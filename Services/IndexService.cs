
using SongList.Models.ViewModels;
using Index.Repository;

namespace Index.Service
{
    public class IndexService
    {
        private readonly IndexRepository _indexRepository;
        public IndexService(IndexRepository indexRepository)
        {
            _indexRepository = indexRepository;
        }
        public HomeIndexViewModel CreateIndexViewModel(SearchCondition condition, bool search)
        {
            var viewmodel = new HomeIndexViewModel
            {
                searchCondition = condition,
                // グループ情報を取得してViewModelに設定する
                groupList = _indexRepository.GetGroupList(),
                // メンバーリストを取得してViewModelに設定する
                memberList = _indexRepository.GetMemberList(),
                // ジャンルリストを取得してViewModelに設定する
                genreList = _indexRepository.GetGenreList()
            };
            if (search)
            {
                viewmodel.songList = _indexRepository.GetSongList(condition);
                viewmodel.ListCount = viewmodel.songList.Count;
            }
            return viewmodel;
        }
    }
}