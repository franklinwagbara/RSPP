using RSPP.Helpers;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;

namespace RRPSPU.Tests.MockData
{
    public static class AuthenticationMock
    {
        public static List<UserMaster> GetFakeEmptyUserMasterList()
        {
            return new List<UserMaster>();
        }
        public static List<UserMaster> GetFakeUnConfirmedUserMasterList()
        {
            return new List<UserMaster>{

                    new UserMaster()
                    {
                        UserEmail= "me@gmail.com",
                        Password="password",
                        Status=AppMessages.ACTIVE,
                        PhoneNum = "09087866567",
                        CompanyAddress = "address",
                        CompanyName = "company name",
                        UserType = AppMessages.COMPANY,
                        UserRole = AppMessages.COMPANY,
                        UpdatedBy = "adam@gmail.com",
                        LoginCount = 0,
                        CreatedOn = DateTime.Now,
                        EmailConfirmed = false,
                        EmailConfirmationToken = Guid.NewGuid().ToString()
                    },
                    new UserMaster()
                    {
                        UserEmail= "you@gmail.com",
                        Password="password",
                        Status=AppMessages.ACTIVE,
                        PhoneNum = "09087866567",
                        CompanyAddress = "address",
                        CompanyName = "company name",
                        UserType = AppMessages.COMPANY,
                        UserRole = AppMessages.COMPANY,
                        UpdatedBy = "adam@gmail.com",
                        LoginCount = 0,
                        CreatedOn = DateTime.Now,
                        EmailConfirmed = false,
                        EmailConfirmationToken = Guid.NewGuid().ToString()
                    },
                    new UserMaster()
                    {
                        UserEmail= "yoyo@gmail.com",
                        Password="password",
                        Status=AppMessages.PASSIVE,
                        PhoneNum = "09087866567",
                        CompanyAddress = "address",
                        CompanyName = "company name",
                        UserType = AppMessages.COMPANY,
                        UserRole = AppMessages.COMPANY,
                        UpdatedBy = "adam@gmail.com",
                        LoginCount = 0,
                        CreatedOn = DateTime.Now,
                        EmailConfirmed = false,
                        EmailConfirmationToken = Guid.NewGuid().ToString()
                    }

            };
        }

        public static UserMaster GetFakeUnConfirmedUserMaster()
        {
            return new UserMaster()
            {
                UserEmail = "great@gmail.com",
                Password = "password",
                Status = AppMessages.ACTIVE,
                PhoneNum = "09087866567",
                CompanyAddress = "address",
                CompanyName = "company name",
                UserType = AppMessages.COMPANY,
                UserRole = AppMessages.COMPANY,
                UpdatedBy = "great@gmail.com",
                LoginCount = 0,
                CreatedOn = DateTime.Now,
                EmailConfirmed = false,
                EmailConfirmationToken = Guid.NewGuid().ToString(),
            };
        }
    }
}
