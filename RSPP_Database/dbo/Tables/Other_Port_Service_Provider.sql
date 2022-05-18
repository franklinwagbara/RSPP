CREATE TABLE [dbo].[Other_Port_Service_Provider] (
    [Other_Port_Service_Provider_Id] INT           IDENTITY (1, 1) NOT NULL,
    [Line_of_Business]               VARCHAR (MAX) NULL,
    [Any_Other_Info]                 VARCHAR (MAX) NULL,
    [ApplicationId]                  VARCHAR (30)  NULL,
    CONSTRAINT [PK_Other_Port_Service_Provider] PRIMARY KEY CLUSTERED ([Other_Port_Service_Provider_Id] ASC),
);

