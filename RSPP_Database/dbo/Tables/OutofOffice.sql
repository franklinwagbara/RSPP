CREATE TABLE [dbo].[OutofOffice] (
    [OutofOfficeId] INT           IDENTITY (1, 1) NOT NULL,
    [Reliever]      VARCHAR (50)  NULL,
    [Relieved]      VARCHAR (50)  NULL,
    [StartDate]     DATETIME      NULL,
    [EndDate]       DATETIME      NULL,
    [Comment]       VARCHAR (MAX) NULL,
    [Status]        VARCHAR (15)  NULL,
    CONSTRAINT [PK_OutofOffice] PRIMARY KEY CLUSTERED ([OutofOfficeId] ASC)
);

