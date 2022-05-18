CREATE TABLE [dbo].[PaymentCategory] (
    [PaymentCategoryId]   INT             IDENTITY (1, 1) NOT NULL,
    [PaymentCategoryName] VARCHAR (500)   NULL,
    [PaymentAmount]       DECIMAL (18, 2) NULL,
    CONSTRAINT [PK_PaymentCategory] PRIMARY KEY CLUSTERED ([PaymentCategoryId] ASC)
);

