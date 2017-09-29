/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

INSERT INTO Product VALUES 
	('Finn The Human Plush', 'Official adventure time product by Jazwares', 5.07, 'finn.jpg'),
	('Jake The Dog Plush', 'Made from high quality materials and very cute', 7.99, 'jake.jpg'),
	('Lovecraftian Cthulhu Slippers Plush Toy', 'Now you can wear Lovecraft''s vision of terror on your feet!', 10.5, 'slippers.jpg'),
	('Dalek Action Figure', 'Exterminate! EXTERMINATE! EXTERMINATE! EXTERMINATE!', 9.5, 'dalek.jpg'),
	('Weeping Angel Action Figure', 'Don''t blink, don''t look away! ', 8.6, 'weeping-angel.jpg'),
	('Vortex Manipulator', 'Makes A Great Gift for Any Dr Who Fan and Whovian!', 11.33, 'vortex-manipulator.png')