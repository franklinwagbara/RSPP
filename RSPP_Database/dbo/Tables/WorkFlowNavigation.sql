CREATE TABLE [dbo].[WorkFlowNavigation] (
    [WorkFlowId]     INT           IDENTITY (1, 1) NOT NULL,
    [Action]         VARCHAR (30)  NOT NULL,
    [ActionRole]     VARCHAR (30)  NULL,
    [CurrentStageID] SMALLINT      NOT NULL,
    [NextStateID]    SMALLINT      NOT NULL,
    [TargetRole]     VARCHAR (150) NOT NULL,
    CONSTRAINT [PK_WorkFlowNavigation] PRIMARY KEY CLUSTERED ([WorkFlowId] ASC)
);

