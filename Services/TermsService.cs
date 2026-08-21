
using SongList.Models.ViewModels;
using Terms.Repository;

namespace Terms.Service
{
    public class TermsService
    {
        private readonly TermsRepository _termsRepository;
        public TermsService(TermsRepository termsRepository)
        {
            _termsRepository = termsRepository;
        }
        public HomeTermsViewModel CreateTermsViewModel(string kind)
        {
            var viewmodel = new HomeTermsViewModel();
            // お知らせ情報を取得
            _termsRepository.GetInformation(viewmodel, kind);
            return viewmodel;
        }
    }
}