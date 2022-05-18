CREATE TABLE [dbo].[PaymentLog] (
    [PaymentID]       BIGINT          IDENTITY (1, 1) NOT NULL,
    [ApplicationID]   VARCHAR (30)    NOT NULL,
    [ApplicantID]     VARCHAR (50)    NOT NULL,
    [TransactionDate] DATETIME        NULL,
    [TransactionID]   VARCHAR (30)    NULL,
    [PaymentCategory] VARCHAR (MAX)   NULL,
    [Description]     VARCHAR (150)   NULL,
    [RRReference]     VARCHAR (30)    NULL,
    [AppReceiptID]    VARCHAR (30)    NULL,
    [TxnAmount]       DECIMAL (20, 2) NULL,
    [Arrears]         DECIMAL (20, 2) NOT NULL,
    [BankCode]        VARCHAR (5)     NULL,
    [Account]         VARCHAR (20)    NULL,
    [TxnMessage]      VARCHAR (150)   NULL,
    [Status]          VARCHAR (10)    NULL,
    [RetryCount]      INT             NULL,
    [LastRetryDate]   DATETIME        NULL,
    [ActionBy]        VARCHAR (80)    NULL,
    CONSTRAINT [PK_PaymentLog] PRIMARY KEY CLUSTERED ([PaymentID] ASC)
);



