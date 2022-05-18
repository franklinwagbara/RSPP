CREATE TABLE [dbo].[MissingDocuments] (
    [MissingDocId]  INT          IDENTITY (1, 1) NOT NULL,
    [ApplicationId] VARCHAR (30) NOT NULL,
    [DocId]         INT          NOT NULL,
    CONSTRAINT [PK_MissingDocuments] PRIMARY KEY CLUSTERED ([MissingDocId] ASC),
    CONSTRAINT [FK_MissingDocuments_ApplicationRequestForm] FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[ApplicationRequestForm] ([ApplicationId])
);

