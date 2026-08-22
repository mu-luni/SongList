using SongList.Models.ViewModels;
using System.Text;
using Npgsql;

namespace Terms.Repository
{
    public class TermsRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        public TermsRepository(NpgsqlDataSource dataSource)
        {
                // データソースを取得
                _dataSource = dataSource;
        }

        public void GetInformation(HomeTermsViewModel viewmodel, string kind)
        {
            using var conn = _dataSource.OpenConnection();
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select info.page_title ");
                sb.AppendLine("     , info.title ");
                sb.AppendLine("     , info.contents ");
                sb.AppendLine("  from information info ");
                sb.AppendLine(" where info.kind = @kind ");
                sb.AppendLine(" order by dtl_no ");

                using (var cmd = new NpgsqlCommand(sb.ToString(), conn))
                {
                    cmd.Parameters.Add("@kind", NpgsqlTypes.NpgsqlDbType.Varchar, 20).Value = kind;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            viewmodel.pageTitle = reader.GetString(0);
                            do
                            {
                                viewmodel.informationModel.Add(new InformationModel
                                {
                                    title = reader.GetString(1),
                                    contents = reader.GetString(2)
                                });
                            } while (reader.Read());
                        }
                    }
                }
                conn.Close();
            }
        }
    }    
}