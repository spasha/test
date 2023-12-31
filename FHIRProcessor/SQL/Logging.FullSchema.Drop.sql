USE [ECD_FHIR]
GO

--DROP SCRIPT
ALTER TABLE [Logging].[Subscriber] DROP CONSTRAINT [FK_Subscriber_Process]
GO
ALTER TABLE [Logging].[Subscriber] DROP CONSTRAINT [FK_Subscriber_EventLevel]
GO
ALTER TABLE [Logging].[Subscriber] DROP CONSTRAINT [FK_Subscriber_Application]
GO
ALTER TABLE [Logging].[Process] DROP CONSTRAINT [FK_Process_Application]
GO
ALTER TABLE [Logging].[Log] DROP CONSTRAINT [FK_Log_Process]
GO
ALTER TABLE [Logging].[Log] DROP CONSTRAINT [FK_Log_EventLevel]
GO
ALTER TABLE [Logging].[Log] DROP CONSTRAINT [FK_Log_Application]
GO
/****** Object:  Index [AK_ProcessName]    Script Date: 4/8/2019 8:25:22 AM ******/
ALTER TABLE [Logging].[Process] DROP CONSTRAINT [AK_ProcessName]
GO
/****** Object:  Index [AK_ApplicationName]    Script Date: 4/8/2019 8:25:22 AM ******/
ALTER TABLE [Logging].[Application] DROP CONSTRAINT [AK_ApplicationName]
GO
/****** Object:  View [Logging].[LogView]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP VIEW [Logging].[LogView]
GO
/****** Object:  Table [Logging].[Subscriber]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP TABLE [Logging].[Subscriber]
GO
/****** Object:  Table [Logging].[Process]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP TABLE [Logging].[Process]
GO
/****** Object:  Table [Logging].[Log]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP TABLE [Logging].[Log]
GO
/****** Object:  Table [Logging].[EventLevel]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP TABLE [Logging].[EventLevel]
GO
/****** Object:  Table [Logging].[Application]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP TABLE [Logging].[Application]
GO
/****** Object:  StoredProcedure [Logging].[Subscribe]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP PROCEDURE [Logging].[Subscribe]
GO
/****** Object:  StoredProcedure [Logging].[LogEvent]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP PROCEDURE [Logging].[LogEvent]
GO
/****** Object:  StoredProcedure [Logging].[GetProcessID]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP PROCEDURE [Logging].[GetProcessID]
GO
/****** Object:  StoredProcedure [Logging].[GetEventLevelID]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP PROCEDURE [Logging].[GetEventLevelID]
GO
/****** Object:  StoredProcedure [Logging].[GetApplicationID]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP PROCEDURE [Logging].[GetApplicationID]
GO
/****** Object:  Schema [Logging]    Script Date: 4/8/2019 8:25:22 AM ******/
DROP SCHEMA [Logging]
GO
