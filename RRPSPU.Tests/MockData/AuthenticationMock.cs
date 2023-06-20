using RSPP.Helpers;
using RSPP.Models.DB;

namespace RRPSPU.Tests.MockData
{
    public static class AuthenticationMock
    {
        public static List<UserMaster> GetFakeUserMasterList()
        {
            return new List<UserMaster>{

                new UserMaster()
                {
                    UserEmail= "me@gmail.com",
                    Password="password",
                    Status=AppMessages.ACTIVE_USER
                },
                    new UserMaster()
                {
                    UserEmail= "you@gmail.com",
                    Password="password",
                    Status=AppMessages.ACTIVE_USER
                },
                    new UserMaster()
                {
                    UserEmail= "yoyo@gmail.com",
                    Password="password",
                    Status=AppMessages.PASSIVE_USER
                }

            };
        }

        public static UserMaster GetFakeUserMaster()
        {
            return new UserMaster()
            {
                UserEmail = "yoyo@gmail.com",
                Password = "password",
                Status = AppMessages.ACTIVE_USER
            };
        }
    }
}
