CREATE TABLE [dbo].[UploadedDocuments] (
    [DocumentUploadId] INT           IDENTITY (1, 1) NOT NULL,
    [DocumentName]     VARCHAR (MAX) NULL,
    [DocumentSource]   VARCHAR (MAX) NULL,
    [ApplicationId]    VARCHAR (30)  NULL,
    CONSTRAINT [PK_UploadedDocuments] PRIMARY KEY CLUSTERED ([DocumentUploadId] ASC),
);

