DROP TABLE IF EXISTS product_status_changes;
DROP TABLE IF EXISTS performed_works;
DROP TABLE IF EXISTS necessary_works;
DROP TABLE IF EXISTS exploit_place;

DROP TABLE IF EXISTS bef_first_repair_res;
DROP TABLE IF EXISTS refurbished_res;
DROP TABLE IF EXISTS assigned_res;
DROP TABLE IF EXISTS between_repairs_res;
DROP TABLE IF EXISTS varranty_res;
DROP TABLE IF EXISTS software;

DROP TABLE IF EXISTS failures;
DROP TABLE IF EXISTS unit_info;
DROP TABLE IF EXISTS usertbl;

CREATE TABLE unit_info
(
    unit_id 			INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    unit_num 			INT UNIQUE NOT NULL  COMMENT 'номер блока',
    release_date 		DATE NOT NULL COMMENT 'дата выпуска',
    product_code 		VARCHAR(50)	  COMMENT 'Шифр изделия',
    reception_type 		ENUM('ОТК','ВП МОУ') COMMENT 'вид приемки',
    var_stor_period		INT COMMENT 'гарантийный срок хранения',
    #--period_resources
    #--exploit_places_id
    operating_hours 	INT DEFAULT 0 COMMENT 'время наработки',
    #--failures
    #--performed_work
    #--necessary_work
    #--product_status_change
    remark 				VARCHAR(2000)  COMMENT 'примечание',
    last_Update			DATE NOT NULL,
    failures_count 		int DEFAULT 0,
    notes               VARCHAR(500);
);
#--Устновленный ресурс-----------------------------------------------------
CREATE TABLE varranty_res
(
    period_id			INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    unit_id				INT UNIQUE,
    period_value		INT NOT NULL COMMENT 'срок',
    operating_hours		INT 		 COMMENT 'наработка' DEFAULT 0,
    FOREIGN KEY(unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE TABLE bef_first_repair_res
(
    period_id			INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    unit_id				INT UNIQUE,
    period_value		INT NOT NULL COMMENT 'срок',
    operating_hours		INT 		 COMMENT 'наработка' DEFAULT 0,
    FOREIGN KEY(unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE TABLE between_repairs_res
(
    period_id			INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    unit_id				INT UNIQUE,
    period_value		INT NOT NULL COMMENT 'срок',
    operating_hours		INT 		 COMMENT 'наработка' DEFAULT 0,
    FOREIGN KEY(unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE TABLE assigned_res
(
    period_id			INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    unit_id				INT UNIQUE,
    period_value		INT NOT NULL COMMENT 'срок',
    operating_hours		INT 		 COMMENT 'наработка' DEFAULT 0,
    FOREIGN KEY(unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE TABLE refurbished_res
(
    period_id			INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    unit_id				INT UNIQUE,
    period_value		INT NOT NULL COMMENT 'срок',
    operating_hours		INT 		 COMMENT 'наработка' DEFAULT 0,
    FOREIGN KEY(unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);
#--Конец-------------------------------------------------------------------

CREATE TABLE performed_works
(
    performed_work_id 	INT AUTO_INCREMENT PRIMARY KEY,
    unit_id 			INT,
    work_type 			ENUM('доработки','перепрограммирование','целевые осмотры','восстановительный ремонт'),
    name 				VARCHAR(100) NOT NULL,
    description 		VARCHAR(200) NOT NULL,
    document			VARCHAR(200) NOT NULL COMMENT 'документ основания',
    start_date			DATE         NOT NULL COMMENT 'дата начала',
    finish_date			DATE         		  COMMENT 'дата завершения',
    FOREIGN KEY(unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

#--Место эксплуатации блока
CREATE TABLE exploit_place
(
    place_id			INT AUTO_INCREMENT PRIMARY KEY,
    unit_id				INT UNIQUE,
    place_name			VARCHAR(100) COMMENT 'название ЭО',
    la_number			VARCHAR(100) COMMENT 'номер ЛА',
    ad_number			VARCHAR(100) COMMENT 'номер АД',
    ad_side				ENUM('левый','правый') COMMENT 'место установки',
    stand_name			VARCHAR(100) COMMENT 'стенд',
    FOREIGN KEY(unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

#--Отказы для всех изделий(блоков)
CREATE TABLE failures
(
    failure_id			INT AUTO_INCREMENT NOT NULL PRIMARY KEY,
    unit_id				INT,
    time_stamp			TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT 'дата отказа',
    failure_name		VARCHAR(50) NOT NULL COMMENT 'короткое название отказа',
    description			VARCHAR(200) NOT NULL COMMENT 'описание отказ',
    document			VARCHAR(200) NOT NULL COMMENT 'документ основания',
    FOREIGN KEY(unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

#--Необходимо выполнить
CREATE TABLE necessary_works
(
    work_id				INT AUTO_INCREMENT NOT NULL PRIMARY KEY,
    unit_id				INT,
    period				DATE 		 NOT NULL COMMENT 'срок выполнения',
    work_name			VARCHAR(50)  NOT NULL COMMENT 'короткое наименование',
    description			VARCHAR(100) NOT NULL COMMENT 'описание',
    FOREIGN KEY(unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

#--Изменение категории (статуса) изделия
CREATE TABLE product_status_changes
(
    change_id			INT AUTO_INCREMENT NOT NULL PRIMARY KEY,
    unit_id				INT,
    $date				DATE			   NOT NULL,
    change_name			VARCHAR(50)	 	   NOT NULL,
    description			VARCHAR(100),
    FOREIGN KEY (unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE

);



CREATE TABLE usertbl
(
    user_id				INT AUTO_INCREMENT PRIMARY KEY,
    login				VARCHAR(100) NOT NULL,
    password			VARCHAR(100) NOT NULL
);

CREATE TABLE software
(
    sw_id				INT AUTO_INCREMENT PRIMARY KEY,
    unit_id				INT,
    sw_version			VARCHAR(100)	NOT NULL COMMENT 'Версия ПО',
    dev_goal			VARCHAR(200)	DEFAULT ""  COMMENT 'Цель разработки',
    implem_date			DATE			COMMENT 'Дата внедрения',
    cancell_date		DATE			COMMENT 'Дата аннулирования',
    FOREIGN KEY (unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE TABLE Users
(
    user_id 			INT AUTO_INCREMENT PRIMARY KEY,
    name 				VARCHAR(100)	NOT NULL
 );

CREATE TABLE repairs
(
    repair_id 			INT AUTO_INCREMENT PRIMARY KEY,
    unit_id				INT,
    r_number			INT 			NOT NULL,
    r_type				VARCHAR(100)	NOT NULL,
    r_date				DATE 			NOT NULL,
    place 				VARCHAR(100)	NOT NULL,
    FOREIGN KEY (unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
)

CREATE TABLE deviations
(
    deviation_id 		INT AUTO_INCREMENT PRIMARY KEY,
    unit_id				INT,
    deviation_type		VARCHAR(255)	NOT NULL,
    FOREIGN KEY (unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
)

CREATE TABLE contracts
(
    contract_id         INT AUTO_INCREMENT PRIMARY KEY,
    unit_id             INT,
    contract_num        VARCHAR(255) NOT NULL,
    contract_date       DATE  NOT NULL,
    associate           VARCHAR(255) NOT NULL,
    director_decision   VARCHAR(255) NOT NULL,
    FOREIGN KEY (unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
)

CREATE TABLE unit_dispatches
(
    dispatch_id         INT AUTO_INCREMENT PRIMARY KEY,         
    unit_id             INT NOT NULL,
    date_getting        DATE,
    date_sending        DATE,
    invoice             VARCHAR(500),
    FOREIGN KEY (unit_id) REFERENCES unit_info(unit_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
)

CREATE TABLE journal_log
(
    record_id           INT AUTO_INCREMENT PRIMARY KEY,         
    date_time           DATE,
    user                VARCHAR(255),
    operation           VARCHAR(255),
    num_code            VARCHAR(255),
    property            VARCHAR(500),
    old_value           VARCHAR(2000),
    new_value           VARCHAR(2000)
)

create DATABASE element_templates;

CREATE TABLE unit_names
(
    name_id				INT AUTO_INCREMENT PRIMARY KEY,
    name                VARCHAR(255) NOT NULL
)

CREATE TABLE variant_names
(
    variant_id          INT AUTO_INCREMENT PRIMARY KEY,
    name_id             INT NOT NULL,
    variant_name        VARCHAR(255) NOT NULL,
    FOREIGN KEY (name_id) REFERENCES unit_names(name_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
)

CREATE TABLE software_names
(
    software_id         INT AUTO_INCREMENT PRIMARY KEY,
    variant_id          INT NOT NULL,
    software_name       VARCHAR(255) NOT NULL,
    FOREIGN KEY (variant_id) REFERENCES variant_names(variant_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
)

CREATE TABLE unit_types
(
    type_id             INT AUTO_INCREMENT PRIMARY KEY,
    type_name           VARCHAR(255) NOT NULL
)

CREATE FUNCTION setFailuresCount (id INT)
RETURNS int
BEGIN
    DECLARE ret int;
    set ret = (SELECT COUNT(*)
    FROM failures
    WHERE unit_id = id);
    RETURN ret;
END;

CREATE TRIGGER `failureInserted`
AFTER INSERT ON `failures`
FOR EACH ROW BEGIN
UPDATE unit_info
SET unit_info.failures_count = setFailuresCount(NEW.unit_id)
WHERE unit_info.unit_id = NEW.unit_id;
END;

CREATE TRIGGER `failureDeleted`
AFTER DELETE ON `failures`
FOR EACH ROW BEGIN
UPDATE unit_info
set unit_info.failures_count = setFailuresCount(OLD.unit_id)
WHERE unit_info.unit_id = OLD.unit_id;
END;

#--Тестовые данные
INSERT INTO unit_info (unit_num, release_date, product_code, reception_type, var_stor_period) VALUES
(1801, '2018-01-25', 'РДЦ-450М-350', 1, 20),
(1802, '2019-12-11', 'РДЦ-450', 2, 23),
(1803, '2020-05-18', 'БЗГ-450', 1, 12),
(1804, '2021-06-18', 'РДЦ-450М-С-500', 2, 18),
(1805, '2018-03-22', 'РДЦ-450М-430-Р', 2, 18);

INSERT INTO performed_works (unit_id, work_type, name, description, start_date, finish_date) VALUES
(1, 4, 'АХША-120123', 'Работа обыкновенная по ремонту', '2018-05-18','2018-05-18'),
(2, 1, 'АХША-123211', 'Доработка', '2018-05-18', '2018-05-18'),
(2, 3, 'АХША-120111', 'Целевой осмотр', '2018-05-18', '2018-05-18'),
(1, 2, 'АХША-120223', 'Перепрограммирование', '2018-05-18', '2018-05-18');

INSERT INTO exploit_place (place_name, unit_id, la_number, ad_side, stand_name) VALUES
('Ивченко', 1, 22556, 1, 'ИС-022'),
('МСБ ИП',  2, 22556, 2, 'ИС-022'),
('Каховка', 3, 22556, 1, 'ИС-022'),
('Лвов пол',4, 22556, 2, 'ИС-022');

INSERT INTO failures (unit_id, failure_name, description) VALUES
(1, 'Штопор', 'Отказ по линии ППРТ привел к штопору ЛА'),
(1, 'Взрыв', 'По причине КЗ в блоке произошел взрыв'),
(2, 'Возгорание', 'Короткое замыкание воспламенило блок'),
(3, 'Испуг', 'Инженер-наладчик получил испуг высокой тяжести'),
(3, 'Расстройство', 'Из-за волнения, у начальника смены закрутило живот'),
(4, 'Неудача №1', 'Долго искали причину отказа, оказалось что забыли включить питание');

INSERT INTO necessary_works (unit_id, work_name, period, description) VALUES
(1, 'Протираем платы', '2019-04-22','Нужно протереть платы блока с помощью изопропилового спирта'),
(4, 'Переклеить наклейки', '2020-01-01', 'Переклеить наклейки на 2020 год');

INSERT INTO product_status_changes (unit_id, change_name, $date, description) VALUES
(1,'Перекрасили блок', '2012-01-12', 'Перекрасили блок в пурпурный цвет и списали как Содомию'),
(4,'Прапорщик сказал', '2013-05-08', 'Прапорщик Шматко Н.И. проинициировал смену статуса на полку под ведро'),
(2,'Тараканий дом', '2014-02-11', 'Смена статуса блока на "тараканий дом"');

INSERT INTO usertbl (login, password) VALUES
('alex', 'toor'),
('root', 'toor');

INSERT INTO software (unit_id, sw_version, dev_goal, implem_date, cancell_date) VALUES
(1, 'АХША.67000-05.01 ПО', 'корректировка по п.4 Решения №Р7541318,35,1162', '2018-01-12','2020-08-20'),
(2, 'АХША.67000-05.02 ПО', 'дополнение по п.3 Решения №Р7541319,35,1162', '2018-02-12','2020-07-20'),
(3, 'АХША.67000-05.03 ПО', 'корректировка по п.2 Решения №Р7541317,35,1162', '2018-03-12','2020-06-20'),
(4, 'АХША.67000-05.04 ПО', 'доработка по п.1 Решения №Р7541316,35,1162', '2018-04-12','2020-05-20'),
(1, 'АХША.67000-05.05 ПО', 'изменение по п.0 Решения №Р7541315,35,1162', '2018-05-12','2020-04-20'),
(2, 'АХША.67000-05.06 ПО', 'отладка по п.3 Решения №Р7541314,35,1162', '2018-06-12','2020-03-20'),
(3, 'АХША.67000-05.07 ПО', 'корректировка по п.3 Решения №Р7541313,35,1162', '2018-07-12','2020-02-20'),
(4, 'АХША.67000-05.08 ПО', 'дополнение по п.3 Решения №Р7541312,35,1162', '2018-08-12','2020-01-20');


INSERT INTO varranty_res			(unit_id, period_value) VALUES (1, 5),(2, 20),(3,120),(4, 220);
INSERT INTO bef_first_repair_res	(unit_id, period_value) VALUES (1, 5),(2, 20),(3,120),(4, 220);
INSERT INTO between_repairs_res		(unit_id, period_value) VALUES (1, 5),(2, 20),(3,120),(4, 220);
INSERT INTO assigned_res			(unit_id, period_value) VALUES (1, 5),(2, 20),(3,120),(4, 220);
INSERT INTO refurbished_res			(unit_id, period_value) VALUES (1, 5),(2, 20),(3,120),(4, 220);

INSERT INTO unit_names (name) VALUES
("РДЦ-450М"), ("РДЦ-450М-С"), ("РДЦ-450М-В"), ("РДЦ-450С-500"),
("РДЦ-450М-СР-2"), ("РДЦ-450М-С-Т-Р"), ("РДЦ-450М-С-1");

INSERT INTO variant_names (name_id, variant_name) VALUES
(1, "400-Р"), (1, "430-Р"), (1, "465-Р"),
(2, "400МВРР-Р"), (2, "400МВМ-Р"), (2, "450МВРР-Р"), (2, "450МВМ-Р"),
(3, "430-Р"), (3, "465-Р"),
(4, "1100Р-Р"), (4, "1100М-Р"), (4, "950Р-Р"), (4, "950М-Р"),
(5, "В1-Р"), (5, "В2-Р"), (5, "В3-Р"), (5, "В4-Р"),
(6, " "),
(7, "CP-1-P"), (7, "CD-1-P"), (7, "CM-1-P"), (7, "C-1-P");

INSERT INTO software_names (variant_id, software_name) VALUES
(1, "АХША.67000-03.01"), (1, "АХША.67000-05.01"),
(2, "АХША.67000-03.02"), (2, "АХША.67000-05.02"),
(3, "АХША.67000-03.03"), (3, "АХША.67000-05.03"),
(4, "15600-01.01"),
(5, "15600-01.02"),
(6, "15600-01.03"),
(7, "15600-01.04"),
(8, "16200-01.01"),
(9, "16200-01.02"),
(10, "17100-03.01"),
(11, "17100-03.02"),
(12, "17100-03.03"),
(13, "17100-03.04"),
(14, "17800-01.01"),
(15, "17800-01.02"),
(16, "17800-01.03"),
(17, "17800-01.04"),
(18, "19600-01"),
(19, "20500-01.01"),
(20, "20500-01.02"),
(21, "20500-01.03"),
(22, "20500-01.04");

INSERT INTO unit_types (type_id, type_name) VALUES
(1, "РДЦ-450"),
(2, "БЗГ-450"),
(3, "БРТ"),
(4, "АМК"),
(5, "КМУ"),
(6, "СКУП-1М"),
(7, "СИД-3"),
(8, "КПА-450");






/*
select * from unit_info;
select * from performed_works;
select * from period_resources;
select * from exploit_place;
select * from failures;
select * from necessary_works;
select * from product_status_changes;
*/
