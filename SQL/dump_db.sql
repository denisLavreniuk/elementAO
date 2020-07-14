DROP TABLE IF EXISTS increasement;
DROP TABLE IF EXISTS usertbl;
DROP TABLE IF EXISTS deposit;


CREATE TABLE usertbl
(
    id MEDIUMINT NOT NULL AUTO_INCREMENT,
	login varchar(50),
    full_name varchar(100) NOT NULL,
    email varchar(100) NOT NULL,
	password varchar(100) NOT NULL,
    referal varchar(100) DEFAULT NULL,
	balance integer DEFAULT '0',
	total_earn integer DEFAULT '0',
	valid BOOLEAN DEFAULT false,
	`type` integer DEFAULT '0',
	PRIMARY KEY (id)
);

CREATE TABLE deposit
(
    id MEDIUMINT NOT NULL AUTO_INCREMENT,
	userid integer NOT NULL,
	`date` date NOT NULL,
	money integer DEFAULT '0',
	status BOOLEAN DEFAULT false,
	PRIMARY KEY (id)
);

CREATE TABLE increasement
(
	id MEDIUMINT NOT NULL AUTO_INCREMENT,
    userid integer NOT NULL,
	date date NOT NULL,
	time time NOT NULL,
	email varchar(100) NOT NULL,
	amount integer NOT NULL,
	PRIMARY KEY (id)
	
	#FOREIGN KEY (userid) REFERENCES usertbl(userid)
	#FOREIGN KEY (email)  REFERENCES usertbl(email) 
	#ON DELETE CASCADE 
	#ON UPDATE CASCADE
	
	
);

INSERT INTO `usertbl` (login, full_name, email, password, referal) VALUES 
("user1","user1","user1@g.com", "toor", ""), ("user2","user2","user2@g.com", "toor", ""),
("user3","user3","user3@g.com", "toor", ""), ("user4","user4","user4@g.com", "toor", ""),
("user5","user5","user5@g.com", "toor", ""), ("user6","user6","user6@g.com", "toor", ""),
("user7","user7","user7@g.com", "toor", ""), ("user8","user8","user8@g.com", "toor", "");

INSERT INTO `deposit` (userid, date, money, status) VALUES 
('1', "2017-12-15 00:00:00", '666', false), ('2', "2017-12-15 00:00:00", '666', false),
('3', "2017-12-15 00:00:00", '666', false), ('4', "2017-12-15 00:00:00", '666', false),
('5', "2017-12-15 00:00:00", '666', false), ('6', "2017-12-15 00:00:00", '666', false);

INSERT INTO `increasement`(`userid`,`date`, `time`, `email`, `amount`) VALUES 
('1', "2017-12-15 00:00:00", "2017-12-15 00:00:00", "user1@g.com", '121'),
('1', "2017-12-15 00:00:00", "2017-12-15 00:00:00", "user1@g.com", '121'),
('1', "2017-12-15 00:00:00", "2017-12-15 00:00:00", "user1@g.com", '121'),
('1', "2017-12-15 00:00:00", "2017-12-15 00:00:00", "user1@g.com", '121');
