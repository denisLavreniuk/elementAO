CREATE TABLE unit_info --Форма 2А (данные изделия/блока)
{
	id 				int NOT NULL AUTO_INCREMENT,
	num				int,						--номер блока
	release_date 	datetime,					--дата выпуска
	sw_version 		varchar(100),				--установленная версия ПО
	product_code 	varchar(100),				--шифр изделия
	reception_type	varchar(100),				--вид приемки (ОТК, ВП МОУ)
	category 		varchar(100),				--категория(статус) изделия, образца (для ЛКИб для эксплуатации, другое)
	*life_resource								--установленный ресурс	
	exploitation_place(id)						--место эксплуатации
	operating_hours int,						--наработка с начала эксплуатации	
	*failures(id)								--список отказов
	*performed_work(id)							--выполненные работы в ЭО
	*necessary_work(id)							--список необходимых работ
	*product_status_change(id)					--изменение категории(статуса изделия)
	remark			varchar(200)							--примечание
};
-----------------------------------------------------------------------------
CREATE TABLE life_resource --установленный ресурс
{
	id,
	varanty_storage_period,
	--time_resource для всех полей ниже
	*varanty(id), 			--гарантийный
	*before_first_repair(id)--до 1-го ремонта
	*between_repairs(id)	--межремонтный	
	*assigned(id)			--назначенный
	*tech_condition(id)		--по техническому состоянию, ограничения
};

CREATE TABLE time_resource --таблица для каждого из видов ресурсов
{
	id,
	period,			--срок
	operating_time	--наработка
};
------------------------------------------------------------------------------
CREATE TABLE exploitation_place	--Место эксплуатации
{
	id,
	name,			--название ЭО
	la_nummber,		--номер ЛА
	ad_number,		--номер АД(лев/прав)
	stand_name,		--стенд
};

CREATE TABLE engine_failure	--Отказы
{
	id,
	name,			--короткое название отказа
	datetime,		--дата отказа
	description,	--описание отказа
};
------------------------------------------------------------
CREATE TABLE performed_work (
  performed_work_id INT AUTO_INCREMENT PRIMARY KEY,
  unit_id INT,
  work_type ENUM('доработки','перепрограммрование','целевые осмотры','восстановительный ремонт'),
  name VARCHAR(100) NOT NULL,
  description VARCHAR(200) NOT NULL,
  FOREIGN KEY(unit_id) REFERENCES unit_info(unit_id)
);

------------------------------------------------------------
CREATE TABLE necessary_work	--необходимо выполнять
{
	work_id,
	name,			--наименование
	period,			--срок выполнения
	description		--описание
};

CREATE TABLE product_status_change	--Изменение категории (статуса) изделия
{
	change_id,
	name,			--название
	datetime,		--дата
	description		--основание
};