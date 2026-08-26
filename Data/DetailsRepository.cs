using SongList.Models.ViewModels;
using System.Text;
using Npgsql;

namespace Details.Repository
{
    public class DetailsRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        public DetailsRepository(NpgsqlDataSource dataSource)
        {
                // データソースを取得
                _dataSource = dataSource;
        }
        public void GetMemberName(HomeDetailsViewModel viewmodel, string memberCode)
        {
            using var conn = _dataSource.OpenConnection();
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select member_name ");
                sb.AppendLine("  from member_list ");
                sb.AppendLine(" where member_code = @memberCode ");

                using (var cmd = new NpgsqlCommand(sb.ToString(), conn))
                {
                    cmd.Parameters.Add("@memberCode", NpgsqlTypes.NpgsqlDbType.Varchar, 2).Value = memberCode;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            viewmodel.DtlMemberName = reader.GetString(0);
                        }
                    }
                }
                conn.Close();
            }
        }
        public void GetDetailSongList(HomeDetailsViewModel viewmodel, long songId)
        {
            using var conn = _dataSource.OpenConnection();
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select song.song_name ");
                sb.AppendLine("     , song.artist_name ");
                sb.AppendLine("     , coalesce(song.genre_code1, '')");
                sb.AppendLine("       || coalesce(E'\t' || song.genre_code2, '') ");
                sb.AppendLine("       || coalesce(E'\t' || song.genre_code3, '') ");
                sb.AppendLine("       || coalesce(E'\t' || song.genre_code4, '') ");
                sb.AppendLine("       || coalesce(E'\t' || song.genre_code5, '') ");
                sb.AppendLine("       as genre_code ");
                sb.AppendLine("     , coalesce(sgen1.genre_name, '')");
                sb.AppendLine("       || coalesce(E'\t' || sgen2.genre_name, '') ");
                sb.AppendLine("       || coalesce(E'\t' || sgen3.genre_name, '') ");
                sb.AppendLine("       || coalesce(E'\t' || sgen4.genre_name, '') ");
                sb.AppendLine("       || coalesce(E'\t' || sgen5.genre_name, '') ");
                sb.AppendLine("       as genre_name ");
                sb.AppendLine("     , coalesce(song.tieup_name, '') as tieup_name ");
                sb.AppendLine("  from song_list song ");
                sb.AppendLine(" left join song_genre_list sgen1 ");
                sb.AppendLine("    on song.genre_code1 = sgen1.genre_code ");
                sb.AppendLine(" left join song_genre_list sgen2 ");
                sb.AppendLine("    on song.genre_code2 = sgen2.genre_code ");
                sb.AppendLine(" left join song_genre_list sgen3 ");
                sb.AppendLine("    on song.genre_code3 = sgen3.genre_code ");
                sb.AppendLine(" left join song_genre_list sgen4 ");
                sb.AppendLine("    on song.genre_code4 = sgen4.genre_code ");
                sb.AppendLine(" left join song_genre_list sgen5 ");
                sb.AppendLine("    on song.genre_code5 = sgen5.genre_code ");
                sb.AppendLine(" where song.song_id = @songId ");

                using (var cmd = new NpgsqlCommand(sb.ToString(), conn))
                {
                   cmd.Parameters.Add("@songId", NpgsqlTypes.NpgsqlDbType.Numeric, 8).Value = songId;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            viewmodel.DtlSongName = reader.GetString(0);
                            viewmodel.DtlArtistName = reader.GetString(1);
                            viewmodel.DtlGenreCode = reader.GetString(2);
                            viewmodel.DtlGenreName = reader.GetString(3);
                            viewmodel.DtlTieupName = reader.GetString(4);
                        }
                    }
                }
                conn.Close();
            }
        }
 
         public List<StreamListModel> GetStreamList(long songId, string memberCode)
        {
            var memberList = new List<StreamListModel>();
            using var conn = _dataSource.OpenConnection();
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select case when slst.start_time is null then strl.stream_url ");
                sb.AppendLine("        else strl.stream_url || '?t=' ");
                sb.AppendLine("         || extract(hour from slst.start_time) * 3600 + extract(minute from slst.start_time) * 60 + extract(second from slst.start_time) || 's' ");
                sb.AppendLine("        end as stream_url ");
                sb.AppendLine("     , to_char(strl.release_date, 'YYYY/MM/DD') as release_date ");
                sb.AppendLine("     , strl.stream_title ");
                sb.AppendLine("     , coalesce(slst.setlist_notes, '') as setlist_notes ");
                sb.AppendLine("     , coalesce(strl.limited_code, '') as limited_code ");
                sb.AppendLine("     , coalesce(limt.limited_name, '') as limited_name ");
                sb.AppendLine("  from setlist slst ");
                sb.AppendLine(" inner join stream_list strl ");
                sb.AppendLine("    on slst.stream_id = strl.stream_id ");
                sb.AppendLine("   and slst.member_code = strl.member_code ");
                sb.AppendLine(" left join limited_list limt ");
                sb.AppendLine("    on limt.limited_code = strl.limited_code ");
                sb.AppendLine(" where slst.song_id = @songId ");
                sb.AppendLine("   and slst.member_code = @memberCode ");
                sb.AppendLine("   and strl.limited_code <> '02' ");
                sb.AppendLine(" order by strl.release_date ");

                using (var cmd = new NpgsqlCommand(sb.ToString(), conn))
                {
                    cmd.Parameters.Add("@songId", NpgsqlTypes.NpgsqlDbType.Numeric, 8).Value = songId;
                    cmd.Parameters.Add("@memberCode", NpgsqlTypes.NpgsqlDbType.Varchar, 2).Value = memberCode;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            memberList.Add(new StreamListModel
                            {
                                StreamUrl = reader.GetString(0),
                                ReleaseDate = reader.GetString(1),
                                StreamTitle = reader.GetString(2),
                                SetlistNotes = reader.GetString(3),
                                LimitedCode = reader.GetString(4),
                                LimitedName = reader.GetString(5)
                            });
                        }
                    }
                }
                conn.Close();
            }
            return memberList;
        }
    }    
}
