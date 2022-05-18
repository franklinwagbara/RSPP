CREATE TABLE [dbo].[UserMaster] (
    [UserMasterId]   INT NOT NULL IDENTITY,
    [UserEmail]      VARCHAR (150) NULL,
    [CompanyAddress] VARCHAR (MAX) NULL,
    [Password]       VARCHAR (100) NULL,
    [PhoneNum]       VARCHAR (20)  NULL,
    [CompanyName]    VARCHAR (350) NULL,
    [UserType]       VARCHAR (30)  NULL,
    [UserRole]       VARCHAR (50)  NULL,
    [CreatedOn]      DATETIME      NULL,
    [UpdatedBy]      VARCHAR (80)  NULL,
    [UpdatedOn]      DATETIME      NULL,
    [Status]         VARCHAR (10)  NULL,
    [LastLogin]      DATETIME      NULL,
    [LoginCount]     INT           NULL,
    [LastComment]    VARCHAR (MAX) NULL,
    [FirstName]      VARCHAR (30)  NULL,
    [LastName]       VARCHAR (30)  NULL,
    [SignatureImage] VARCHAR(300) NULL
    CONSTRAINT [PK_UserMaster] PRIMARY KEY CLUSTERED ([UserMasterId])
);

