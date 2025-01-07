using Microsoft.AspNetCore.Http;

namespace CS322_PZ_AnteaPrimorac5157.Extensions
{
    public static class SessionExtensions
    {
        private const string LikePrefix = "Liked_";

        public static bool? GetBool(this ISession session, string key)
        {
            var data = session.Get(key);
            if (data == null)
                return null;
            return BitConverter.ToBoolean(data, 0);
        }

        public static void SetBool(this ISession session, string key, bool value)
        {
            session.Set(key, BitConverter.GetBytes(value));
        }

        public static bool HasLiked(this ISession session, string entityType, int id)
        {
            return session.GetBool($"{LikePrefix}{entityType}_{id}") ?? false;
        }

        public static void SetLiked(this ISession session, string entityType, int id, bool liked)
        {
            session.SetBool($"{LikePrefix}{entityType}_{id}", liked);
        }

        public static bool HasLikedConfession(this ISession session, int id)
        => session.HasLiked("Confession", id);

        public static bool HasLikedComment(this ISession session, int id)
            => session.HasLiked("Comment", id);

        public static void SetConfessionLiked(this ISession session, int id, bool liked)
            => session.SetLiked("Confession", id, liked);

        public static void SetCommentLiked(this ISession session, int id, bool liked)
            => session.SetLiked("Comment", id, liked);
    }
}