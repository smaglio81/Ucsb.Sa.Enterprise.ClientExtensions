SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
PRINT N'Creating role HttpClientRole'
GO
CREATE ROLE [HttpClientRole]
AUTHORIZATION [dbo]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
BEGIN TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating schemas'
GO
CREATE SCHEMA [Ent_Instrumentation]
AUTHORIZATION [dbo]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [Ent_Instrumentation].[Http_Call_Log]'
GO
CREATE TABLE [Ent_Instrumentation].[Http_Call_Log]
(
[CallId] [int] NOT NULL IDENTITY(1, 1),
[Server] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IP] [varchar] (15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Uri] [varchar] (500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[RequestDate] [datetime] NOT NULL,
[ResponseDate] [datetime] NOT NULL,
[TimeDiff] [time] NOT NULL,
[Direction] [char] (3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[StatusCode] [int] NOT NULL,
[Method] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[MetaData] [varchar] (500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[RequestHeader] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[RequestCookie] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[RequestBody] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ResponseCookie] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ResponseHeader] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ResponseBody] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Ent_Instrumentation.Http_Call_Log] on [Ent_Instrumentation].[Http_Call_Log]'
GO
ALTER TABLE [Ent_Instrumentation].[Http_Call_Log] ADD CONSTRAINT [PK_Ent_Instrumentation.Http_Call_Log] PRIMARY KEY CLUSTERED  ([CallId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_Http_Call_Log_CallId_Server_IP_Uri_RequestDate_ResponseDate_TimeDiff_Direction_StatusCode_Method] on [Ent_Instrumentation].[Http_Call_Log]'
GO
CREATE NONCLUSTERED INDEX [IX_Http_Call_Log_CallId_Server_IP_Uri_RequestDate_ResponseDate_TimeDiff_Direction_StatusCode_Method] ON [Ent_Instrumentation].[Http_Call_Log] ([CallId], [Server], [IP], [Uri], [RequestDate], [ResponseDate], [TimeDiff], [Direction], [StatusCode], [Method])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [Ent_Instrumentation].[Http_Error_Log]'
GO
CREATE TABLE [Ent_Instrumentation].[Http_Error_Log]
(
[ErrorId] [int] NOT NULL IDENTITY(1, 1),
[CallId] [int] NULL,
[Uri] [varchar] (500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[RequestDate] [datetime] NULL,
[Type] [varchar] (200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Message] [varchar] (500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Source] [varchar] (200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[TargetSite] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[StackTrace] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Ent_Instrumentation.Http_Error_Log] on [Ent_Instrumentation].[Http_Error_Log]'
GO
ALTER TABLE [Ent_Instrumentation].[Http_Error_Log] ADD CONSTRAINT [PK_Ent_Instrumentation.Http_Error_Log] PRIMARY KEY CLUSTERED  ([ErrorId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_Http_Error_Log_ErrorId_CallId_Uri_RequestDate] on [Ent_Instrumentation].[Http_Error_Log]'
GO
CREATE NONCLUSTERED INDEX [IX_Http_Error_Log_ErrorId_CallId_Uri_RequestDate] ON [Ent_Instrumentation].[Http_Error_Log] ([ErrorId], [CallId], [Uri], [RequestDate])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on  [Ent_Instrumentation].[Http_Call_Log]'
GO
GRANT SELECT ON  [Ent_Instrumentation].[Http_Call_Log] TO [HttpClientRole]
GO
GRANT INSERT ON  [Ent_Instrumentation].[Http_Call_Log] TO [HttpClientRole]
GO
GRANT DELETE ON  [Ent_Instrumentation].[Http_Call_Log] TO [HttpClientRole]
GO
GRANT UPDATE ON  [Ent_Instrumentation].[Http_Call_Log] TO [HttpClientRole]
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on  [Ent_Instrumentation].[Http_Error_Log]'
GO
GRANT SELECT ON  [Ent_Instrumentation].[Http_Error_Log] TO [HttpClientRole]
GO
GRANT INSERT ON  [Ent_Instrumentation].[Http_Error_Log] TO [HttpClientRole]
GO
GRANT DELETE ON  [Ent_Instrumentation].[Http_Error_Log] TO [HttpClientRole]
GO
GRANT UPDATE ON  [Ent_Instrumentation].[Http_Error_Log] TO [HttpClientRole]
IF @@ERROR <> 0 SET NOEXEC ON
GO
COMMIT TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @Success AS BIT
SET @Success = 1
SET NOEXEC OFF
IF (@Success = 1) PRINT 'The database update succeeded'
ELSE BEGIN
	IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
	PRINT 'The database update failed'
END
GO