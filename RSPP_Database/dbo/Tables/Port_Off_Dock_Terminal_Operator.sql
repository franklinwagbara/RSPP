CREATE TABLE [dbo].[Port_Off_Dock_Terminal_Operator] (
    [Port_Off_Terminal_OperatorId] INT           IDENTITY (1, 1) NOT NULL,
    [Line_of_Business]             VARCHAR (50)  NULL,
    [Name_of_Terminal]             VARCHAR (500) NULL,
    [Location_of_Terminal]         VARCHAR (MAX) NULL,
    [Status_of_Terminal]           VARCHAR (MAX) NULL,
    [Cargo_Type]                   VARCHAR (MAX) NULL,
    [Any_Other_Info]               VARCHAR (MAX) NULL,
    [ApplicationId]                VARCHAR (30)  NULL,
    CONSTRAINT [PK_Port_Off_Dock_Terminal_Operator] PRIMARY KEY CLUSTERED ([Port_Off_Terminal_OperatorId] ASC),
);

