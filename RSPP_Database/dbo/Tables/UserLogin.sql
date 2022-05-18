CREATE TABLE [dbo].[UserLogin] (
    [LoginPk]      BIGINT        IDENTITY (1, 1) NOT NULL,
    [UserEmail]    VARCHAR (150) NOT NULL,
    [UserType]     VARCHAR (100) NULL,
    [Browser]      VARCHAR (MAX) NULL,
    [Client]       VARCHAR (50)  NULL,
    [LoginTime]    DATETIME      NULL,
    [Status]       VARCHAR (10)  NULL,
    [LoginMessage] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_UserLogin] PRIMARY KEY CLUSTERED ([LoginPk] ASC, [UserEmail] ASC)
);

