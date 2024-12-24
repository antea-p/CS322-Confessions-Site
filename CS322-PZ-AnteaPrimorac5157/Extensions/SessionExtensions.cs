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

        public static bool HasLiked(this ISession session, int confessionId)
        {
            return session.GetBool($"{LikePrefix}{confessionId}") ?? false;
        }

        public static void SetLiked(this ISession session, int confessionId, bool liked)
        {
            session.SetBool($"{LikePrefix}{confessionId}", liked);
        }
    }
}