CREATE TABLE [dbo].[CertificateSerialNumber]
(
	[CertificateSerialNumberId] INT NOT NULL IDENTITY, 
    [SerialNumber] BIGINT NULL, 
    CONSTRAINT [PK_CertificateSerialNumber] PRIMARY KEY ([CertificateSerialNumberId]) 
)
