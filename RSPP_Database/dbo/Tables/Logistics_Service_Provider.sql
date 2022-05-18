CREATE TABLE [dbo].[Logistics_Service_Provider] (
    [Logistics_Service_Provider_Id] INT           IDENTITY (1, 1) NOT NULL,
    [Line_of_Business]              VARCHAR (MAX) NULL,
    [Custom_License_Num]            VARCHAR (100) NULL,
    [CRFFN_Registration_Num]        VARCHAR (100) NULL,
    [Other_License]                 VARCHAR (100) NULL,
    [Any_Other_Info]                VARCHAR (MAX) NULL,
    [ApplicationId]                 VARCHAR (30)  NULL,
    [Custom_License_ExpiryDate]     DATETIME      NULL,
    [CRFFN_Registraton_ExpiryDate]  DATETIME      NULL,
    [Other_License_ExpiryDate]      DATETIME      NULL,
    CONSTRAINT [PK_Logistics_Service_Provider] PRIMARY KEY CLUSTERED ([Logistics_Service_Provider_Id] ASC),
);

