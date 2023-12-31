USE [ECD_FHIR]
GO

--CREATE SCRIPT
/****** Object:  Schema [Logging]    Script Date: 4/8/2019 8:25:22 AM ******/
CREATE SCHEMA [Logging]
GO
/****** Object:  Table [Logging].[Application]    Script Date: 4/8/2019 8:25:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Logging].[Application](
	[ApplicationID] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationName] [varchar](255) NOT NULL,
 CONSTRAINT [PK_Application] PRIMARY KEY CLUSTERED 
(
	[ApplicationID] ASC
)WITH (DATA_COMPRESSION = PAGE, PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Logging].[EventLevel]    Script Date: 4/8/2019 8:25:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Logging].[EventLevel](
	[EventLevelID] [int] IDENTITY(1,1) NOT NULL,
	[EventType] [varchar](255) NOT NULL,
	[EventValue] [int] NOT NULL,
 CONSTRAINT [PK_EventLevel] PRIMARY KEY CLUSTERED 
(
	[EventLevelID] ASC
)WITH (DATA_COMPRESSION = PAGE, PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
) 

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Logging].[Log]    Script Date: 4/8/2019 8:25:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Logging].[Log](
	[LogID] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationID] [int] NOT NULL,
	[ThreadID] [int] NULL,
	[ProcessID] [int] NOT NULL,
	[MachineName] [varchar](255) NOT NULL,
	[UserName] [varchar](255) NOT NULL,
	[EventLevelID] [int] NOT NULL,
	[Message] [varchar](max) NOT NULL,
	[Payload] [varchar](max) NULL,
	[DateTimeStamp] [datetime] NOT NULL,
 CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED 
(
	[LogID] ASC
)WITH (DATA_COMPRESSION = PAGE, PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
) 

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Logging].[Process]    Script Date: 4/8/2019 8:25:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Logging].[Process](
	[ProcessID] [int] IDENTITY(1,1) NOT NULL,
	[ProcessName] [varchar](255) NOT NULL,
	[ApplicationID] [int] NOT NULL,
 CONSTRAINT [PK_Process] PRIMARY KEY CLUSTERED 
(
	[ProcessID] ASC
)WITH (DATA_COMPRESSION = PAGE, PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [Logging].[Subscriber]    Script Date: 4/8/2019 8:25:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [Logging].[Subscriber](
	[SubscriberID] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationID] [int] NOT NULL,
	[ProcessID] [int] NOT NULL,
	[EventLevelID] [int] NOT NULL,
	[Address] [varchar](255) NOT NULL,
 CONSTRAINT [PK_Subscriber] PRIMARY KEY CLUSTERED 
(
	[SubscriberID] ASC
)WITH (DATA_COMPRESSION = PAGE, PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)

GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [Logging].[LogView]    Script Date: 4/8/2019 8:25:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [Logging].[LogView]
AS
SELECT        Logging.[Log].LogID, Logging.Application.ApplicationName, Logging.Process.ProcessName, Logging.[Log].MachineName, Logging.EventLevel.EventType, Logging.[Log].ThreadID, 
                                                                                                 Logging.[Log].UserName, Logging.[Log].Message, Logging.[Log].Payload, Logging.[Log].DateTimeStamp
FROM            Logging.[Log] (NOLOCK) INNER JOIN
                         Logging.Application (NOLOCK) ON Logging.[Log].ApplicationID = Logging.Application.ApplicationID INNER JOIN
                         Logging.Process (NOLOCK) ON Logging.[Log].ProcessID = Logging.Process.ProcessID AND Logging.Application.ApplicationID = Logging.Process.ApplicationID INNER JOIN
                         Logging.EventLevel (NOLOCK) ON Logging.[Log].EventLevelID = Logging.EventLevel.EventLevelID



GO
SET IDENTITY_INSERT [Logging].[EventLevel] ON 

GO
INSERT [Logging].[EventLevel] ([EventLevelID], [EventType], [EventValue]) VALUES (1, N'None', 0)
GO
INSERT [Logging].[EventLevel] ([EventLevelID], [EventType], [EventValue]) VALUES (2, N'Verbose', 1000)
GO
INSERT [Logging].[EventLevel] ([EventLevelID], [EventType], [EventValue]) VALUES (3, N'Information', 2000)
GO
INSERT [Logging].[EventLevel] ([EventLevelID], [EventType], [EventValue]) VALUES (4, N'Warning', 3000)
GO
INSERT [Logging].[EventLevel] ([EventLevelID], [EventType], [EventValue]) VALUES (5, N'Error', 4000)
GO
INSERT [Logging].[EventLevel] ([EventLevelID], [EventType], [EventValue]) VALUES (6, N'Critical', 5000)
GO
INSERT [Logging].[EventLevel] ([EventLevelID], [EventType], [EventValue]) VALUES (7, N'Fatal', 6000)
GO
INSERT [Logging].[EventLevel] ([EventLevelID], [EventType], [EventValue]) VALUES (8, N'All', 2147483647)
GO
SET IDENTITY_INSERT [Logging].[EventLevel] OFF
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [AK_ApplicationName]    Script Date: 4/8/2019 8:25:25 AM ******/
ALTER TABLE [Logging].[Application] ADD  CONSTRAINT [AK_ApplicationName] UNIQUE NONCLUSTERED 
(
	[ApplicationName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [AK_ProcessName]    Script Date: 4/8/2019 8:25:25 AM ******/
ALTER TABLE [Logging].[Process] ADD  CONSTRAINT [AK_ProcessName] UNIQUE NONCLUSTERED 
(
	[ProcessName] ASC,
	[ApplicationID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
ALTER TABLE [Logging].[Log]  WITH CHECK ADD  CONSTRAINT [FK_Log_Application] FOREIGN KEY([ApplicationID])
REFERENCES [Logging].[Application] ([ApplicationID])
GO
ALTER TABLE [Logging].[Log] CHECK CONSTRAINT [FK_Log_Application]
GO
ALTER TABLE [Logging].[Log]  WITH CHECK ADD  CONSTRAINT [FK_Log_EventLevel] FOREIGN KEY([EventLevelID])
REFERENCES [Logging].[EventLevel] ([EventLevelID])
GO
ALTER TABLE [Logging].[Log] CHECK CONSTRAINT [FK_Log_EventLevel]
GO
ALTER TABLE [Logging].[Log]  WITH CHECK ADD  CONSTRAINT [FK_Log_Process] FOREIGN KEY([ProcessID])
REFERENCES [Logging].[Process] ([ProcessID])
GO
ALTER TABLE [Logging].[Log] CHECK CONSTRAINT [FK_Log_Process]
GO
ALTER TABLE [Logging].[Process]  WITH CHECK ADD  CONSTRAINT [FK_Process_Application] FOREIGN KEY([ApplicationID])
REFERENCES [Logging].[Application] ([ApplicationID])
GO
ALTER TABLE [Logging].[Process] CHECK CONSTRAINT [FK_Process_Application]
GO
ALTER TABLE [Logging].[Subscriber]  WITH CHECK ADD  CONSTRAINT [FK_Subscriber_Application] FOREIGN KEY([ApplicationID])
REFERENCES [Logging].[Application] ([ApplicationID])
GO
ALTER TABLE [Logging].[Subscriber] CHECK CONSTRAINT [FK_Subscriber_Application]
GO
ALTER TABLE [Logging].[Subscriber]  WITH CHECK ADD  CONSTRAINT [FK_Subscriber_EventLevel] FOREIGN KEY([EventLevelID])
REFERENCES [Logging].[EventLevel] ([EventLevelID])
GO
ALTER TABLE [Logging].[Subscriber] CHECK CONSTRAINT [FK_Subscriber_EventLevel]
GO
ALTER TABLE [Logging].[Subscriber]  WITH CHECK ADD  CONSTRAINT [FK_Subscriber_Process] FOREIGN KEY([ProcessID])
REFERENCES [Logging].[Process] ([ProcessID])
GO
ALTER TABLE [Logging].[Subscriber] CHECK CONSTRAINT [FK_Subscriber_Process]
GO
/****** Object:  StoredProcedure [Logging].[GetApplicationID]    Script Date: 4/8/2019 8:25:22 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [Logging].[GetApplicationID]
                (
                @ApplicationName varchar(255),
                @ApplicationID INT OUTPUT
                )
AS
                SET NOCOUNT ON 
                IF @ApplicationName = ''
                BEGIN
                                SET @ApplicationName = 'Default'
                END
                SELECT @ApplicationID = ApplicationID FROM [Logging].Application (NOLOCK)
                WHERE ApplicationName = @ApplicationName
                IF @ApplicationID IS NULL
                BEGIN
					BEGIN TRY
                        INSERT INTO [Logging].Application (ApplicationName) VALUES (@ApplicationName)
                        SELECT @ApplicationID = scope_identity()
					END TRY
					BEGIN CATCH
						SELECT @ApplicationID = ApplicationID FROM [Logging].Application (NOLOCK)
						WHERE ApplicationName = @ApplicationName
						IF @ApplicationID IS NULL
						BEGIN
							EXEC [Logging].[GetApplicationID] @ApplicationName, @ApplicationID OUTPUT
						END
					END CATCH
                END
                RETURN


GO
/****** Object:  StoredProcedure [Logging].[GetEventLevelID]    Script Date: 4/8/2019 8:25:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [Logging].[GetEventLevelID]
                (
                @EventType varchar(255),
                @EventID INT OUTPUT
                )
AS
                SET NOCOUNT ON 
                IF @EventType = ''
                BEGIN
                                SET @EventType = 'None'
                END
                SELECT @EventID = EventLevelID FROM [Logging].EventLevel (NOLOCK)
                WHERE EventType = @EventType
                IF @EventID IS NULL
                BEGIN
                                INSERT INTO [Logging].EventLevel (EventType, EventValue) VALUES (@EventType, 0)
                                SELECT @EventID = scope_identity()
                END
                RETURN --@EventID


GO
/****** Object:  StoredProcedure [Logging].[GetProcessID]    Script Date: 4/8/2019 8:25:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [Logging].[GetProcessID]
                (
                @ProcessName varchar(255),
                @ApplicationID INT,
                @ProcessID INT OUTPUT
                )
AS
                SET NOCOUNT ON
                IF @ProcessName = ''
                BEGIN
                    SET @ProcessName = 'Default'
                END 
                
                SELECT @ProcessID = ProcessID FROM [Logging].Process (NOLOCK)
                WHERE ProcessName = @ProcessName AND ApplicationID = @ApplicationID
                IF @ProcessID IS NULL
                BEGIN
					BEGIN TRY
						INSERT INTO [Logging].Process (ProcessName, ApplicationID) VALUES (@ProcessName, @ApplicationID)
						SELECT @ProcessID = scope_identity()
					END TRY
					BEGIN CATCH
						SELECT @ProcessID = ProcessID FROM [Logging].Process (NOLOCK)
						WHERE ProcessName = @ProcessName AND ApplicationID = @ApplicationID
						IF @ProcessID IS NULL
						BEGIN
							EXEC [Logging].[GetProcessID] @ProcessName, @ApplicationID, @ProcessID OUTPUT
						END
					END CATCH
                END
                RETURN --@ProcessID

GO
/****** Object:  StoredProcedure [Logging].[LogEvent]    Script Date: 4/8/2019 8:25:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [Logging].[LogEvent]
                (
                @Application varchar(255),
                @Process varchar(255) = '',
                @Machine varchar(255) = '',
                @ThreadID int = 0,
                @UserName varchar(255),
                @EventType varchar(255),
                @Message varchar(MAX),
                @Payload varchar(MAX) = '',
				@TimeStamp DateTime = null
                )
AS
                SET NOCOUNT ON

				SET @TimeStamp = COALESCE(@TimeStamp, GetDate())

                DECLARE @AppID INT
                EXEC [Logging].GetApplicationID @Application, @AppID OUTPUT
                DECLARE @ProcID INT
                EXEC [Logging].GetProcessID @Process, @AppID, @ProcID OUTPUT
                DECLARE @AllProcID INT
                EXEC [Logging].GetProcessID 'All', @AppID, @AllProcID OUTPUT
                DECLARE @EvID INT
                EXEC [Logging].GetEventLevelID @EventType, @EvID OUTPUT
                
                INSERT INTO [Logging].Log (ApplicationID, ThreadID, ProcessID, MachineName, UserName, EventLevelID, Message, Payload, DateTimeStamp) 
                                                VALUES (@AppID, @ThreadID, @ProcID, @Machine, @UserName, @EvID, @Message, @Payload, @TimeStamp)
                
				DECLARE @Body VARCHAR(MAX) = CONCAT(
					'The following ',@EventType,' event was generated by ', @Process, ' in ', @Application,
					':<p/><b>LogID</b>: ', scope_identity(), '<p/>',
					'<b>Message</b>: ', @Message, '<p/>',
					'<b>Machine</b>: ', @Machine, '<p/>',
					'<b>Process</b>: ', @Process, '<p/>',
					'<b>User</b>: ', @UserName, '<p/>',
					'<b>Payload</b>: ', @Payload, '<p/>')

				DECLARE @Subject VARCHAR(MAX) = CONCAT(@EventType, ' Exception in ', @Application)
                
                --Get subscriber email addresses to pass back
                DECLARE @EvLev INT
                SELECT @EvLev = EventValue FROM [Logging].EventLevel (NOLOCK)
                WHERE EventLevelID = @EvID
                
				DECLARE @EmailRecipients VARCHAR(MAX)

                SET @EmailRecipients = (SELECT A = REPLACE ((SELECT [Logging].Subscriber.Address AS 'data()'
					FROM [Logging].Subscriber (NOLOCK) INNER JOIN
					[Logging].EventLevel (NOLOCK) ON [Logging].Subscriber.EventLevelID = [Logging].EventLevel.EventLevelID
					WHERE [Logging].EventLevel.EventValue <= @EvLev 
						AND ApplicationID = @AppID 
						AND ([Logging].Subscriber.ProcessID = @ProcID 
							OR [Logging].Subscriber.ProcessID = @AllProcID)
					FOR XML PATH ( '' )), ' ', '; '))

				IF @EmailRecipients = '' OR @EmailRecipients = '; '
				BEGIN
					RETURN
				END

				DECLARE @Priority VARCHAR(6) = 'Normal'

                DECLARE @CritLev INT
                SELECT @CritLev = EventValue FROM [Logging].EventLevel (NOLOCK)
                WHERE EventType = 'Critical'

                DECLARE @WarnLev INT
                SELECT @WarnLev = EventValue FROM [Logging].EventLevel (NOLOCK)
                WHERE EventType = 'Warning'

				IF @EvLev > (@CritLev - 1)
				BEGIN
					SET @Priority = 'High'
				END
				ELSE IF @EvLev < @WarnLev
				BEGIN
					SET @Priority = 'Low'
				END

				EXEC msdb.dbo.sp_send_dbmail @recipients = @EmailRecipients,
					@subject = @Subject,
					@body = @Body,
					@from_address = 'DoNotReply@va.gov',
					@body_format = 'HTML',
					@importance = @Priority
				
                RETURN

GO
/****** Object:  StoredProcedure [Logging].[Subscribe]    Script Date: 4/8/2019 8:25:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [Logging].[Subscribe]
                (
                @Application varchar(255),
                @Process varchar(255) = '',
                @EventType varchar(255),
                @Email varchar(255)
                )
AS
                SET NOCOUNT ON
                IF @EventType = 'None'
                BEGIN
                                SET @EventType = 'All'
                END
                ELSE IF @EventType = 'All'
                BEGIN
                                SET @EventType = 'None'
                END
                DECLARE @AppID INT
                EXEC [Logging].GetApplicationID @Application, @AppID OUTPUT
                DECLARE @ProcID INT
                EXEC [Logging].GetProcessID @Process, @AppID, @ProcID OUTPUT
                DECLARE @EvID INT
                EXEC [Logging].GetEventLevelID @EventType, @EvID OUTPUT
                DECLARE @SubID INT
                
                SELECT @SubID = SubscriberID FROM [Logging].Subscriber (NOLOCK)
                WHERE ApplicationID = @AppID AND ProcessID = @ProcID AND Address = @Email
                IF @SubID IS NULL
                BEGIN
                INSERT INTO [Logging].Subscriber (ApplicationID, ProcessID, EventLevelID, Address)
                VALUES (@AppID, @ProcID, @EvID, @Email)
                END
                IF @SubID > 0
                BEGIN
                UPDATE [Logging].Subscriber SET EventLevelID = @EvID WHERE SubscriberID = @SubID
                END

                RETURN


GO
