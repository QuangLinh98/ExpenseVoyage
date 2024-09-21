namespace ExpenseVoyage.SessionExtention
{
	public static class SessionExtensions
	{
		public static int? GetUserId(this ISession session)
		{
			return session.GetInt32("UserId");
		}
	}
}
