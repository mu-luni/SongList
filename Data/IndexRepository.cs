using SongList.Models.ViewModels;
using System.Text;
using Npgsql;

namespace Index.Repository
{
    public class IndexRepository
    {
        private readonly string _configuration;
        private readonly string _groupCode;
        public IndexRepository(IConfiguration configuration)
        {
                // DB接続文字列を取得
                _configuration = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                // グループコードを取得
                _groupCode = configuration["GroupCode"] ?? throw new InvalidOperationException("Configuration 'GroupCode' not found.");
        }

        public GroupListModel GetGroupList()
        {
            var groupList = new GroupListModel();
            using (var conn = new NpgsqlConnection(_configuration))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("select group_name, description from group_list where group_code = @group_code", conn))
                {
                    cmd.Parameters.Add("@group_code", NpgsqlTypes.NpgsqlDbType.Varchar, 2).Value = _groupCode;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            groupList.GroupName = reader.GetString(0);
                            groupList.Description = reader.GetString(1);
                        }
                    }
                }
                conn.Close();
            }
            return groupList;
        }
        public List<MemberListModel> GetMemberList()
        {
            var memberList = new List<MemberListModel>();
            using (var conn = new NpgsqlConnection(_configuration))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("select member_code, member_name from member_list where group_code = @group_code order by member_code", conn))
                {
                    cmd.Parameters.Add("@group_code", NpgsqlTypes.NpgsqlDbType.Varchar, 2).Value = _groupCode;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            memberList.Add(new MemberListModel
                            {
                                MemberCode = reader.GetString(0),
                                MemberName = reader.GetString(1)
                            });
                        }
                    }
                }
                conn.Close();
            }
            return memberList;
        }
        public List<GenreListModel> GetGenreList()
        {
            var genreList = new List<GenreListModel>();
            using (var conn = new NpgsqlConnection(_configuration))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("select genre_code, genre_name from song_genre_list order by genre_code", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            genreList.Add(new GenreListModel
                            {
                                GenreCode = reader.GetString(0),
                                GenreName = reader.GetString(1)
                            });
                        }
                    }
                }
                conn.Close();
            }
            return genreList;
        }
        public List<SongListModel> GetSongList(SearchCondition searchCondition)
        {
            var songList = new List<SongListModel>();
            using (var conn = new NpgsqlConnection(_configuration))
            {
                conn.Open();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select song.song_id ");
                sb.AppendLine("     , @srcMember as member_code ");
                sb.AppendLine("     , song.song_name ");
                sb.AppendLine("     , song.artist_name ");
                sb.AppendLine("     , coalesce(to_char(slst.last_sung_date, 'YYYY-MM-DD'), '') as last_sung_date ");
                sb.AppendLine("     , coalesce(slst.sung_count, 0) as sung_count ");
                sb.AppendLine("  from song_list song ");
                sb.AppendLine(" left join ");
                sb.AppendLine("       (select sls.song_id ");
                sb.AppendLine("             , max(stl.release_date) as last_sung_date ");
                sb.AppendLine("             , count(sls.stream_id) as sung_count ");
                sb.AppendLine("          from setlist sls ");
                sb.AppendLine("         inner join stream_list stl ");
                sb.AppendLine("            on stl.stream_id = sls.stream_id ");
                sb.AppendLine("           and stl.member_code = sls.member_code ");
                sb.AppendLine("         where sls.member_code = @srcMember ");
                sb.AppendLine("         group by sls.song_id ");
                sb.AppendLine("       ) slst ");
                sb.AppendLine("    on song.song_id = slst.song_id ");
                sb.AppendLine(" where song.sing_member ilike '%' || @srcMember || '%' ");
                if (!string.IsNullOrEmpty(searchCondition.SongName))
                {
                    sb.AppendLine("   and (song.song_name ilike '%' || @srcSong || '%' ");
                    sb.AppendLine("    or  song.song_kana ilike '%' || to_kana(@srcSong) || '%' ");
                    sb.AppendLine("       ) ");
                }
                if (!string.IsNullOrEmpty(searchCondition.ArtistName))
                {
                    sb.AppendLine("   and (song.artist_name ilike '%' || @srcArtist || '%' ");
                    sb.AppendLine("    or  song.artist_kana ilike '%' || to_kana(@srcArtist) || '%' ");
                    sb.AppendLine("       ) ");
                }
                if (!string.IsNullOrEmpty(searchCondition.Genre))
                {
                    sb.AppendLine("   and @srcGenre in (song.genre_code1, song.genre_code2, song.genre_code3, song.genre_code4, song.genre_code5) ");
                }
                if (!string.IsNullOrEmpty(searchCondition.Tieup))
                {
                    sb.AppendLine("   and (song.tieup_name ilike '%' || @srcTieup || '%' ");
                    sb.AppendLine("    or  song.tieup_kana ilike '%' || to_kana(@srcTieup) || '%' ");
                    sb.AppendLine("       ) ");
                }
                sb.AppendLine(" order by song.artist_name ");

                using (var cmd = new NpgsqlCommand(sb.ToString(), conn))
                {
                    cmd.Parameters.Add("@srcMember", NpgsqlTypes.NpgsqlDbType.Varchar, 2).Value = searchCondition.Member;
                    if (!string.IsNullOrEmpty(searchCondition.SongName))
                    {
                        cmd.Parameters.Add("@srcSong", NpgsqlTypes.NpgsqlDbType.Varchar, 500).Value = searchCondition.SongName;
                    }
                    if (!string.IsNullOrEmpty(searchCondition.ArtistName))
                    {
                        cmd.Parameters.Add("@srcArtist", NpgsqlTypes.NpgsqlDbType.Varchar, 500).Value = searchCondition.ArtistName;
                    }
                    if (!string.IsNullOrEmpty(searchCondition.Genre))
                    {
                        cmd.Parameters.Add("@srcGenre", NpgsqlTypes.NpgsqlDbType.Varchar, 500).Value = searchCondition.Genre;
                    }
                    if (!string.IsNullOrEmpty(searchCondition.Tieup))
                    {
                        cmd.Parameters.Add("@srcTieup", NpgsqlTypes.NpgsqlDbType.Varchar, 500).Value = searchCondition.Tieup;
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            songList.Add(new SongListModel
                            {
                                SongId = reader.GetInt64(0),
                                MemberCode = reader.GetString(1),
                                SongName = reader.GetString(2),
                                ArtistName = reader.GetString(3),
                                LastSungDate = reader.GetString(4),
                                SungCount = reader.GetInt32(5)
                            });
                        }
                    }
                }
                conn.Close();
            }

            return songList;
        }
    }    
}
