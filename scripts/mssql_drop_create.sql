USE [master]
GO
ALTER DATABASE [test_studentbank] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
GO

DROP DATABASE [test_studentbank]
GO

CREATE DATABASE [test_studentbank];
