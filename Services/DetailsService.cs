
using SongList.Models.ViewModels;
using Details.Repository;

namespace Details.Service
{
    public class DetailsService
    {
        private readonly DetailsRepository _detailsRepository;
        public DetailsService(DetailsRepository detailsRepository)
        {
            _detailsRepository = detailsRepository;
        }
        public HomeDetailsViewModel CreateDetailsViewModel(long songId, string memberCode)
        {
            var viewmodel = new HomeDetailsViewModel();
            if (!string.IsNullOrEmpty(memberCode))
            {
                // メンバー名を取得してViewModelに設定する
                _detailsRepository.GetMemberName(viewmodel, memberCode);
                // 曲情報を取得してViewModelに設定する
                _detailsRepository.GetDetailSongList(viewmodel, songId);
                // 配信リストを取得してViewModelに設定する
                viewmodel.StreamListModel = _detailsRepository.GetStreamList(songId, memberCode);
                viewmodel.ListCount = viewmodel.StreamListModel.Count;
            }
            return viewmodel;
        }
    }
}