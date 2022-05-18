CREATE TABLE [dbo].[GovernmentAgency] (
    [GovAgencyId]               INT           IDENTITY (1, 1) NOT NULL,
    [Services_Provided_In_Port] VARCHAR (MAX) NULL,
    [Any_Other_Relevant_Info]   VARCHAR (MAX) NULL,
    [ApplicationId]             VARCHAR (30)  NULL,
    CONSTRAINT [PK_GovernmentAgency] PRIMARY KEY CLUSTERED ([GovAgencyId] ASC)
);

