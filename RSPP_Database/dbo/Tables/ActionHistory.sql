CREATE TABLE [dbo].[ActionHistory] (
    [ActionId]        BIGINT        IDENTITY (1, 1) NOT NULL,
    [ApplicationId]   VARCHAR (30)  NULL,
    [CurrentStageID]  SMALLINT      NULL,
    [Action]          VARCHAR (30)  NULL,
    [ActionDate]      DATETIME      NULL,
    [TriggeredBy]     VARCHAR (50)  NULL,
    [TriggeredByRole] VARCHAR (150) NULL,
    [MESSAGE]         TEXT          NULL,
    [TargetedTo]      VARCHAR (50)  NULL,
    [TargetedToRole]  VARCHAR (150) NULL,
    [NextStateID]     SMALLINT      NULL,
    CONSTRAINT [PK_ActionHistory] PRIMARY KEY CLUSTERED ([ActionId] ASC),
);



