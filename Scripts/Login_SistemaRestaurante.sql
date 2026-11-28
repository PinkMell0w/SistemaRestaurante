USE [master]
GO

/* For security reasons the login is created disabled and with a random password. */
/****** Objeto: Login [usr_SistemaRestaurante] Fecha de script: 27-Nov-26 10:49:57 PM ******/
CREATE LOGIN [usr_SistemaRestaurante] WITH PASSWORD=N'yt0CToBvlDl08nf1KsIUTk/cIiqPpAEyQ7ROsI6QTu0=', DEFAULT_DATABASE=[SistemaRestaurante], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

ALTER LOGIN [usr_SistemaRestaurante] DISABLE
GO


