CREATE TABLE [dbo].[ExtraPayment] (
    [ExtraPaymentID]     BIGINT          IDENTITY (1, 1) NOT NULL,
    [ApplicationID]      VARCHAR (30)    NOT NULL,
    [ApplicantID]        VARCHAR (50)    NOT NULL,
    [LicenseTypeCode]    VARCHAR (20)    NULL,
    [TransactionDate]    DATETIME        NULL,
    [TransactionID]      VARCHAR (30)    NULL,
    [Description]        VARCHAR (150)   NULL,
    [RRReference]        VARCHAR (30)    NULL,
    [AppReceiptID]       VARCHAR (30)    NULL,
    [TxnAmount]          DECIMAL (20, 2) NULL,
    [Arrears]            DECIMAL (20, 2) NOT NULL,
    [BankCode]           VARCHAR (5)     NULL,
    [Account]            VARCHAR (20)    NULL,
    [TxnMessage]         VARCHAR (150)   NULL,
    [Status]             VARCHAR (10)    NULL,
    [RetryCount]         INT             NULL,
    [LastRetryDate]      DATETIME        NULL,
    [ExtraPaymentAppRef] VARCHAR (30)    NULL,
    [SanctionType]       VARCHAR (80)    NULL,
    [ExtraPaymentBy]     VARCHAR (80)    NULL,
    CONSTRAINT [PK_ExtraPayment] PRIMARY KEY CLUSTERED ([ExtraPaymentID] ASC)
);

