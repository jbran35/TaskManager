//using System.Security.Claims;

//namespace TaskManager.API
//{
//    public static class RetrieveClaims
//    {
//        public static Guid GetUserId(this ClaimsPrincipal user)
//        {
//            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);

//            if (string.IsNullOrEmpty(userIdString))
//            {
//                return Unauthorized(new { Message = "User ID not found in token" });
//            }

//        }
//    }
//}
