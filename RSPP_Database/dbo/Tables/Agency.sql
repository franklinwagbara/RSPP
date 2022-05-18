CREATE TABLE [dbo].[Agency] (
    [AgencyId]   INT           IDENTITY (1, 1) NOT NULL,
    [AgencyName] VARCHAR (250) NULL,
    CONSTRAINT [PK_Agency] PRIMARY KEY CLUSTERED ([AgencyId] ASC)
);

